// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="DocumentTabLayout"/> to <see cref="Orientation"/>.
/// </summary>
public class DocumentTabOrientationConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="DocumentTabDockConverter"/> instance.
    /// </summary>
    public static readonly DocumentTabOrientationConverter Instance = new ();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DocumentTabLayout.Left => global::Avalonia.Layout.Orientation.Vertical,
            DocumentTabLayout.Right => global::Avalonia.Layout.Orientation.Vertical,
            DocumentTabLayout.Top => global::Avalonia.Layout.Orientation.Horizontal,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
