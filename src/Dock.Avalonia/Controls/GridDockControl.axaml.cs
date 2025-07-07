// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="GridDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_ItemsControl", typeof(ItemsControl))]
public class GridDockControl : TemplatedControl
{
    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var itemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        if (itemsControl is not null)
        {
            itemsControl.Loaded += ItemsControlOnLoaded;
        }
    }

    private void ItemsControlOnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
        {
            if (itemsControl.ItemsPanelRoot is Grid grid)
            {
                if (itemsControl.DataContext is IGridDock gridDock)
                {
                    if (gridDock.ColumnDefinitions is not null)
                    {
                        grid.ColumnDefinitions = ColumnDefinitions.Parse(gridDock.ColumnDefinitions);
                    }

                    if (gridDock.RowDefinitions is not null)
                    {
                        grid.RowDefinitions = RowDefinitions.Parse(gridDock.RowDefinitions);
                    }
                }
            }
        }
    }
}
