using System.Globalization;
using System.Xml;
using Avalonia.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// These typed attribute writers follow the public model's inheritance order.
// Container traversal and application WriteXml dispatch belong to AvalonDockXml.
internal static class LayoutXmlAttributes
{
    internal static void Content(LayoutContent content, XmlWriter writer)
    {
        Text(writer, nameof(content.Title), content.Title);
        Boolean(writer, nameof(content.IsSelected), content.IsSelected);
        Boolean(writer, nameof(content.IsLastFocusedDocument), content.IsLastFocusedDocument);
        Text(writer, nameof(content.ContentId), content.ContentId);
        Text(writer, nameof(content.ToolTip), content.ToolTip as string);
        Number(writer, nameof(content.FloatingLeft), content.FloatingLeft);
        Number(writer, nameof(content.FloatingTop), content.FloatingTop);
        Number(writer, nameof(content.FloatingWidth), content.FloatingWidth);
        Number(writer, nameof(content.FloatingHeight), content.FloatingHeight);
        Boolean(writer, nameof(content.IsFloating), content.IsFloating);
        Boolean(writer, nameof(content.IsMaximized), content.IsMaximized);
        // The two content types have different CanClose defaults.
        writer.WriteAttributeString(nameof(content.CanClose), content.CanClose.ToString());
        Boolean(writer, nameof(content.CanFloat), content.CanFloat, defaultValue: true);
        if (content.LastActivationTimeStamp is { } time)
            writer.WriteAttributeString(nameof(content.LastActivationTimeStamp), time.ToString(CultureInfo.InvariantCulture));
        Reference(writer, content, initial: false);
        Reference(writer, content, initial: true);
    }

    internal static void Document(LayoutDocument document, XmlWriter writer)
    {
        Text(writer, nameof(document.Description), document.Description);
        Boolean(writer, nameof(document.CanMove), document.CanMove, defaultValue: true);
    }

    internal static void Anchorable(LayoutAnchorable tool, XmlWriter writer)
    {
        Boolean(writer, nameof(tool.CanHide), tool.CanHide, defaultValue: true);
        Boolean(writer, nameof(tool.CanAutoHide), tool.CanAutoHide, defaultValue: true);
        if (tool.AutoHideWidth > 0) Number(writer, nameof(tool.AutoHideWidth), tool.AutoHideWidth);
        if (tool.AutoHideHeight > 0) Number(writer, nameof(tool.AutoHideHeight), tool.AutoHideHeight);
        Number(writer, nameof(tool.AutoHideMinWidth), tool.AutoHideMinWidth, 25);
        Number(writer, nameof(tool.AutoHideMinHeight), tool.AutoHideMinHeight, 25);
        Boolean(writer, nameof(tool.CanDockAsTabbedDocument), tool.CanDockAsTabbedDocument, defaultValue: true);
    }

    internal static void Position(LayoutElement element, ILayoutPositionableGroup group, XmlWriter writer)
    {
        Length(writer, nameof(group.DockWidth), group.DockWidth);
        Length(writer, nameof(group.DockHeight), group.DockHeight);
        Number(writer, nameof(group.DockMinWidth), group.DockMinWidth, 25);
        Number(writer, nameof(group.DockMinHeight), group.DockMinHeight, 25);
        Number(writer, nameof(group.FloatingWidth), group.FloatingWidth);
        Number(writer, nameof(group.FloatingHeight), group.FloatingHeight);
        Number(writer, nameof(group.FloatingLeft), group.FloatingLeft);
        Number(writer, nameof(group.FloatingTop), group.FloatingTop);
        Boolean(writer, nameof(group.IsMaximized), group.IsMaximized);

        // Native group return locations and these options extend upstream XML.
        if (!LayoutContainerIdentity.IsPane(element)) Identifier(element, writer);
        Boolean(writer, nameof(group.AllowDuplicateContent), group.AllowDuplicateContent, defaultValue: true);
        Boolean(writer, nameof(group.CanRepositionItems), group.CanRepositionItems, defaultValue: true);
    }

    internal static void Identifier(LayoutElement element, XmlWriter writer)
    {
        if (element.XmlId is { } id) writer.WriteAttributeString("Id", id);
    }

    internal static void DocumentPane(LayoutDocumentPane pane, XmlWriter writer)
    {
        Identifier(pane, writer);
        Boolean(writer, nameof(pane.ShowHeader), pane.ShowHeader, defaultValue: true);
    }

    internal static void AnchorablePane(LayoutAnchorablePane pane, XmlWriter writer)
    {
        Identifier(pane, writer);
        if (pane.Name is { } name) writer.WriteAttributeString(nameof(pane.Name), name);
    }

    internal static void AnchorGroup(LayoutAnchorGroup group, XmlWriter writer)
    {
        Identifier(group, writer);
        if (LayoutContainerIdentity.CanWriteReference(group.PreviousContainer) && group.PreviousContainer is LayoutElement previous)
            writer.WriteAttributeString("PreviousContainerId", previous.XmlId);
    }

    private static void Reference(XmlWriter writer, LayoutContent content, bool initial)
    {
        ILayoutContainer? container = initial ? content.InitialContainer : content.PreviousContainer;
        if (!LayoutContainerIdentity.CanWriteReference(container) || container is not LayoutElement element) return;
        writer.WriteAttributeString(initial ? "InitialContainerId" : "PreviousContainerId", element.XmlId);
        int index = initial ? content.InitialContainerIndex : content.PreviousContainerIndex;
        writer.WriteAttributeString(initial ? "InitialContainerIndex" : "PreviousContainerIndex", index.ToString(CultureInfo.CurrentCulture));
    }

    private static void Text(XmlWriter writer, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) writer.WriteAttributeString(name, value);
    }

    private static void Boolean(XmlWriter writer, string name, bool value, bool defaultValue = false)
    {
        if (value != defaultValue) writer.WriteAttributeString(name, value.ToString());
    }

    private static void Number(XmlWriter writer, string name, double value, double defaultValue = 0)
    {
        if (value != defaultValue) writer.WriteAttributeString(name, value.ToString(CultureInfo.InvariantCulture));
    }

    private static void Length(XmlWriter writer, string name, GridLength value)
    {
        if (value.IsStar && value.Value == 1) return;
        writer.WriteAttributeString(name, value.IsAuto ? "Auto" :
            value.Value.ToString(CultureInfo.InvariantCulture) + (value.IsStar ? "*" : string.Empty));
    }
}
