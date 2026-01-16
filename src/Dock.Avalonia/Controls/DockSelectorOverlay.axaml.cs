// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Dock.Avalonia.Selectors;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Selector overlay control for documents and tools.
/// </summary>
[PseudoClasses(":open")]
public class DockSelectorOverlay : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<DockSelectorOverlay, bool>(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<DockSelectorItem>?> ItemsProperty =
        AvaloniaProperty.Register<DockSelectorOverlay, IReadOnlyList<DockSelectorItem>?>(nameof(Items));

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property.
    /// </summary>
    public static readonly StyledProperty<DockSelectorItem?> SelectedItemProperty =
        AvaloniaProperty.Register<DockSelectorOverlay, DockSelectorItem?>(nameof(SelectedItem));

    /// <summary>
    /// Defines the <see cref="Mode"/> property.
    /// </summary>
    public static readonly StyledProperty<DockSelectorMode> ModeProperty =
        AvaloniaProperty.Register<DockSelectorOverlay, DockSelectorMode>(nameof(Mode), DockSelectorMode.Documents);

    /// <summary>
    /// Gets or sets a value indicating whether the overlay is open.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the selector items.
    /// </summary>
    public IReadOnlyList<DockSelectorItem>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public DockSelectorItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the selector mode.
    /// </summary>
    public DockSelectorMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockSelectorOverlay"/> class.
    /// </summary>
    public DockSelectorOverlay()
    {
        SetCurrentValue(IsVisibleProperty, false);
        SetCurrentValue(IsHitTestVisibleProperty, false);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOpenProperty)
        {
            PseudoClasses.Set(":open", IsOpen);
            SetCurrentValue(IsVisibleProperty, IsOpen);
            SetCurrentValue(IsHitTestVisibleProperty, IsOpen);
        }
    }
}
