// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStrip custom control.
/// </summary>
[PseudoClasses(":create")]
public class ToolTabStrip : TabStrip
{
    /// <summary>
    /// Defines the <see cref="DockAdornerHost"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DockAdornerHostProperty =
        AvaloniaProperty.Register<ToolTabStrip, Control?>(nameof(DockAdornerHost));

    /// <summary>
    /// Defines the <see cref="CanCreateItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanCreateItemProperty =
        AvaloniaProperty.Register<ToolTabStrip, bool>(nameof(CanCreateItem));

    /// <summary>
    /// Gets or sets if tab strop dock can create new items.
    /// </summary>
    public bool CanCreateItem
    {
        get => GetValue(CanCreateItemProperty);
        set => SetValue(CanCreateItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the control that should host the dock adorner.
    /// </summary>
    public Control? DockAdornerHost
    {
        get => GetValue(DockAdornerHostProperty);
        set => SetValue(DockAdornerHostProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStrip);

    /// <summary>
    /// Initializes new instance of the <see cref="ToolTabStrip"/> class.
    /// </summary>
    public ToolTabStrip()
    {
        UpdatePseudoClasses(CanCreateItem);
    }

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ToolTabStripItem();
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ToolTabStripItem>(item, out recycleKey);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCreateItemProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClasses(bool canCreate)
    {
        PseudoClasses.Set(":create", canCreate);
    }
}
