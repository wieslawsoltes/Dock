// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters
{
    /// <summary>
    /// Converts model <see cref="Model.Alignment"/> enum to avalonia <see cref="global::Avalonia.Controls.Dock"/> enum.
    /// </summary>
    public class AlignmentConverter : IValueConverter
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
                if (value is Model.Alignment alignment)
                {
                    switch (alignment)
                    {
                        case Model.Alignment.Unset:
                            return AvaloniaProperty.UnsetValue;
                        case Model.Alignment.Left:
                            return global::Avalonia.Controls.Dock.Left;
                        case Model.Alignment.Bottom:
                            return global::Avalonia.Controls.Dock.Bottom;
                        case Model.Alignment.Right:
                            return global::Avalonia.Controls.Dock.Right;
                        case Model.Alignment.Top:
                            return global::Avalonia.Controls.Dock.Top;
                        default:
                            throw new NotSupportedException($"Provided dock is not supported in Avalonia.");
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
                if (value is global::Avalonia.Controls.Dock dock)
                {
                    switch (dock)
                    {
                        case global::Avalonia.Controls.Dock.Left:
                            return Model.Alignment.Left;
                        case global::Avalonia.Controls.Dock.Bottom:
                            return Model.Alignment.Bottom;
                        case global::Avalonia.Controls.Dock.Right:
                            return Model.Alignment.Right;
                        case global::Avalonia.Controls.Dock.Top:
                            return Model.Alignment.Top;
                        default:
                            return Model.Alignment.Unset;
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
