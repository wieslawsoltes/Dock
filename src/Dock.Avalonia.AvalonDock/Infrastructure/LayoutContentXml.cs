using System;
using System.Globalization;
using System.Xml;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Content metadata uses the ordinary setters, in inheritance order. Keep the
// caller's reader live so callbacks and derived XML hooks observe its position.
internal static class LayoutContentXml
{
    internal static void ReadDocument(LayoutDocument document, XmlReader reader)
    {
        Prepare(document, reader);
        if (Attribute(reader, nameof(document.Description)) is { } description) document.Description = description;
        if (Attribute(reader, nameof(document.CanMove)) is { } movable) document.CanMove = bool.Parse(movable);
    }

    internal static void ReadAnchorable(LayoutAnchorable tool, XmlReader reader)
    {
        Prepare(tool, reader);
        if (Attribute(reader, nameof(tool.CanHide)) is { } hide) tool.CanHide = bool.Parse(hide);
        if (Attribute(reader, nameof(tool.CanAutoHide)) is { } autoHide) tool.CanAutoHide = bool.Parse(autoHide);
        if (Attribute(reader, nameof(tool.AutoHideWidth)) is { } width) tool.AutoHideWidth = Number(width);
        if (Attribute(reader, nameof(tool.AutoHideHeight)) is { } height) tool.AutoHideHeight = Number(height);
        if (Attribute(reader, nameof(tool.AutoHideMinWidth)) is { } minimumWidth) tool.AutoHideMinWidth = Number(minimumWidth);
        if (Attribute(reader, nameof(tool.AutoHideMinHeight)) is { } minimumHeight) tool.AutoHideMinHeight = Number(minimumHeight);
        if (Attribute(reader, nameof(tool.CanDockAsTabbedDocument)) is { } tabbed) tool.CanDockAsTabbedDocument = bool.Parse(tabbed);
    }

    internal static void Read(LayoutContent content, XmlReader reader)
    {
        Prepare(content, reader);
        if (Attribute(reader, nameof(content.Title)) is { } title) content.Title = title;
        if (Attribute(reader, nameof(content.IsSelected)) is { } selected) content.IsSelected = bool.Parse(selected);
        if (Attribute(reader, nameof(content.ContentId)) is { } id) content.ContentId = id;
        if (Attribute(reader, nameof(content.IsLastFocusedDocument)) is { } lastFocused) content.SetLastFocusedDocument(bool.Parse(lastFocused));
        if (Attribute(reader, nameof(content.PreviousContainerId)) is { } previous) content.PreviousContainerId = previous;
        if (Attribute(reader, nameof(content.PreviousContainerIndex)) is { } previousIndex) content.PreviousContainerIndex = int.Parse(previousIndex);
        if (Attribute(reader, nameof(content.InitialContainerId)) is { } initial) content.InitialContainerId = initial;
        if (Attribute(reader, nameof(content.InitialContainerIndex)) is { } initialIndex) content.InitialContainerIndex = int.Parse(initialIndex);
        if (Attribute(reader, nameof(content.FloatingLeft)) is { } left) content.FloatingLeft = Number(left);
        if (Attribute(reader, nameof(content.FloatingTop)) is { } top) content.FloatingTop = Number(top);
        if (Attribute(reader, nameof(content.FloatingWidth)) is { } width) content.FloatingWidth = Number(width);
        if (Attribute(reader, nameof(content.FloatingHeight)) is { } height) content.FloatingHeight = Number(height);
        if (Attribute(reader, nameof(content.IsFloating)) is { } floating) content.SetFloating(bool.Parse(floating));
        if (Attribute(reader, nameof(content.IsMaximized)) is { } maximized) content.IsMaximized = bool.Parse(maximized);
        if (Attribute(reader, nameof(content.CanClose)) is { } closable) content.CanClose = bool.Parse(closable);
        if (Attribute(reader, nameof(content.CanFloat)) is { } floatable) content.CanFloat = bool.Parse(floatable);
        if (Attribute(reader, nameof(content.LastActivationTimeStamp)) is { } timestamp)
            content.LastActivationTimeStamp = DateTime.Parse(timestamp, CultureInfo.InvariantCulture);

        // Preserve the adapter's additional tooltip round trip after the
        // standard attributes; it must not precede their observable callbacks.
        if (Attribute(reader, nameof(content.ToolTip)) is { } tooltip) content.ToolTip = tooltip;
        reader.Read();
    }

    private static void Prepare(LayoutContent content, XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        content.VerifyAccess();
        if (reader.ReadState == ReadState.Initial) reader.MoveToContent();
    }

    private static string? Attribute(XmlReader reader, string name) => reader.MoveToAttribute(name) ? reader.Value : null;
    private static double Number(string value) => double.Parse(value, CultureInfo.InvariantCulture);
}
