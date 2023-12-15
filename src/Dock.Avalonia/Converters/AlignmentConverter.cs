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
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Alignment"/> enum to avalonia <see cref="Dock"/> enum.
/// </summary>
public class AlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="AlignmentConverter"/> instance.
    /// </summary>
    public static readonly AlignmentConverter Instance = new AlignmentConverter();

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            Alignment alignment => alignment switch
            {
                Alignment.Unset => AvaloniaProperty.UnsetValue,
                Alignment.Left => global::Avalonia.Controls.Dock.Left,
                Alignment.Bottom => global::Avalonia.Controls.Dock.Bottom,
                Alignment.Right => global::Avalonia.Controls.Dock.Right,
                Alignment.Top => global::Avalonia.Controls.Dock.Top,
                _ => throw new NotSupportedException($"Provided dock is not supported in Avalonia.")
            },
            _ => value
        };
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            global::Avalonia.Controls.Dock dock => dock switch
            {
                global::Avalonia.Controls.Dock.Left => Alignment.Left,
                global::Avalonia.Controls.Dock.Bottom => Alignment.Bottom,
                global::Avalonia.Controls.Dock.Right => Alignment.Right,
                global::Avalonia.Controls.Dock.Top => Alignment.Top,
                _ => Alignment.Unset
            },
            _ => value
        };
    }
}
