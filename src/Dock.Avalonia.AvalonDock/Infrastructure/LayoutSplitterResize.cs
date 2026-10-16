using System;
using Avalonia.Controls;
using Orientation = Avalonia.Layout.Orientation;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Converts a completed native splitter gesture to the public length units.
internal static class LayoutSplitterResize
{
    internal static bool TryResize(Control target, Control neighbor, Orientation orientation, double delta, double minimumSize)
    {
        if (GetModel(target) is not { } first || GetModel(neighbor) is not { } second
            || first is not ILayoutElement firstElement || second is not ILayoutElement secondElement
            || !ReferenceEquals(firstElement.Parent, secondElement.Parent) || !double.IsFinite(delta)) return false;
        bool horizontal = orientation == Orientation.Horizontal;
        double a = horizontal ? target.Bounds.Width : target.Bounds.Height;
        double b = horizontal ? neighbor.Bounds.Width : neighbor.Bounds.Height;
        if (a + b <= 0) return true;
        double minA = Minimum(horizontal ? first.DockMinWidth : first.DockMinHeight, minimumSize);
        double minB = Minimum(horizontal ? second.DockMinWidth : second.DockMinHeight, minimumSize);
        double lower = Math.Min(0, minA - a);
        double upper = Math.Max(0, b - minB);
        delta = Math.Clamp(delta, lower, upper);
        GridLength lengthA = horizontal ? first.DockWidth : first.DockHeight;
        GridLength lengthB = horizontal ? second.DockWidth : second.DockHeight;
        double weight = lengthA.IsStar && lengthB.IsStar ? lengthA.Value + lengthB.Value : 1;
        GridLength nextA = Resize(lengthA, a, delta, minA, a + b, weight);
        GridLength nextB = Resize(lengthB, b, -delta, minB, a + b, weight);
        if (horizontal) { first.DockWidth = nextA; second.DockWidth = nextB; }
        else { first.DockHeight = nextA; second.DockHeight = nextB; }
        return true;
    }

    private static ILayoutPositionableGroup? GetModel(Control control) =>
        control.DataContext is IDockable { Factory: LayoutFactory factory } model
            ? factory.GetLayoutContainer(model) as ILayoutPositionableGroup : null;

    private static double Minimum(double model, double splitter) => Math.Max(double.IsNaN(model) ? 0 : model, splitter);

    private static GridLength Resize(GridLength length, double actual, double delta, double minimum, double total, double weight) =>
        length.IsStar ? new GridLength(Math.Max(0, (actual + delta) / total * weight), GridUnitType.Star)
            : new GridLength(Math.Max(minimum, (length.IsAuto ? actual : length.Value) + delta));
}
