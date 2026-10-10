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
        HashSet<string> containerIds = new(StringComparer.Ordinal);
        foreach (ILayoutElement child in layout.Descendents())
        {
            if (child is not LayoutElement element || child is not (ILayoutPositionableGroup or LayoutAnchorGroup)) continue;
            // Native pane copies may share an Id. References in AvalonDock XML
            // must identify one return container, including across repeat saves.
            if (!string.IsNullOrEmpty(element.DockModel.Id) && containerIds.Add(element.DockModel.Id)) continue;
            string id;
            do { id = Guid.NewGuid().ToString("N"); } while (!containerIds.Add(id));
            element.DockModel.Id = id;
        }
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
        foreach (LayoutContent content in selected) content.IsSelected = true;
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
            if (xml.Attribute("DockWidth") is { } width) position.DockWidth = GridLength.Parse(width.Value);
            if (xml.Attribute("DockHeight") is { } height) position.DockHeight = GridLength.Parse(height.Value);
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
            floating.IsMaximized = Boolean(xml, "IsMaximized", false);
        }
        if (element is LayoutContent content)
        {
            content.Title = (string?)xml.Attribute("Title") ?? string.Empty;
            content.ContentId = (string?)xml.Attribute("ContentId") ?? string.Empty;
            content.CanClose = Boolean(xml, "CanClose", content.CanClose);
            content.CanFloat = Boolean(xml, "CanFloat", content.CanFloat);
            content.ToolTip = (string?)xml.Attribute("ToolTip");
            content.PreviousContainerId = (string?)xml.Attribute("PreviousContainerId");
            content.PreviousContainerIndex = (int?)xml.Attribute("PreviousContainerIndex") ?? -1;
            content.InitialContainerId = (string?)xml.Attribute("InitialContainerId");
            content.InitialContainerIndex = (int?)xml.Attribute("InitialContainerIndex") ?? -1;
            if (xml.Attribute("LastActivationTimeStamp") is { } time) content.LastActivationTimeStamp = DateTime.Parse(time.Value, CultureInfo.InvariantCulture);
            if (Boolean(xml, "IsSelected", false)) selected.Add(content);
            if (Boolean(xml, "IsLastFocusedDocument", false)) active = content;
        }
        if (element is LayoutDocument document)
        {
            document.Description = (string?)xml.Attribute("Description");
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
            anchorGroup.DockModel.Id = (string?)xml.Attribute("Id") ?? string.Empty;
            anchorGroup.PreviousContainerId = (string?)xml.Attribute("PreviousContainerId");
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
        return element;
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
        foreach (LayoutContent content in selected) content.IsSelected = true;
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

    private static bool Boolean(XElement element, string name, bool fallback) =>
        element.Attribute(name) is { } value ? bool.Parse(value.Value) : fallback;
    private static string FormatLength(GridLength value) => value.IsAuto ? "Auto" :
        value.Value.ToString(CultureInfo.InvariantCulture) + (value.IsStar ? "*" : string.Empty);
    private static double Number(XElement element, string name, double fallback) =>
        element.Attribute(name) is { } value ? double.Parse(value.Value, CultureInfo.InvariantCulture) : fallback;
}
