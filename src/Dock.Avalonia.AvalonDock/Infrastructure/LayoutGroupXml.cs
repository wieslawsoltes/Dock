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
    private static void Prepare(LayoutElement element, XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        element.VerifyAccess();
        if (reader.ReadState == ReadState.Initial) reader.MoveToContent();
    }

    private static void Identifier(LayoutElement element, XmlReader reader)
    {
        if (Attribute(reader, "Id") is { } id)
        {
            element.HasExplicitXmlId = true;
            element.RestoreXmlProperty(DockableBase.IdProperty, id);
        }
    }

    internal static void ReadDocumentPane(LayoutDocumentPane pane, XmlReader reader)
    {
        Prepare(pane, reader);
        Identifier(pane, reader);
        if (Attribute(reader, "ShowHeader") is { } header) pane.RestoreShowHeader(bool.Parse(header));
    }

    internal static void ReadAnchorablePane(LayoutAnchorablePane pane, XmlReader reader)
    {
        Prepare(pane, reader);
        Identifier(pane, reader);
        if (Attribute(reader, "Name") is { } name) pane.RestoreName(name);
        pane.BeginXmlRead();
    }

    internal static void ReadAnchorGroup(LayoutAnchorGroup group, XmlReader reader)
    {
        Prepare(group, reader);
        Identifier(group, reader);
        if (Attribute(reader, "PreviousContainerId") is { } previous)
            group.PreviousContainerId = previous;
    }

    internal static void ReadOrientable(LayoutElement element, ILayoutOrientableGroup group, XmlReader reader)
    {
        Prepare(element, reader);
        Identifier(element, reader); // Native group return-location extension.
        if (Attribute(reader, "Orientation") is { } direction)
            group.Orientation = Enum.Parse<Orientation>(direction, ignoreCase: true);
    }

    internal static void ReadPositionable(LayoutElement element, ILayoutPositionableGroup position, XmlReader reader)
    {
        Prepare(element, reader);
        // Concrete panes and orientable groups have already read their identifier.
        // An application deriving directly from this base can also restore a
        // native group return identifier, without inferring additional metadata.
        if (element is not (LayoutDocumentPane or LayoutAnchorablePane or LayoutPanel or LayoutDocumentPaneGroup or LayoutAnchorablePaneGroup))
            Identifier(element, reader);
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
