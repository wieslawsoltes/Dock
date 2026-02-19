// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Returns true when all supplied values are the same object instance.
/// </summary>
public sealed class ReferenceEqualsMultiConverter : IMultiValueConverter
{
    /// <summary>
    /// Singleton converter instance.
    /// </summary>
    public static ReferenceEqualsMultiConverter Instance { get; } = new();

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0)
        {
            return null;
        }

        if (values[0] is null)
        {
            return false;
        }

        var first = values[0];
        for (var i = 1; i < values.Count; i++)
        {
            if (!ReferenceEquals(first, values[i]))
            {
                return false;
            }
        }

        return true;
    }
}
