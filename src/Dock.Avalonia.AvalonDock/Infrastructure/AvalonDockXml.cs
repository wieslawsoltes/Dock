using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
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
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) window.DockWindow.Save();
        EnsureContainerIds(layout);
        XElement root = new("LayoutRoot");
        XElement panel = WriteElement(layout.RootPanel);
        panel.Name = "RootPanel";
        root.Add(panel);
        root.Add(WriteSide(layout.TopSide, "TopSide"), WriteSide(layout.RightSide, "RightSide"),
            WriteSide(layout.LeftSide, "LeftSide"), WriteSide(layout.BottomSide, "BottomSide"));
        XElement floating = new("FloatingWindows");
        foreach (LayoutFloatingWindow window in layout.FloatingWindows) floating.Add(WriteElement(window));
        root.Add(floating);
        XElement hidden = new("Hidden");
        foreach (LayoutAnchorable content in layout.Hidden) hidden.Add(WriteElement(content));
        root.Add(hidden);
        return root;
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

    private static XElement WriteSide(LayoutAnchorSide side, string name)
    {
        XElement xml = WriteElement(side);
        xml.Name = name;
        return xml;
    }

    private static XElement WriteElement(LayoutElement element)
    {
        if (element is LayoutFloatingWindow window) ValidateFloatingWindow(window);
        XElement xml = new(element switch
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
        });
        if (element is ILayoutOrientableGroup orientable) xml.SetAttributeValue("Orientation", orientable.Orientation);
        if (element is ILayoutPositionableGroup position)
        {
            if (string.IsNullOrEmpty(element.DockModel.Id)) element.DockModel.Id = Guid.NewGuid().ToString("N");
            xml.SetAttributeValue("Id", element.DockModel.Id);
            xml.SetAttributeValue("DockWidth", FormatLength(position.DockWidth));
            xml.SetAttributeValue("DockHeight", FormatLength(position.DockHeight));
            xml.SetAttributeValue("DockMinWidth", position.DockMinWidth);
            xml.SetAttributeValue("DockMinHeight", position.DockMinHeight);
            xml.SetAttributeValue("AllowDuplicateContent", position.AllowDuplicateContent);
            xml.SetAttributeValue("CanRepositionItems", position.CanRepositionItems);
        }
        if (element is ILayoutElementForFloatingWindow floating)
        {
            xml.SetAttributeValue("FloatingWidth", floating.FloatingWidth);
            xml.SetAttributeValue("FloatingHeight", floating.FloatingHeight);
            xml.SetAttributeValue("FloatingLeft", floating.FloatingLeft);
            xml.SetAttributeValue("FloatingTop", floating.FloatingTop);
            xml.SetAttributeValue("IsMaximized", floating.IsMaximized);
        }
        if (element is LayoutContent content)
        {
            xml.SetAttributeValue("Title", content.Title);
            xml.SetAttributeValue("ContentId", content.ContentId);
            xml.SetAttributeValue("CanClose", content.CanClose);
            xml.SetAttributeValue("CanFloat", content.CanFloat);
            xml.SetAttributeValue("IsSelected", content.IsSelected);
            xml.SetAttributeValue("IsLastFocusedDocument", content.IsLastFocusedDocument);
            if (content.ToolTip is string tip) xml.SetAttributeValue("ToolTip", tip);
            if (content.LastActivationTimeStamp is { } time) xml.SetAttributeValue("LastActivationTimeStamp", time.ToString(CultureInfo.InvariantCulture));
            var previous = (content.PreviousContainer as LayoutElement)?.DockModel ?? content.DockModel.OriginalOwner;
            if (previous is not null)
            {
                xml.SetAttributeValue("PreviousContainerId", previous.Id);
                xml.SetAttributeValue("PreviousContainerIndex", content.PreviousContainerIndex);
            }
            if (content.InitialContainer is LayoutElement initial)
            {
                xml.SetAttributeValue("InitialContainerId", initial.DockModel.Id);
                xml.SetAttributeValue("InitialContainerIndex", content.InitialContainerIndex);
            }
        }
        if (element is LayoutDocument document)
        {
            xml.SetAttributeValue("Description", document.Description);
            xml.SetAttributeValue("CanMove", document.CanMove);
        }
        if (element is LayoutAnchorable tool)
        {
            xml.SetAttributeValue("CanHide", tool.CanHide);
            xml.SetAttributeValue("CanAutoHide", tool.CanAutoHide);
            xml.SetAttributeValue("CanDockAsTabbedDocument", tool.CanDockAsTabbedDocument);
            xml.SetAttributeValue("AutoHideWidth", tool.AutoHideWidth);
            xml.SetAttributeValue("AutoHideHeight", tool.AutoHideHeight);
            xml.SetAttributeValue("AutoHideMinWidth", tool.AutoHideMinWidth);
            xml.SetAttributeValue("AutoHideMinHeight", tool.AutoHideMinHeight);
        }
        if (element is LayoutDocumentPane documents) xml.SetAttributeValue("ShowHeader", documents.ShowHeader);
        if (element is LayoutAnchorablePane tools) xml.SetAttributeValue("Name", tools.Name);
        if (element is LayoutAnchorGroup anchorGroup)
        {
            xml.SetAttributeValue("Id", anchorGroup.DockModel.Id);
            xml.SetAttributeValue("PreviousContainerId", (anchorGroup.PreviousContainer as LayoutElement)?.DockModel.Id);
        }
        if (element is ILayoutContainer container)
            foreach (ILayoutElement child in container.Children) xml.Add(WriteElement((LayoutElement)child));
        return xml;
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
        ArgumentNullException.ThrowIfNull(writer);
        XElement xml;
        if (element is LayoutRoot root) xml = Write(root);
        else
        {
            EnsureContainerIds(element.Root as LayoutElement ?? element);
            xml = WriteElement(element);
        }
        foreach (XAttribute attribute in xml.Attributes()) writer.WriteAttributeString(attribute.Name.LocalName, attribute.Value);
        foreach (XNode child in xml.Nodes()) child.WriteTo(writer);
    }

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
        ArgumentNullException.ThrowIfNull(writer);
        window.DockWindow.Save();
        XElement xml = WriteElement(window);
        foreach (XElement child in xml.Elements()) child.WriteTo(writer);
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
