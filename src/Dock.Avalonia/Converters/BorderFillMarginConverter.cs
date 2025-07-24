// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts values to a <see cref="Thickness"/> for the margin of a border fill.
/// </summary>
public class BorderFillMarginConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets <see cref="BorderFillMarginConverter"/> instance.
    /// </summary>
    public static readonly BorderFillMarginConverter Instance = new BorderFillMarginConverter();

    /// <summary>
    /// Converts a list of values to a <see cref="Thickness"/> for the margin of a border fill.
    /// </summary>
    /// <param name="values">The values to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 5 
            && values[0] is double width 
            && values[1] is double height 
            && values[2] is bool isVisible
            && values[3] is VerticalAlignment verticalAlignment
            && values[4] is HorizontalAlignment horizontalAlignment)
        {
            if (isVisible)
            {
                if (verticalAlignment == VerticalAlignment.Top
                    && horizontalAlignment == HorizontalAlignment.Stretch)
                {
                    return new Thickness(0, height, 0, 0);
                }

                if (verticalAlignment == VerticalAlignment.Stretch
                    && horizontalAlignment == HorizontalAlignment.Left)
                {
                    return new Thickness(width, 0, 0, 0);
                }
            }
        }

        return new Thickness();
    }
}
