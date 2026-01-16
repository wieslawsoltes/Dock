// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Determines scroll button visibility based on scroll viewer state.
/// </summary>
public sealed class ScrollViewerScrollButtonVisibilityConverter : IMultiValueConverter
{
    private static readonly ScrollViewerScrollButtonVisibilityConverter s_instance = new();

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static ScrollViewerScrollButtonVisibilityConverter Instance => s_instance;

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 4)
        {
            return false;
        }

        var visibility = ParseScrollBarVisibility(values[0]);
        if (visibility == ScrollBarVisibility.Disabled || visibility == ScrollBarVisibility.Hidden)
        {
            return false;
        }

        if (!TryGetDouble(values[1], out var offset) ||
            !TryGetDouble(values[2], out var extent) ||
            !TryGetDouble(values[3], out var viewport))
        {
            return false;
        }

        if (extent <= 0 || viewport <= 0)
        {
            return false;
        }

        var maxOffset = extent - viewport;
        if (maxOffset <= 0)
        {
            return false;
        }

        return IsLeft(parameter) ? offset > 0 : offset < maxOffset;
    }

    private static ScrollBarVisibility ParseScrollBarVisibility(object? value)
    {
        if (value is ScrollBarVisibility visibility)
        {
            return visibility;
        }

        if (value is string text && Enum.TryParse<ScrollBarVisibility>(text, true, out var parsed))
        {
            return parsed;
        }

        return ScrollBarVisibility.Auto;
    }

    private static bool TryGetDouble(object? value, out double result)
    {
        switch (value)
        {
            case double number:
                result = number;
                break;
            case float number:
                result = number;
                break;
            case int number:
                result = number;
                break;
            case string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                break;
            default:
                result = 0;
                return false;
        }

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            return false;
        }

        return true;
    }

    private static bool IsLeft(object? parameter)
    {
        if (parameter is string text)
        {
            if (text.Equals("left", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (text.Equals("right", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (text.Equals("up", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (text.Equals("down", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed <= 0;
            }
        }

        return parameter switch
        {
            int intValue => intValue <= 0,
            double doubleValue => doubleValue <= 0,
            _ => false
        };
    }
}
