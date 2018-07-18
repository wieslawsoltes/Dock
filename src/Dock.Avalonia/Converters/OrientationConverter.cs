// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converts model <see cref="Model.Orientation"/> enum to avalonia <see cref="Orientation"/> enum.
    /// </summary>
    public class OrientationConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is Model.Orientation orientation)
                {
                    switch (orientation)
                    {
                        case Model.Orientation.Horizontal:
                            return Orientation.Horizontal;
                        case Model.Orientation.Vertical:
                            return Orientation.Vertical;
                        default:
                            throw new NotSupportedException($"Provided orientation is not supported in Avalonia.");
                    }
                }
                else
                {
                    return value;
                }
            }
            return AvaloniaProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type of the target.</param>
        /// <param name="parameter">A user-defined parameter.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>The converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is Orientation orientation)
                {
                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            return Model.Orientation.Horizontal;
                        case Orientation.Vertical:
                            return Model.Orientation.Vertical;
                        default:
                            throw new NotSupportedException($"Provided orientation is not supported in Model.");
                    }
                }
                else
                {
                    return value;
                }
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}
