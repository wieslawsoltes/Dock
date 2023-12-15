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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// The <see cref="GripMode"/> enum value converters.
/// </summary>
public static class GripModeConverters
{
    /// <summary>
    /// The <see cref="GripMode.AutoHide"/> to <see cref="Grid.RowProperty"/> value converter.
    /// </summary>
    public static readonly IValueConverter GridRowAutoHideConverter =
        new FuncValueConverter<GripMode, int>(x => x == GripMode.AutoHide ? 1 : 0);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleVisibleConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleVisibleOrHiddenConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.Hidden || x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleAutoHideOrVisibleConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.AutoHide || x == GripMode.Visible);

    /// <summary>
    /// The <see cref="GripMode"/> to <see cref="Visual.IsVisible"/> value converter.
    /// </summary>
    public static readonly IValueConverter IsVisibleAutoHideOrHiddenConverter =
        new FuncValueConverter<GripMode, bool>(x => x == GripMode.AutoHide || x == GripMode.Hidden);
}
