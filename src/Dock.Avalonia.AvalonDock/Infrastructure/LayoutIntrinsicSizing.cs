using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using AvalonDock.Layout;
using Dock.Model.Controls;
using Dock.Model.Core;
using Orientation = Avalonia.Layout.Orientation;

namespace AvalonDock.Infrastructure;

// Measures the existing native presenters. Auto consumes its natural extent;
// stars measured without a finite budget retain that extent as an arrange minimum.
internal sealed class LayoutIntrinsicSizing
{
    private readonly Dictionary<ILayoutPositionableGroup, double> _lengths = new();
    private bool _unconstrained;

    internal double Measure(global::Avalonia.Controls.Controls children, Size constraint, Orientation orientation)
    {
        _lengths.Clear();
        bool horizontal = orientation == Orientation.Horizontal;
        _unconstrained = double.IsPositiveInfinity(horizontal ? constraint.Width : constraint.Height);
        double total = 0;
        for (int i = 0; i < children.Count; i++)
        {
            Control presenter = children[i];
            if (presenter.DataContext is not IDockable { Factory: LayoutFactory factory } native || native is ISplitter
                || factory.GetLayoutContainer(native) is not ILayoutPositionableGroup model
                || native.IsCollapsable && native.IsEmpty) continue;
            GridLength length = horizontal ? model.DockWidth : model.DockHeight;
            double extent = length.IsAbsolute ? length.Value : 0;
            if (length.IsAuto || _unconstrained && length.IsStar)
            {
                presenter.Measure(horizontal ? constraint.WithWidth(double.PositiveInfinity) : constraint.WithHeight(double.PositiveInfinity));
                extent = horizontal ? presenter.DesiredSize.Width : presenter.DesiredSize.Height;
                _lengths[model] = extent;
            }
            total += Math.Max(extent, GetMinimum(model, horizontal));
        }
        return total;
    }

    internal double GetLength(ILayoutPositionableGroup model) => _lengths.TryGetValue(model, out double value) ? value : 0;

    internal bool IsIntrinsic(ILayoutPositionableGroup model) => _lengths.ContainsKey(model);

    internal double GetMinimum(ILayoutPositionableGroup model, bool horizontal)
    {
        double minimum = horizontal ? model.DockMinWidth : model.DockMinHeight;
        minimum = double.IsNaN(minimum) ? 0 : Math.Max(0, minimum);
        GridLength length = horizontal ? model.DockWidth : model.DockHeight;
        return _unconstrained && length.IsStar ? Math.Max(minimum, GetLength(model)) : minimum;
    }

    internal void Clear() => _lengths.Clear();
}
