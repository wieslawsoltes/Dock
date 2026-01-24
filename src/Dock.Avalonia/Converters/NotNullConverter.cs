// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converter that returns true when the value is not null.
/// </summary>
public sealed class NotNullConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly NotNullConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isNotNull = value is not null;

        if (parameter is bool invertBool && invertBool)
        {
            return !isNotNull;
        }

        if (parameter is string text
            && string.Equals(text, "Not", StringComparison.OrdinalIgnoreCase))
        {
            return !isNotNull;
        }

        return isNotNull;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
