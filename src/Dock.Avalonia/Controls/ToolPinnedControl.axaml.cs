// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolPinnedControl"/> xaml.
/// </summary>
public class ToolPinnedControl : ItemsControl
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ToolPinnedControl, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Gets or sets the orientation in which child controls will be layed out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolPinnedControl);

    /// <inheritdoc/>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new ToolPinItemControl
        {
            Orientation = Orientation
        };
    }

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);

        if (container is ToolPinItemControl pinItemControl)
        {
            pinItemControl.Orientation = Orientation;
        }
    }

    /// <inheritdoc/>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<ToolPinItemControl>(item, out recycleKey);
    }
}
