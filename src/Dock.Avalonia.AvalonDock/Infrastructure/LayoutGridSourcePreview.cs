using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Temporary Grid overrides keep caller definitions, bindings and saved sizes intact.
internal sealed class LayoutGridSourcePreview(Grid grid, ILayoutOrientableGroup model)
{
    private readonly List<IDisposable> _overrides = new();
    private ILayoutPanelElement? _pane;

    internal bool IsActive => _pane is not null;

    internal void SetPane(ILayoutPanelElement? pane)
    {
        ClearOverrides();
        _pane = pane;
        Apply();
        grid.InvalidateMeasure();
    }

    internal void Apply()
    {
        if (_pane is null) return;
        bool horizontal = model.Orientation == Orientation.Horizontal;
        bool visibleAfter = false;
        for (int i = grid.Children.Count - 1; i >= 0; i--)
        {
            Control child = grid.Children[i];
            bool visible;
            if (child is ILayoutControl { Model: ILayoutPanelElement element })
            {
                visible = LayoutDragSourceVisibility.IsVisible(element, _pane);
                visibleAfter |= visible;
            }
            else
                visible = visibleAfter && i > 0
                    && grid.Children[i - 1] is ILayoutControl { Model: ILayoutPanelElement previous }
                    && LayoutDragSourceVisibility.IsVisible(previous, _pane);
            if (visible) continue;
            _overrides.Add(child.SetValue(Visual.IsVisibleProperty, false, BindingPriority.Animation)!);
            if (horizontal && i < grid.ColumnDefinitions.Count)
            {
                _overrides.Add(grid.ColumnDefinitions[i].SetValue(ColumnDefinition.MinWidthProperty, 0, BindingPriority.Animation)!);
                _overrides.Add(grid.ColumnDefinitions[i].SetValue(ColumnDefinition.WidthProperty, new GridLength(0), BindingPriority.Animation)!);
            }
            else if (!horizontal && i < grid.RowDefinitions.Count)
            {
                _overrides.Add(grid.RowDefinitions[i].SetValue(RowDefinition.MinHeightProperty, 0, BindingPriority.Animation)!);
                _overrides.Add(grid.RowDefinitions[i].SetValue(RowDefinition.HeightProperty, new GridLength(0), BindingPriority.Animation)!);
            }
        }
    }

    internal void ClearOverrides()
    {
        foreach (IDisposable value in _overrides) value.Dispose();
        _overrides.Clear();
    }
}
