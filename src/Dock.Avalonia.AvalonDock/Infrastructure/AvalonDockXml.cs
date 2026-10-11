using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Explicit codecs keep persistence independent of reflection and CLR type names.
internal static class AvalonDockXml
{
    internal static XElement Write(LayoutRoot layout)
    {
        XDocument document = new();
        using (XmlWriter writer = document.CreateWriter())
        {
            writer.WriteStartElement("LayoutRoot");
            layout.WriteXml(writer);
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
        EnsureId(scope);
        foreach (ILayoutElement child in scope.Descendents()) EnsureId(child);

        void EnsureId(ILayoutElement child)
        {
            if (child is not LayoutElement element || child is not (ILayoutPositionableGroup or LayoutAnchorGroup)) return;
            // Native pane copies may share an Id. References in AvalonDock XML
            // must identify one return container, including across repeat saves.
            if (!string.IsNullOrEmpty(element.DockModel.Id) && containerIds.Add(element.DockModel.Id)) return;
            string id;
            do { id = Guid.NewGuid().ToString("N"); } while (!containerIds.Add(id));
            element.DockModel.Id = id;
        }
    }

    internal static LayoutRoot Read(XElement xml)
    {
        if (xml.Name != "LayoutRoot") throw new XmlException("Expected an AvalonDock LayoutRoot element.");
        LayoutRoot root = new();
        bool hasPanel = false;
        List<LayoutContent> selected = new();
        LayoutContent? active = null;
        foreach (XElement child in xml.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "RootPanel":
                    if (hasPanel) throw new XmlException("A layout can contain only one RootPanel.");
                    root.RootPanel = (LayoutPanel)ReadElement(child, selected, ref active);
                    hasPanel = true;
                    break;
                case "Hidden":
                    foreach (XElement hidden in child.Elements()) root.Hidden.Add((LayoutAnchorable)ReadElement(hidden, selected, ref active));
                    break;
                case "TopSide": root.TopSide = (LayoutAnchorSide)ReadElement(child, selected, ref active); break;
                case "RightSide": root.RightSide = (LayoutAnchorSide)ReadElement(child, selected, ref active); break;
                case "LeftSide": root.LeftSide = (LayoutAnchorSide)ReadElement(child, selected, ref active); break;
                case "BottomSide": root.BottomSide = (LayoutAnchorSide)ReadElement(child, selected, ref active); break;
                case "FloatingWindows":
                    foreach (XElement window in child.Elements())
                    {
                        if (ReadElement(window, selected, ref active) is not LayoutFloatingWindow floating)
                            throw new XmlException("FloatingWindows can contain only floating layouts.");
                        root.FloatingWindows.Add(floating);
                    }
                    break;
                default: throw new XmlException($"Unknown layout element '{child.Name}'.");
            }
        }
        if (!hasPanel) throw new XmlException("A layout must contain a RootPanel.");
        RestoreSelection(selected);
        if (active is not null) root.ActiveContent = active;
        return root;
    }

    private static void WriteElement(LayoutElement element, XmlWriter writer, string? name = null)
    {
        if (element is LayoutFloatingWindow window) ValidateFloatingWindow(window);
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
            if (string.IsNullOrEmpty(element.DockModel.Id)) element.DockModel.Id = Guid.NewGuid().ToString("N");
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
            if (content.ToolTip is string tip) WriteAttribute(writer, "ToolTip", tip);
            if (content.LastActivationTimeStamp is { } time) WriteAttribute(writer, "LastActivationTimeStamp", time.ToString(CultureInfo.InvariantCulture));
            var previous = (content.PreviousContainer as LayoutElement)?.DockModel ?? content.DockModel.OriginalOwner;
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
            WriteAttribute(writer, "Id", anchorGroup.DockModel.Id);
            WriteAttribute(writer, "PreviousContainerId", (anchorGroup.PreviousContainer as LayoutElement)?.DockModel.Id);
        }
        if (element is ILayoutContainer container)
            foreach (ILayoutElement child in container.Children) WriteElement((LayoutElement)child, writer);
    }

    private static LayoutElement ReadElement(XElement xml, List<LayoutContent> selected, ref LayoutContent? active)
    {
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
            "LayoutAnchorSide" or "TopSide" or "RightSide" or "LeftSide" or "BottomSide" => new LayoutAnchorSide(),
            _ => throw new XmlException($"Unknown AvalonDock element '{xml.Name}'.")
        };
        ReadInto(element, xml, selected, ref active);
        return element;
    }

    private static void ReadInto(LayoutElement element, XElement xml, List<LayoutContent> selected, ref LayoutContent? active)
    {
        if (element is ILayoutOrientableGroup orientable && xml.Attribute("Orientation") is { } direction)
        {
            orientable.Orientation = direction.Value switch
            {
                "Horizontal" => Orientation.Horizontal,
                "Vertical" => Orientation.Vertical,
                _ => throw new XmlException($"Invalid orientation '{direction.Value}'.")
            };
        }
        if (element is ILayoutPositionableGroup position)
        {
            if (xml.Attribute("Id") is { } id) element.DockModel.Id = id.Value;
            position.RestoreDockLengths(
                xml.Attribute("DockWidth") is { } width ? GridLength.Parse(width.Value) : position.DockWidth,
                xml.Attribute("DockHeight") is { } height ? GridLength.Parse(height.Value) : position.DockHeight);
            position.DockMinWidth = Number(xml, "DockMinWidth", position.DockMinWidth);
            position.DockMinHeight = Number(xml, "DockMinHeight", position.DockMinHeight);
            position.AllowDuplicateContent = Boolean(xml, "AllowDuplicateContent", position.AllowDuplicateContent);
            position.CanRepositionItems = Boolean(xml, "CanRepositionItems", position.CanRepositionItems);
        }
        if (element is ILayoutElementForFloatingWindow floating)
        {
            floating.FloatingWidth = Number(xml, "FloatingWidth", floating.FloatingWidth);
            floating.FloatingHeight = Number(xml, "FloatingHeight", floating.FloatingHeight);
            floating.FloatingLeft = Number(xml, "FloatingLeft", floating.FloatingLeft);
            floating.FloatingTop = Number(xml, "FloatingTop", floating.FloatingTop);
            floating.IsMaximized = Boolean(xml, "IsMaximized", floating.IsMaximized);
        }
        if (element is LayoutContent content)
        {
            if (xml.Attribute("Title") is { } title) content.Title = title.Value;
            if (xml.Attribute("ContentId") is { } contentId) content.ContentId = contentId.Value;
            content.CanClose = Boolean(xml, "CanClose", content.CanClose);
            content.CanFloat = Boolean(xml, "CanFloat", content.CanFloat);
            if (xml.Attribute("ToolTip") is { } tip) content.ToolTip = tip.Value;
            if (xml.Attribute("PreviousContainerId") is { } previousId) content.PreviousContainerId = previousId.Value;
            if (xml.Attribute("PreviousContainerIndex") is { } previousIndex) content.PreviousContainerIndex = (int)previousIndex;
            if (xml.Attribute("InitialContainerId") is { } initialId) content.InitialContainerId = initialId.Value;
            if (xml.Attribute("InitialContainerIndex") is { } initialIndex) content.InitialContainerIndex = (int)initialIndex;
            if (xml.Attribute("LastActivationTimeStamp") is { } time) content.LastActivationTimeStamp = DateTime.Parse(time.Value, CultureInfo.InvariantCulture);
            if (Boolean(xml, "IsSelected", false)) selected.Add(content);
            else if (xml.Attribute("IsSelected") is not null) content.IsSelected = false;
            if (Boolean(xml, "IsLastFocusedDocument", false)) active = content;
        }
        if (element is LayoutDocument document)
        {
            if (xml.Attribute("Description") is { } description) document.Description = description.Value;
            document.CanMove = Boolean(xml, "CanMove", document.CanMove);
        }
        if (element is LayoutAnchorable tool)
        {
            tool.CanHide = Boolean(xml, "CanHide", tool.CanHide);
            tool.CanAutoHide = Boolean(xml, "CanAutoHide", tool.CanAutoHide);
            tool.CanDockAsTabbedDocument = Boolean(xml, "CanDockAsTabbedDocument", tool.CanDockAsTabbedDocument);
            tool.AutoHideWidth = Number(xml, "AutoHideWidth", tool.AutoHideWidth);
            tool.AutoHideHeight = Number(xml, "AutoHideHeight", tool.AutoHideHeight);
            tool.AutoHideMinWidth = Number(xml, "AutoHideMinWidth", tool.AutoHideMinWidth);
            tool.AutoHideMinHeight = Number(xml, "AutoHideMinHeight", tool.AutoHideMinHeight);
        }
        if (element is LayoutDocumentPane documents) documents.ShowHeader = Boolean(xml, "ShowHeader", documents.ShowHeader);
        if (element is LayoutAnchorablePane tools && xml.Attribute("Name") is { } name) tools.Name = name.Value;
        if (element is LayoutAnchorGroup anchorGroup)
        {
            if (xml.Attribute("Id") is { } groupId) anchorGroup.DockModel.Id = groupId.Value;
            if (xml.Attribute("PreviousContainerId") is { } previousId) anchorGroup.PreviousContainerId = previousId.Value;
        }
        foreach (XElement child in xml.Elements())
        {
            LayoutElement childElement = ReadElement(child, selected, ref active);
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
    private static void WriteAttribute(XmlWriter writer, string name, int value) =>
        writer.WriteAttributeString(name, XmlConvert.ToString(value));
    private static void WriteAttribute(XmlWriter writer, string name, double value) =>
        writer.WriteAttributeString(name, XmlConvert.ToString(value));

    internal static void ReadContents(LayoutElement element, XmlReader reader)
    {
        XElement xml = ReadNode(reader);
        List<LayoutContent> selected = new();
        LayoutContent? active = null;
        ReadInto(element, xml, selected, ref active);
        RestoreSelection(selected);
        if (active is not null && element.Root is { } root) root.ActiveContent = active;
    }

    internal static LayoutRoot? ReadRoot(XmlReader reader)
    {
        XElement xml = ReadNode(reader);
        if (xml.IsEmpty) return null;
        // IXmlSerializable callers own the surrounding element name.
        xml.Name = "LayoutRoot";
        return Read(xml);
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
        if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != expectedName)
            throw new XmlException($"Expected a {expectedName} element.");
        XElement xml = (XElement)XNode.ReadFrom(reader);
        List<LayoutContent> selected = new();
        LayoutContent? active = null;
        LayoutFloatingWindow window = (LayoutFloatingWindow)ReadElement(xml, selected, ref active);
        RestoreSelection(selected);
        return window;
    }

    private static void RestoreSelection(List<LayoutContent> selected)
    {
        foreach (LayoutContent content in selected)
        {
            if (content.Parent is ILayoutContentSelector selector && content.Parent is ILayoutContainer container)
            {
                selector.SelectedContentIndex = selector.IndexOf(content);
                // Standalone XML models have no factory to update sibling state.
                foreach (ILayoutElement sibling in container.Children)
                    if (sibling is LayoutContent other && !ReferenceEquals(other, content)) other.IsSelected = false;
            }
            content.IsSelected = true;
        }
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

    private static bool Boolean(XElement element, string name, bool fallback) =>
        element.Attribute(name) is { } value ? bool.Parse(value.Value) : fallback;
    private static string FormatLength(GridLength value) => value.IsAuto ? "Auto" :
        value.Value.ToString(CultureInfo.InvariantCulture) + (value.IsStar ? "*" : string.Empty);
    private static double Number(XElement element, string name, double fallback) =>
        element.Attribute(name) is { } value ? double.Parse(value.Value, CultureInfo.InvariantCulture) : fallback;
}
