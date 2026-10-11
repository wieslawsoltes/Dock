using System;
using Avalonia;
using Avalonia.Controls;
using Orientation = Avalonia.Layout.Orientation;
using Dock.Model.Core;
using Dock.Model.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Translates AvalonDock's pixel/star dimensions into Dock's proportional layout.
internal sealed class LayoutSizing
{
    internal void Apply(ILayoutContainer container, Size available, double splitterWidth, double splitterHeight)
    {
        if (container is not ILayoutOrientableGroup oriented) return;
        bool horizontal = oriented.Orientation == Orientation.Horizontal;
        double extent = horizontal ? available.Width : available.Height;
        if (!double.IsFinite(extent) || extent <= 0) return;
        double splitterSize = horizontal ? splitterWidth : splitterHeight;
        int splitters = 0;
        if (container is LayoutElement { DockModel: IDock { VisibleDockables: { } models } })
            for (int i = 1; i + 1 < models.Count; i++)
                if (models[i] is ISplitter && !IsCollapsed(models[i - 1]) && !IsCollapsed(models[i + 1])) splitters++;
        double usable = Math.Max(0, extent - splitters * splitterSize);
        if (usable <= 0) return;
        double pixels = 0;
        double stars = 0;
        foreach (ILayoutElement child in container.Children)
        {
            if (child is not ILayoutPositionableGroup position || child is LayoutElement element && IsCollapsed(element.DockModel)) continue;
            GridLength length = horizontal ? position.DockWidth : position.DockHeight;
            if (length.IsAbsolute) pixels += length.Value;
            else stars += length.IsStar ? length.Value : 1;
        }
        double remainder = Math.Max(0, usable - pixels);
        foreach (ILayoutElement child in container.Children)
        {
            if (child is not ILayoutPositionableGroup position) continue;
            if (child is LayoutElement element && IsCollapsed(element.DockModel))
            {
                position.ApplyDockProportion(0, usable, horizontal);
                continue;
            }
            GridLength length = horizontal ? position.DockWidth : position.DockHeight;
            double desired = length.IsAbsolute ? length.Value : stars > 0 ? remainder * (length.IsStar ? length.Value : 1) / stars : 0;
            double total = pixels > usable ? pixels : usable;
            double proportion = desired / total;
            position.ApplyDockProportion(proportion, usable, horizontal);
        }
    }

    private static bool IsCollapsed(IDockable model) => model.IsCollapsable && model.IsEmpty;
}
