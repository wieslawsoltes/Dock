using System;
using System.Globalization;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Core;

namespace AvalonDock.Infrastructure;

// Group metadata is read in inheritance order, preserving preceding assignments
// if a later attribute fails. Stored dimensions do not invoke public setters.
internal static class LayoutGroupXml
{
    internal static void ReadMetadata(LayoutElement element, XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        element.VerifyAccess();
        if (reader.ReadState == ReadState.Initial) reader.MoveToContent();
        if (element is ILayoutPositionableGroup or LayoutAnchorGroup && Attribute(reader, "Id") is { } id)
        {
            element.HasExplicitXmlId = true;
            element.RestoreXmlProperty(DockableBase.IdProperty, id);
        }
        if (element is LayoutDocumentPane documents && Attribute(reader, "ShowHeader") is { } header)
            documents.RestoreShowHeader(bool.Parse(header));
        if (element is LayoutAnchorablePane tools)
        {
            if (Attribute(reader, "Name") is { } name) tools.RestoreName(name);
            tools.BeginXmlRead();
        }
        if (element is LayoutAnchorGroup group && Attribute(reader, "PreviousContainerId") is { } previous)
            group.PreviousContainerId = previous;
        if (element is ILayoutOrientableGroup orientable && Attribute(reader, "Orientation") is { } direction)
            orientable.Orientation = Enum.Parse<Orientation>(direction, ignoreCase: true);
        if (element is not ILayoutPositionableGroup position) return;
        if (Attribute(reader, "DockWidth") is { } width)
            position.RestoreDockLengths(GridLength.Parse(width), position.DockHeight);
        if (Attribute(reader, "DockHeight") is { } height)
            position.RestoreDockLengths(position.DockWidth, GridLength.Parse(height));
        if (Attribute(reader, "DockMinWidth") is { } minimumWidth)
            position.RestoreDockMinimum(Number(minimumWidth), horizontal: true);
        if (Attribute(reader, "DockMinHeight") is { } minimumHeight)
            position.RestoreDockMinimum(Number(minimumHeight), horizontal: false);
        if (Attribute(reader, "FloatingWidth") is { } floatingWidth)
            position.FloatingMetadata = position.FloatingMetadata with { Width = Number(floatingWidth) };
        if (Attribute(reader, "FloatingHeight") is { } floatingHeight)
            position.FloatingMetadata = position.FloatingMetadata with { Height = Number(floatingHeight) };
        if (Attribute(reader, "FloatingLeft") is { } left)
            position.FloatingMetadata = position.FloatingMetadata with { Left = Number(left) };
        if (Attribute(reader, "FloatingTop") is { } top)
            position.FloatingMetadata = position.FloatingMetadata with { Top = Number(top) };
        if (Attribute(reader, "IsMaximized") is { } maximized)
            position.FloatingMetadata = position.FloatingMetadata with { IsMaximized = bool.Parse(maximized) };

        // These are additional persisted Dock options, following standard metadata.
        if (Attribute(reader, "AllowDuplicateContent") is { } duplicate)
            position.AllowDuplicateContent = bool.Parse(duplicate);
        if (Attribute(reader, "CanRepositionItems") is { } reposition)
            position.CanRepositionItems = bool.Parse(reposition);
    }

    private static string? Attribute(XmlReader reader, string name) => reader.MoveToAttribute(name) ? reader.Value : null;
    private static double Number(string value) => double.Parse(value, CultureInfo.InvariantCulture);
}
