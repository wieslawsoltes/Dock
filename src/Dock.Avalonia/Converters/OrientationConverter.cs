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
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Model.Core.Orientation"/> enum to avalonia <see cref="Orientation"/> enum.
/// </summary>
public class OrientationConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="OrientationConverter"/> instance.
    /// </summary>
    public static readonly OrientationConverter Instance = new OrientationConverter();

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
            Model.Core.Orientation orientation => orientation switch
            {
                Model.Core.Orientation.Horizontal => Orientation.Horizontal,
                Model.Core.Orientation.Vertical => Orientation.Vertical,
                _ => throw new NotSupportedException($"Provided orientation is not supported in Avalonia.")
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
            Orientation orientation => orientation switch
            {
                Orientation.Horizontal => Model.Core.Orientation.Horizontal,
                Orientation.Vertical => Model.Core.Orientation.Vertical,
                _ => throw new NotSupportedException($"Provided orientation is not supported in Model.")
            },
            _ => value
        };
    }
}
