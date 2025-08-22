// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Compares an enum value to a parameter and returns true if equal.
/// </summary>
public class EnumEqualsConverter : IValueConverter
{
    public static EnumEqualsConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return false;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


