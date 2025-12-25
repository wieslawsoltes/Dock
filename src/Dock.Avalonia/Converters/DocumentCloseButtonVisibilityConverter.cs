// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Determines close button visibility based on close button mode and active state.
/// </summary>
public sealed class DocumentCloseButtonVisibilityConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static readonly DocumentCloseButtonVisibilityConverter Instance = new();

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return true;
        }

        var mode = ParseMode(values[0]);
        var isActive = values[1] is bool active && active;

        return mode switch
        {
            DocumentCloseButtonShowMode.Always => true,
            DocumentCloseButtonShowMode.Active => isActive,
            DocumentCloseButtonShowMode.Never => false,
            _ => true
        };
    }

    private static DocumentCloseButtonShowMode ParseMode(object? value)
    {
        if (value is DocumentCloseButtonShowMode mode)
        {
            return mode;
        }

        if (value is string text && Enum.TryParse<DocumentCloseButtonShowMode>(text, true, out var parsed))
        {
            return parsed;
        }

        return DocumentCloseButtonShowMode.Always;
    }
}
