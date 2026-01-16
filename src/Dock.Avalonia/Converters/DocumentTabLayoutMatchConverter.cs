// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="DocumentTabLayout"/> values to booleans for visibility checks.
/// </summary>
public class DocumentTabLayoutMatchConverter : IValueConverter
{
    /// <summary>
    /// Shared <see cref="DocumentTabLayoutMatchConverter"/> instance.
    /// </summary>
    public static readonly DocumentTabLayoutMatchConverter Instance = new ();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentTabLayout layout || parameter is not string parameterText)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var spans = parameterText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var span in spans)
        {
            var candidateText = span.Trim();
            if (Enum.TryParse<DocumentTabLayout>(candidateText, true, out var candidate) && candidate == layout)
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
