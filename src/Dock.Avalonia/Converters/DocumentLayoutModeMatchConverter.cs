// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="DocumentLayoutMode"/> values to booleans for visibility checks.
/// </summary>
public class DocumentLayoutModeMatchConverter : IValueConverter
{
    /// <summary>
    /// Shared <see cref="DocumentLayoutModeMatchConverter"/> instance.
    /// </summary>
    public static readonly DocumentLayoutModeMatchConverter Instance = new ();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentLayoutMode layoutMode || parameter is not string parameterText)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var spans = parameterText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var span in spans)
        {
            var candidateText = span.Trim();
            if (Enum.TryParse<DocumentLayoutMode>(candidateText, true, out var candidate) && candidate == layoutMode)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
