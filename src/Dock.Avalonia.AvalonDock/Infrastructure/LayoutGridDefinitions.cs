using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Controls.ProportionalStackPanel;

namespace AvalonDock.Infrastructure;

// Public definitions participate in Avalonia Grid layout, including caller edits.
// Native proportions are synchronized from the resulting pane slots after arrange.
internal sealed class LayoutGridDefinitions(Grid grid, ILayoutOrientableGroup model)
{
    internal void Refresh()
    {
        bool horizontal = model.Orientation == Orientation.Horizontal;
        if (horizontal)
        {
            grid.RowDefinitions.Clear();
            while (grid.ColumnDefinitions.Count > grid.Children.Count) grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count - 1);
            while (grid.ColumnDefinitions.Count < grid.Children.Count) grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        else
        {
            grid.ColumnDefinitions.Clear();
            while (grid.RowDefinitions.Count > grid.Children.Count) grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
            while (grid.RowDefinitions.Count < grid.Children.Count) grid.RowDefinitions.Add(new RowDefinition());
        }
        bool visibleAfter = false;
        for (int i = grid.Children.Count - 1; i >= 0; i--)
        {
            Control child = grid.Children[i];
            bool visible;
            GridLength length;
            double minimum = 0;
            if (child is ILayoutControl { Model: ILayoutPositionableGroup pane })
            {
                visible = ((ILayoutControl)child).Model is ILayoutPanelElement { IsVisible: true };
                length = visible ? horizontal ? pane.DockWidth : pane.DockHeight : new GridLength(0);
                minimum = visible ? horizontal ? pane.DockMinWidth : pane.DockMinHeight : 0;
                visibleAfter |= visible;
                ProportionalStackPanel.SetIsCollapsed(child, !visible);
            }
            else
            {
                visible = visibleAfter && i > 0 && grid.Children[i - 1] is ILayoutControl { Model: ILayoutPanelElement { IsVisible: true } };
                double thickness = horizontal ? LayoutSplitterSizing.GetWidth(grid) : LayoutSplitterSizing.GetHeight(grid);
                length = new GridLength(visible ? double.IsNaN(thickness) ? 6 : Math.Max(0, thickness) : 0);
                if (child is LayoutGridResizerControl splitter) splitter.SetCurrentValue(ProportionalStackPanelSplitter.ThicknessProperty, length.Value);
            }
            child.SetCurrentValue(Visual.IsVisibleProperty, visible);
            Grid.SetColumn(child, horizontal ? i : 0);
            Grid.SetRow(child, horizontal ? 0 : i);
            minimum = double.IsNaN(minimum) ? 0 : Math.Max(0, minimum);
            if (horizontal)
            {
                grid.ColumnDefinitions[i].Width = length;
                grid.ColumnDefinitions[i].MinWidth = minimum;
            }
            else
            {
                grid.RowDefinitions[i].Height = length;
                grid.RowDefinitions[i].MinHeight = minimum;
            }
        }
    }

    internal void SynchronizeProportions(Size size)
    {
        bool horizontal = model.Orientation == Orientation.Horizontal;
        double available = horizontal ? size.Width : size.Height;
        foreach (Control child in grid.Children)
            if (child is LayoutGridResizerControl && child.IsVisible) available -= horizontal ? child.Bounds.Width : child.Bounds.Height;
        available = Math.Max(1, available);
        foreach (Control child in grid.Children)
            if (child is ILayoutControl { Model: ILayoutPositionableGroup pane })
                pane.ApplyDockProportion(child.IsVisible ? (horizontal ? child.Bounds.Width : child.Bounds.Height) / available : 0, available, horizontal);
    }
}
