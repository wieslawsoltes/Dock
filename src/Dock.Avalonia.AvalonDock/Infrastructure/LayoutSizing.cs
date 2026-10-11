using System;
using System.Collections;
using Avalonia.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Orientation = Avalonia.Layout.Orientation;

namespace AvalonDock.Infrastructure;

// Translates public lengths into proportions without normalizing fixed columns
// or writing render-time constraints back into the requested model dimensions.
internal sealed class LayoutSizing
{
    internal bool Apply(ILayoutContainer container, double available, double denominator)
    {
        if (container is not ILayoutOrientableGroup oriented || container.Children is not IList children
            || !double.IsFinite(available) || !double.IsFinite(denominator) || denominator <= 0) return false;
        bool horizontal = oriented.Orientation == Orientation.Horizontal;
        double pixels = 0;
        double maxWeight = 0;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is not ILayoutPositionableGroup child || IsCollapsed(child)) continue;
            GridLength length = GetLength(child, horizontal);
            if (length.IsAbsolute) pixels += Math.Max(length.Value, GetMinimum(child, horizontal));
            else maxWeight = Math.Max(maxWeight, GetWeight(length));
        }
        double remaining = Math.Max(0, available - pixels);
        double scale = GetStarScale(children, horizontal, maxWeight, remaining);
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is not ILayoutPositionableGroup child) continue;
            double desired = 0;
            if (!IsCollapsed(child))
            {
                GridLength length = GetLength(child, horizontal);
                desired = length.IsAbsolute ? length.Value : maxWeight > 0 ? GetWeight(length) / maxWeight * scale : 0;
                desired = Math.Max(desired, GetMinimum(child, horizontal));
            }
            child.ApplyDockProportion(desired / denominator, denominator, horizontal);
        }
        return true;
    }

    // Water-fill the star allocation: reserve constrained children first, then
    // divide the remaining space among the others. Scaling weights avoids overflow.
    private static double GetStarScale(IList children, bool horizontal, double maxWeight, double available)
    {
        if (maxWeight <= 0) return 0;
        double weight = 0;
        for (int i = 0; i < children.Count; i++)
            if (children[i] is ILayoutPositionableGroup child && !IsCollapsed(child) && !GetLength(child, horizontal).IsAbsolute)
                weight += GetWeight(GetLength(child, horizontal)) / maxWeight;
        double scale = weight > 0 ? available / weight : 0;
        for (int pass = 0; pass < children.Count; pass++)
        {
            double reserved = 0;
            double activeWeight = 0;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is not ILayoutPositionableGroup child || IsCollapsed(child)) continue;
                GridLength length = GetLength(child, horizontal);
                if (length.IsAbsolute) continue;
                double normalizedWeight = GetWeight(length) / maxWeight;
                double minimum = GetMinimum(child, horizontal);
                if (minimum > normalizedWeight * scale) reserved += minimum;
                else activeWeight += normalizedWeight;
            }
            double next = activeWeight > 0 ? Math.Max(0, available - reserved) / activeWeight : 0;
            if (next >= scale) break;
            scale = next;
        }
        return scale;
    }

    private static bool IsCollapsed(ILayoutPositionableGroup child) =>
        child is LayoutElement { DockModel: IDockable { IsCollapsable: true, IsEmpty: true } };
    private static GridLength GetLength(ILayoutPositionableGroup child, bool horizontal) => horizontal ? child.DockWidth : child.DockHeight;
    private static double GetMinimum(ILayoutPositionableGroup child, bool horizontal)
    {
        double value = horizontal ? child.DockMinWidth : child.DockMinHeight;
        return double.IsNaN(value) ? 0 : Math.Max(0, value);
    }
    // Intrinsic auto measurement is a separate policy still to be implemented.
    private static double GetWeight(GridLength length) => length.IsStar ? length.Value : 1;
}
