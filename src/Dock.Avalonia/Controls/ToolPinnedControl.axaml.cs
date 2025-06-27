// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Interaction logic for <see cref="ToolPinnedControl"/> xaml.
/// </summary>
public class ToolPinnedControl : TemplatedControl
{
    /// <summary>
    /// Defines the <see cref="Items"/> property.
    /// </summary>
    public static readonly DirectProperty<ToolPinnedControl, IEnumerable?> ItemsProperty =
        AvaloniaProperty.RegisterDirect<ToolPinnedControl, IEnumerable?>(
            nameof(Items), 
            o => o.Items, 
            (o, v) => o.Items = v);

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ToolPinnedControl, Orientation>(nameof(Orientation), Orientation.Vertical);

    private IEnumerable? _items = new AvaloniaList<object>();

    /// <summary>
    /// Gets or sets the items to display.
    /// </summary>
    public IEnumerable? Items
    {
        get { return _items; }
        set { SetAndRaise(ItemsProperty, ref _items, value); }
    }

    /// <summary>
    /// Gets or sets the orientation in which child controls will be layed out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}
