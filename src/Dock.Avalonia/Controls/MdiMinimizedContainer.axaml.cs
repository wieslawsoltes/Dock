// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Container control for minimized MDI documents with horizontal stack panel layout.
/// </summary>
public class MdiMinimizedContainer : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="MinimizedItems"/> property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<IDockable>?> MinimizedItemsProperty =
        AvaloniaProperty.Register<MdiMinimizedContainer, ObservableCollection<IDockable>?>(nameof(MinimizedItems));

    /// <summary>
    /// Defines the <see cref="HasItems"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasItemsProperty =
        AvaloniaProperty.Register<MdiMinimizedContainer, bool>(nameof(HasItems));

    private ItemsControl? _itemsControl;

    static MdiMinimizedContainer()
    {
        MinimizedItemsProperty.Changed.AddClassHandler<MdiMinimizedContainer>((x, e) => x.OnItemsChanged());
    }

    /// <summary>
    /// Gets or sets the collection of minimized MDI document items.
    /// </summary>
    public ObservableCollection<IDockable>? MinimizedItems
    {
        get => GetValue(MinimizedItemsProperty);
        set => SetValue(MinimizedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the container has items.
    /// </summary>
    public bool HasItems
    {
        get => GetValue(HasItemsProperty);
        set => SetValue(HasItemsProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsControl = e.NameScope.Find<ItemsControl>("PART_MinimizedItems");
        UpdateItemsControl();
    }

    private void OnItemsChanged()
    {
        UpdateItemsControl();
        UpdateVisibility();
    }

    private void UpdateItemsControl()
    {
        if (_itemsControl is not null)
        {
            _itemsControl.ItemsSource = MinimizedItems;
        }
    }

    private void UpdateVisibility()
    {
        HasItems = MinimizedItems?.Any() == true;
    }
}
