// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ProportionalDockControl"/> xaml.
/// </summary>
[TemplatePart("PART_ItemsControl", typeof(ItemsControl), IsRequired = true)]
public class ProportionalDockControl : TemplatedControl
{
    private ItemsControl? _itemsControl;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // Unsubscribe from the previous ItemsControl if it exists
        if (_itemsControl != null)
        {
            _itemsControl.ContainerPrepared -= ItemsControlOnContainerPrepared;
            _itemsControl = null;
        }

        base.OnApplyTemplate(e);

        var itemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");

        if (itemsControl != null)
        {
            _itemsControl = itemsControl;
            _itemsControl.ContainerPrepared += ItemsControlOnContainerPrepared;
        }
    }

    private void ItemsControlOnContainerPrepared(object sender, ContainerPreparedEventArgs e)
    {
        if (e.Container.DataContext is IDockable)
        {
            e.Container[!ProportionalStackPanel.ProportionProperty] = new Binding(nameof(IDockable.Proportion));
        }
    }
}
