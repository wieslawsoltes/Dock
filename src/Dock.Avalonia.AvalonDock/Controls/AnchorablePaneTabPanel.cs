using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvalonDock.Controls;

/// <summary>Arranges tool headers in a horizontal row, sharing constrained space equally.</summary>
public class AnchorablePaneTabPanel : Panel
{
    /// <summary>Creates a panel whose tab order runs from left to right.</summary>
    public AnchorablePaneTabPanel() => FlowDirection = FlowDirection.LeftToRight;

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0;
        double height = 0;
        int count = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible) continue;
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
            count++;
        }
        if (count > 0 && width > availableSize.Width)
        {
            Size constraint = new(availableSize.Width / count, availableSize.Height);
            foreach (Control child in Children)
                if (child.IsVisible) child.Measure(constraint);
        }
        return new Size(Math.Min(width, availableSize.Width), height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        double desired = 0;
        int count = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible) continue;
            desired += child.DesiredSize.Width;
            count++;
        }
        if (count == 0) return finalSize;
        double x = 0;
        bool natural = finalSize.Width > desired;
        double share = finalSize.Width / count;
        foreach (Control child in Children)
        {
            if (!child.IsVisible) continue;
            double width = natural ? child.DesiredSize.Width : share;
            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }
        return finalSize;
    }
}
