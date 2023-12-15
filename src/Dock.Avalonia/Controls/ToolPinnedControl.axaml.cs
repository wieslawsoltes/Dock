/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
