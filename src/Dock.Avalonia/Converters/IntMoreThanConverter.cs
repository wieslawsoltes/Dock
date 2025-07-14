// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts an integer to a boolean indicating whether it is more than a specified value.
/// </summary>
public class IntMoreThanConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the value to compare against. The converter will return true if the input integer is more than this value.
    /// </summary>
    public int TrueIfMoreThan { get; set; }

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > TrueIfMoreThan;
        }
        return false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
