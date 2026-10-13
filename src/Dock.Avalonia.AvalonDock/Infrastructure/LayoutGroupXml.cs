using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Core;

namespace AvalonDock.Infrastructure;

// Group metadata is read in inheritance order, preserving preceding assignments
// if a later attribute fails. Stored dimensions do not invoke public setters.
internal static class LayoutGroupXml
{
    internal static void ReadMetadata(LayoutElement element, XElement xml)
    {
        element.VerifyAccess();
        if (xml.Attribute("Id") is { } id && element is ILayoutPositionableGroup or LayoutAnchorGroup)
        {
            element.HasExplicitXmlId = true;
            element.RestoreXmlProperty(DockableBase.IdProperty, id.Value);
        }
        if (element is LayoutDocumentPane documents && xml.Attribute("ShowHeader") is { } header)
            documents.RestoreShowHeader(bool.Parse(header.Value));
        if (element is LayoutAnchorablePane tools && xml.Attribute("Name") is { } name)
            tools.RestoreName(name.Value);
        if (element is LayoutAnchorGroup group && xml.Attribute("PreviousContainerId") is { } previous)
            group.PreviousContainerId = previous.Value;
        if (element is ILayoutOrientableGroup orientable && xml.Attribute("Orientation") is { } direction)
        {
            if (!Enum.TryParse(direction.Value, ignoreCase: true, out Orientation orientation))
                throw new XmlException($"Invalid orientation '{direction.Value}'.");
            orientable.Orientation = orientation;
        }
        if (element is not ILayoutPositionableGroup position) return;
        if (xml.Attribute("DockWidth") is { } width)
            position.RestoreDockLengths(GridLength.Parse(width.Value), position.DockHeight);
        if (xml.Attribute("DockHeight") is { } height)
            position.RestoreDockLengths(position.DockWidth, GridLength.Parse(height.Value));
        if (xml.Attribute("DockMinWidth") is { } minimumWidth)
            position.RestoreDockMinimum(Number(minimumWidth), horizontal: true);
        if (xml.Attribute("DockMinHeight") is { } minimumHeight)
            position.RestoreDockMinimum(Number(minimumHeight), horizontal: false);
        if (xml.Attribute("FloatingWidth") is { } floatingWidth)
            position.FloatingMetadata = position.FloatingMetadata with { Width = Number(floatingWidth) };
        if (xml.Attribute("FloatingHeight") is { } floatingHeight)
            position.FloatingMetadata = position.FloatingMetadata with { Height = Number(floatingHeight) };
        if (xml.Attribute("FloatingLeft") is { } left)
            position.FloatingMetadata = position.FloatingMetadata with { Left = Number(left) };
        if (xml.Attribute("FloatingTop") is { } top)
            position.FloatingMetadata = position.FloatingMetadata with { Top = Number(top) };
        if (xml.Attribute("IsMaximized") is { } maximized)
            position.FloatingMetadata = position.FloatingMetadata with { IsMaximized = bool.Parse(maximized.Value) };

        // These are additional persisted Dock options, following standard metadata.
        if (xml.Attribute("AllowDuplicateContent") is { } duplicate)
            position.AllowDuplicateContent = bool.Parse(duplicate.Value);
        if (xml.Attribute("CanRepositionItems") is { } reposition)
            position.CanRepositionItems = bool.Parse(reposition.Value);
    }

    private static double Number(XAttribute value) => double.Parse(value.Value, CultureInfo.InvariantCulture);
}
