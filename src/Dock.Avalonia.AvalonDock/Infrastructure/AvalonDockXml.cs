using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

// Explicit codecs keep persistence independent of reflection and CLR type names.
internal static class AvalonDockXml
{
    internal static XElement Write(LayoutRoot layout)
    {
        var model = (Dock.Model.Avalonia.Controls.RootDock)layout.DockModel;
        if (model.LeftPinnedDockables is { Count: > 0 } || model.RightPinnedDockables is { Count: > 0 }
            || model.TopPinnedDockables is { Count: > 0 } || model.BottomPinnedDockables is { Count: > 0 })
            throw new NotSupportedException("Serialization of auto-hide groups is not implemented yet.");
        XElement root = new("LayoutRoot");
        XElement panel = WriteElement(layout.RootPanel);
        panel.Name = "RootPanel";
        root.Add(panel);
        root.Add(new XElement("TopSide"), new XElement("RightSide"), new XElement("LeftSide"), new XElement("BottomSide"));
        if (model.Windows is { Count: > 0 })
            throw new NotSupportedException("Serialization of floating windows is not implemented yet.");
        root.Add(new XElement("FloatingWindows"));
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
                case "TopSide": case "RightSide": case "LeftSide": case "BottomSide": case "FloatingWindows":
                    if (child.HasElements) throw new NotSupportedException($"Persistence of '{child.Name}' is not implemented yet.");
                    break;
                default: throw new XmlException($"Unknown layout element '{child.Name}'.");
            }
        }
        if (!hasPanel) throw new XmlException("A layout must contain a RootPanel.");
        foreach (LayoutContent content in selected) content.IsSelected = true;
        if (active is not null) root.ActiveContent = active;
        return root;
    }

    private static XElement WriteElement(LayoutElement element)
    {
        XElement xml = new(element switch
        {
            LayoutPanel => "LayoutPanel",
            LayoutDocumentPaneGroup => "LayoutDocumentPaneGroup",
            LayoutAnchorablePaneGroup => "LayoutAnchorablePaneGroup",
            LayoutDocumentPane => "LayoutDocumentPane",
            LayoutAnchorablePane => "LayoutAnchorablePane",
            LayoutDocument => "LayoutDocument",
            LayoutAnchorable => "LayoutAnchorable",
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
        if (element is LayoutContent content)
        {
            xml.SetAttributeValue("Title", content.Title);
            xml.SetAttributeValue("ContentId", content.ContentId);
            xml.SetAttributeValue("CanClose", content.CanClose);
            xml.SetAttributeValue("CanFloat", content.CanFloat);
            xml.SetAttributeValue("IsSelected", content.IsSelected);
            xml.SetAttributeValue("IsLastFocusedDocument", content.IsLastFocusedDocument);
            xml.SetAttributeValue("FloatingWidth", content.FloatingWidth);
            xml.SetAttributeValue("FloatingHeight", content.FloatingHeight);
            xml.SetAttributeValue("FloatingLeft", content.FloatingLeft);
            xml.SetAttributeValue("FloatingTop", content.FloatingTop);
            xml.SetAttributeValue("IsMaximized", content.IsMaximized);
            if (content.ToolTip is string tip) xml.SetAttributeValue("ToolTip", tip);
            if (content.LastActivationTimeStamp is { } time) xml.SetAttributeValue("LastActivationTimeStamp", time.ToString(CultureInfo.InvariantCulture));
            var previous = (content.PreviousContainer as LayoutElement)?.DockModel ?? content.DockModel.OriginalOwner;
            if (previous is not null)
            {
                xml.SetAttributeValue("PreviousContainerId", previous.Id);
                xml.SetAttributeValue("PreviousContainerIndex", content.PreviousContainerIndex);
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
        if (element is LayoutContent content)
        {
            content.Title = (string?)xml.Attribute("Title") ?? string.Empty;
            content.ContentId = (string?)xml.Attribute("ContentId") ?? string.Empty;
            content.CanClose = Boolean(xml, "CanClose", content.CanClose);
            content.CanFloat = Boolean(xml, "CanFloat", content.CanFloat);
            content.FloatingWidth = Number(xml, "FloatingWidth", content.FloatingWidth);
            content.FloatingHeight = Number(xml, "FloatingHeight", content.FloatingHeight);
            content.FloatingLeft = Number(xml, "FloatingLeft", content.FloatingLeft);
            content.FloatingTop = Number(xml, "FloatingTop", content.FloatingTop);
            content.IsMaximized = Boolean(xml, "IsMaximized", false);
            content.ToolTip = (string?)xml.Attribute("ToolTip");
            content.PreviousContainerId = (string?)xml.Attribute("PreviousContainerId");
            content.PreviousContainerIndex = (int?)xml.Attribute("PreviousContainerIndex") ?? -1;
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
        // Name and persistence Id are distinct upstream. Preserve Id for references.
        if (element is LayoutAnchorablePane tools && xml.Attribute("Name") is { } name && xml.Attribute("Id") is null) tools.Name = name.Value;
        foreach (XElement child in xml.Elements())
        {
            if (element is not ILayoutGroup group) throw new XmlException($"'{xml.Name}' cannot contain layout children.");
            group.InsertChildAt(group.ChildrenCount, ReadElement(child, selected, ref active));
        }
        return element;
    }

    private static bool Boolean(XElement element, string name, bool fallback) =>
        element.Attribute(name) is { } value ? bool.Parse(value.Value) : fallback;
    private static string FormatLength(GridLength value) => value.IsAuto ? "Auto" :
        value.Value.ToString(CultureInfo.InvariantCulture) + (value.IsStar ? "*" : string.Empty);
    private static double Number(XElement element, string name, double fallback) =>
        element.Attribute(name) is { } value ? double.Parse(value.Value, CultureInfo.InvariantCulture) : fallback;
}
