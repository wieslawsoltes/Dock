using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.Infrastructure;

// Translates AvalonDock's pixel/star dimensions into Dock's proportional layout.
internal sealed class LayoutSizing
{
    internal void Apply(LayoutRoot root, Size available) => ApplyGroup(root.RootPanel, available);

    private void ApplyGroup(ILayoutContainer container, Size available)
    {
        if (container is not ILayoutOrientableGroup oriented) return;
        bool horizontal = oriented.Orientation == Orientation.Horizontal;
        double extent = horizontal ? available.Width : available.Height;
        if (!double.IsFinite(extent) || extent <= 0) return;
        // Dock's default proportional splitter occupies four device-independent pixels.
        double usable = Math.Max(0, extent - Math.Max(0, container.ChildrenCount - 1) * 4);
        if (usable <= 0) return;
        double pixels = 0;
        double stars = 0;
        foreach (ILayoutElement child in container.Children)
        {
            if (child is not ILayoutPositionableGroup position) continue;
            GridLength length = horizontal ? position.DockWidth : position.DockHeight;
            if (length.IsAbsolute) pixels += length.Value;
            else stars += length.IsStar ? length.Value : 1;
        }
        double remainder = Math.Max(0, usable - pixels);
        foreach (ILayoutElement child in container.Children)
        {
            if (child is not ILayoutPositionableGroup position) continue;
            GridLength length = horizontal ? position.DockWidth : position.DockHeight;
            double desired = length.IsAbsolute ? length.Value : stars > 0 ? remainder * (length.IsStar ? length.Value : 1) / stars : 0;
            double total = pixels > usable ? pixels : usable;
            double proportion = desired / total;
            position.ApplyDockProportion(proportion, usable, horizontal);
            if (child is ILayoutContainer nested)
                ApplyGroup(nested, horizontal ? new Size(usable * proportion, available.Height) : new Size(available.Width, usable * proportion));
        }
    }
}
