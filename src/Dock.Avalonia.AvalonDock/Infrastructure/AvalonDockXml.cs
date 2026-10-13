using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace AvalonDock.Infrastructure;

// Explicit codecs keep persistence independent of reflection and CLR type names.
internal static class AvalonDockXml
{
    internal static XElement Write(LayoutRoot layout, LayoutSerializationTypes? types = null)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
        {
            writer.WriteStartElement("LayoutRoot");
            EnsureContainerIds(layout);
            layout.WriteXml(new LayoutXmlWriter(writer, types));
            writer.WriteEndElement();
        }
        return document.Root!;
    }

    private static void WriteRootContents(LayoutRoot layout, XmlWriter writer)
    {
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) window.DockWindow.Save();
        WriteElement(layout.RootPanel, writer, "RootPanel");
        WriteElement(layout.TopSide, writer, "TopSide");
        WriteElement(layout.RightSide, writer, "RightSide");
        WriteElement(layout.LeftSide, writer, "LeftSide");
        WriteElement(layout.BottomSide, writer, "BottomSide");
        writer.WriteStartElement("FloatingWindows");
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) WriteElement(window, writer);
        writer.WriteEndElement();
        writer.WriteStartElement("Hidden");
        foreach (LayoutAnchorable content in layout.Hidden) WriteElement(content, writer);
        writer.WriteEndElement();
    }

    private static void EnsureContainerIds(LayoutElement scope)
    {
        HashSet<string> containerIds = new(StringComparer.Ordinal);
        // Preserve pane IDs before assigning identifiers to native group
        // extensions, matching the precedence used while restoring references.
        if (LayoutContainerIdentity.IsPane(scope)) EnsureId(scope);
        foreach (ILayoutElement child in scope.Descendents())
            if (LayoutContainerIdentity.IsPane(child)) EnsureId(child);
        if (!LayoutContainerIdentity.IsPane(scope)) EnsureId(scope);
        foreach (ILayoutElement child in scope.Descendents())
            if (!LayoutContainerIdentity.IsPane(child)) EnsureId(child);

        void EnsureId(ILayoutElement child)
        {
            if (child is not LayoutElement element || child is not (ILayoutPositionableGroup or LayoutAnchorGroup)) return;
            // Native pane copies may share an Id. References in AvalonDock XML
            // must identify one return container, including across repeat saves.
            if (element.XmlId is { } current && containerIds.Add(current)) return;
            string id;
            do { id = Guid.NewGuid().ToString("N"); } while (!containerIds.Add(id));
            element.DockModel.Id = id;
        }
    }

    internal static LayoutRoot Read(XElement xml, LayoutSerializationTypes? types = null)
    {
        if (xml.Name != "LayoutRoot") throw new XmlException("Expected an AvalonDock LayoutRoot element.");
        LayoutRoot root = new();
        bool hasPanel = false;
        LayoutXmlReadContext context = new(types);
        foreach (XElement child in xml.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "RootPanel":
                    if (hasPanel) throw new XmlException("A layout can contain only one RootPanel.");
                    root.RootPanel = (LayoutPanel)ReadElement(child, context);
                    hasPanel = true;
                    break;
                case "Hidden":
                    foreach (XElement hidden in child.Elements()) root.Hidden.Add((LayoutAnchorable)ReadElement(hidden, context));
                    break;
                case "TopSide": root.TopSide = (LayoutAnchorSide)ReadElement(child, context); break;
                case "RightSide": root.RightSide = (LayoutAnchorSide)ReadElement(child, context); break;
                case "LeftSide": root.LeftSide = (LayoutAnchorSide)ReadElement(child, context); break;
                case "BottomSide": root.BottomSide = (LayoutAnchorSide)ReadElement(child, context); break;
                case "FloatingWindows":
                    foreach (XElement window in child.Elements())
                    {
                        if (ReadElement(window, context) is not LayoutFloatingWindow floating)
                            throw new XmlException("FloatingWindows can contain only floating layouts.");
                        root.FloatingWindows.Add(floating);
                    }
                    break;
                default: throw new XmlException($"Unknown layout element '{child.Name}'.");
            }
        }
        if (!hasPanel) throw new XmlException("A layout must contain a RootPanel.");
        return root;
    }

    private static void WriteElement(LayoutElement element, XmlWriter writer, string? name = null)
    {
        if (element is LayoutFloatingWindow window) ValidateFloatingWindow(window);
        name ??= (writer as LayoutXmlWriter)?.Types?.GetName(element);
        name ??= element switch
        {
            LayoutPanel => "LayoutPanel",
            LayoutDocumentPaneGroup => "LayoutDocumentPaneGroup",
            LayoutAnchorablePaneGroup => "LayoutAnchorablePaneGroup",
            LayoutDocumentPane => "LayoutDocumentPane",
            LayoutAnchorablePane => "LayoutAnchorablePane",
            LayoutDocument => "LayoutDocument",
            LayoutAnchorable => "LayoutAnchorable",
            LayoutAnchorGroup => "LayoutAnchorGroup",
            LayoutAnchorSide => "LayoutAnchorSide",
            LayoutDocumentFloatingWindow => "LayoutDocumentFloatingWindow",
            LayoutAnchorableFloatingWindow => "LayoutAnchorableFloatingWindow",
            LayoutGroupFloatingWindow => "LayoutGroupFloatingWindow",
            _ => throw new NotSupportedException("Unsupported AvalonDock layout node.")
        };
        writer.WriteStartElement(name);
        ((IXmlSerializable)element).WriteXml(writer);
        writer.WriteEndElement();
    }

    private static void WriteNodeContents(LayoutElement element, XmlWriter writer)
    {
        if (element is ILayoutOrientableGroup orientable) WriteAttribute(writer, "Orientation", orientable.Orientation.ToString());
        if (element is ILayoutPositionableGroup position)
        {
            if (element.XmlId is null) element.DockModel.Id = Guid.NewGuid().ToString("N");
            WriteAttribute(writer, "Id", element.DockModel.Id);
            WriteAttribute(writer, "DockWidth", FormatLength(position.DockWidth));
            WriteAttribute(writer, "DockHeight", FormatLength(position.DockHeight));
            WriteAttribute(writer, "DockMinWidth", position.DockMinWidth);
            WriteAttribute(writer, "DockMinHeight", position.DockMinHeight);
            WriteAttribute(writer, "AllowDuplicateContent", position.AllowDuplicateContent);
            WriteAttribute(writer, "CanRepositionItems", position.CanRepositionItems);
        }
        if (element is ILayoutElementForFloatingWindow floating)
        {
            WriteAttribute(writer, "FloatingWidth", floating.FloatingWidth);
            WriteAttribute(writer, "FloatingHeight", floating.FloatingHeight);
            WriteAttribute(writer, "FloatingLeft", floating.FloatingLeft);
            WriteAttribute(writer, "FloatingTop", floating.FloatingTop);
            WriteAttribute(writer, "IsMaximized", floating.IsMaximized);
        }
        if (element is LayoutContent content)
        {
            WriteAttribute(writer, "Title", content.Title);
            WriteAttribute(writer, "ContentId", content.ContentId);
            WriteAttribute(writer, "CanClose", content.CanClose);
            WriteAttribute(writer, "CanFloat", content.CanFloat);
            WriteAttribute(writer, "IsSelected", content.IsSelected);
            WriteAttribute(writer, "IsLastFocusedDocument", content.IsLastFocusedDocument);
            if (content.IsFloating) WriteAttribute(writer, "IsFloating", true);
            if (content.ToolTip is string tip) WriteAttribute(writer, "ToolTip", tip);
            if (content.LastActivationTimeStamp is { } time) WriteAttribute(writer, "LastActivationTimeStamp", time.ToString(CultureInfo.InvariantCulture));
            var previous = (content.PreviousContainer as LayoutElement)?.DockModel;
            if (previous is not null)
            {
                WriteAttribute(writer, "PreviousContainerId", previous.Id);
                WriteAttribute(writer, "PreviousContainerIndex", content.PreviousContainerIndex);
            }
            if (content.InitialContainer is LayoutElement initial)
            {
                WriteAttribute(writer, "InitialContainerId", initial.DockModel.Id);
                WriteAttribute(writer, "InitialContainerIndex", content.InitialContainerIndex);
            }
        }
        if (element is LayoutDocument document)
        {
            WriteAttribute(writer, "Description", document.Description);
            WriteAttribute(writer, "CanMove", document.CanMove);
        }
        if (element is LayoutAnchorable tool)
        {
            WriteAttribute(writer, "CanHide", tool.CanHide);
            WriteAttribute(writer, "CanAutoHide", tool.CanAutoHide);
            WriteAttribute(writer, "CanDockAsTabbedDocument", tool.CanDockAsTabbedDocument);
            WriteAttribute(writer, "AutoHideWidth", tool.AutoHideWidth);
            WriteAttribute(writer, "AutoHideHeight", tool.AutoHideHeight);
            WriteAttribute(writer, "AutoHideMinWidth", tool.AutoHideMinWidth);
            WriteAttribute(writer, "AutoHideMinHeight", tool.AutoHideMinHeight);
        }
        if (element is LayoutDocumentPane documents) WriteAttribute(writer, "ShowHeader", documents.ShowHeader);
        if (element is LayoutAnchorablePane tools) WriteAttribute(writer, "Name", tools.Name);
        if (element is LayoutAnchorGroup anchorGroup)
        {
            WriteAttribute(writer, "Id", anchorGroup.XmlId);
            WriteAttribute(writer, "PreviousContainerId", (anchorGroup.PreviousContainer as LayoutElement)?.DockModel.Id);
        }
        if (element is ILayoutContainer container)
            foreach (ILayoutElement child in container.Children) WriteElement((LayoutElement)child, writer);
    }

    private static LayoutElement ReadElement(XElement xml, LayoutXmlReadContext context)
    {
        if (context.Types?.TryCreate(xml.Name.LocalName, out LayoutElement? custom) == true)
        {
            using XmlReader source = xml.CreateReader();
            using LayoutXmlReader reader = new(source, context);
            reader.MoveToContent();
            ((IXmlSerializable)custom!).ReadXml(reader);
            return custom!;
        }
        LayoutElement element = xml.Name.LocalName switch
        {
            "RootPanel" or "LayoutPanel" => new LayoutPanel(),
            "LayoutDocumentPaneGroup" => new LayoutDocumentPaneGroup(),
            "LayoutAnchorablePaneGroup" => new LayoutAnchorablePaneGroup(),
            "LayoutDocumentPane" => new LayoutDocumentPane(),
            "LayoutAnchorablePane" => new LayoutAnchorablePane(),
            "LayoutDocument" => new LayoutDocument(),
            "LayoutAnchorable" => new LayoutAnchorable(),
            "LayoutAnchorGroup" => new LayoutAnchorGroup(),
            "LayoutDocumentFloatingWindow" => new LayoutDocumentFloatingWindow(),
            "LayoutAnchorableFloatingWindow" => new LayoutAnchorableFloatingWindow(),
            "LayoutGroupFloatingWindow" => new LayoutGroupFloatingWindow(),
            "LayoutAnchorSide" or "TopSide" or "RightSide" or "LeftSide" or "BottomSide" => new LayoutAnchorSide(),
            _ => throw new XmlException($"Unknown AvalonDock element '{xml.Name}'.")
        };
        ReadInto(element, xml, context);
        if (xml.IsEmpty && element is LayoutAnchorableFloatingWindow floating)
            ((ILayoutElementWithVisibility)floating).ComputeVisibility();
        return element;
    }

    private static void ReadInto(LayoutElement element, XElement xml, LayoutXmlReadContext context)
    {
        if (element is LayoutContent content)
        {
            using XmlReader reader = xml.CreateReader();
            content.ReadXml(reader);
            return;
        }
        if (element is ILayoutGroup) LayoutGroupXml.ReadMetadata(element, xml);
        if (xml.IsEmpty && element is ILayoutGroup && element is ILayoutElementWithVisibility visibility)
            visibility.ComputeVisibility();
        foreach (XElement child in xml.Elements())
        {
            LayoutElement childElement = ReadElement(child, context);
            switch (element)
            {
                case LayoutDocumentFloatingWindow { RootDocument: null } documentWindow when childElement is LayoutDocument rootDocument:
                    documentWindow.RootDocument = rootDocument;
                    break;
                case LayoutAnchorableFloatingWindow { RootPanel: null } toolWindow when childElement is LayoutAnchorablePaneGroup panel:
                    toolWindow.RootPanel = panel;
                    break;
                case ILayoutGroup group:
                    group.InsertChildAt(group.ChildrenCount, childElement);
                    break;
                default: throw new XmlException($"Invalid child in '{xml.Name}'.");
            }
        }
        if (element is LayoutAnchorablePane tools) tools.CompleteXmlRead();
    }

    internal static void WriteContents(LayoutElement element, XmlWriter writer)
    {
        writer = PrepareWriter(element, writer);
        if (element is LayoutRoot root) WriteRootContents(root, writer);
        else WriteNodeContents(element, writer);
    }

    private static XmlWriter PrepareWriter(LayoutElement element, XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (writer is LayoutXmlWriter) return writer;
        EnsureContainerIds(element.Root as LayoutElement ?? element);
        return new LayoutXmlWriter(writer);
    }

    private static void WriteAttribute(XmlWriter writer, string name, string? value)
    {
        if (value is not null) writer.WriteAttributeString(name, value);
    }
    private static void WriteAttribute(XmlWriter writer, string name, bool value) =>
        writer.WriteAttributeString(name, XmlConvert.ToString(value));
    // Match the CLR parsers used by upstream readers. XML Schema's INF spelling
    // is not accepted by those readers; return indices use the current culture.
    private static void WriteAttribute(XmlWriter writer, string name, int value) =>
        writer.WriteAttributeString(name, value.ToString(CultureInfo.CurrentCulture));
    private static void WriteAttribute(XmlWriter writer, string name, double value) =>
        writer.WriteAttributeString(name, value.ToString(CultureInfo.InvariantCulture));

    internal static void ReadContents(LayoutElement element, XmlReader reader)
    {
        XElement xml = ReadNode(reader);
        LayoutXmlReadContext context = (reader as LayoutXmlReader)?.Context ?? new();
        ReadInto(element, xml, context);
    }

    internal static LayoutRoot? ReadRoot(XmlReader reader)
    {
        XElement xml = ReadNode(reader);
        if (xml.IsEmpty) return null;
        // IXmlSerializable callers own the surrounding element name.
        xml.Name = "LayoutRoot";
        return Read(xml, (reader as LayoutXmlReader)?.Context.Types);
    }

    private static XElement ReadNode(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (reader.MoveToContent() != XmlNodeType.Element) throw new XmlException("Expected a layout element.");
        return (XElement)XNode.ReadFrom(reader);
    }

    internal static void WriteFloatingContents(LayoutFloatingWindow window, XmlWriter writer)
    {
        writer = PrepareWriter(window, writer);
        window.DockWindow.Save();
        ValidateFloatingWindow(window);
        foreach (ILayoutElement child in window.Children) WriteElement((LayoutElement)child, writer);
    }

    internal static LayoutFloatingWindow ReadFloatingWindow(XmlReader reader, string expectedName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (reader.MoveToContent() != XmlNodeType.Element || (reader is not LayoutXmlReader && reader.LocalName != expectedName))
            throw new XmlException($"Expected a {expectedName} element.");
        XElement xml = (XElement)XNode.ReadFrom(reader);
        LayoutXmlReadContext context = (reader as LayoutXmlReader)?.Context ?? new();
        LayoutFloatingWindow window = expectedName == nameof(LayoutDocumentFloatingWindow)
            ? new LayoutDocumentFloatingWindow() : new LayoutAnchorableFloatingWindow();
        ReadInto(window, xml, context);
        if (xml.IsEmpty && window is LayoutAnchorableFloatingWindow floating)
            ((ILayoutElementWithVisibility)floating).ComputeVisibility();
        return window;
    }

    private static void ValidateFloatingWindow(LayoutFloatingWindow window)
    {
        RootDock root = window.NativeRoot;
        if (root.HiddenDockables is { Count: > 0 } || root.LeftPinnedDockables is { Count: > 0 } ||
            root.TopPinnedDockables is { Count: > 0 } || root.RightPinnedDockables is { Count: > 0 } ||
            root.BottomPinnedDockables is { Count: > 0 } || root.Windows is { Count: > 0 })
            throw new NotSupportedException("The native floating window contains content outside the AvalonDock floating hierarchy.");
        if (window is LayoutDocumentFloatingWindow document)
        {
            if (root.VisibleDockables is not { Count: 1 } children || children[0] is not DocumentDock pane ||
                (pane.VisibleDockables?.Count ?? 0) != document.ChildrenCount ||
                (document.RootDocument is { } content && !ReferenceEquals(pane.VisibleDockables![0], content.DockModel)))
                throw new NotSupportedException("An AvalonDock document floating window can contain only its root document.");
        }
        else if (window is LayoutAnchorableFloatingWindow tools &&
                 ((root.VisibleDockables?.Count ?? 0) != tools.ChildrenCount ||
                  (tools.RootPanel is { } panel && !ReferenceEquals(root.VisibleDockables![0], panel.DockModel))))
            throw new NotSupportedException("An AvalonDock tool floating window can contain only its root pane group.");
    }

    private static string FormatLength(GridLength value) => value.IsAuto ? "Auto" :
        value.Value.ToString(CultureInfo.InvariantCulture) + (value.IsStar ? "*" : string.Empty);
}
