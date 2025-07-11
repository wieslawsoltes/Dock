// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using AC = Avalonia.Controls;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="DocumentTabLayout"/> to Avalonia <see cref="Dock"/>.
/// </summary>
public class DocumentTabDockConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="DocumentTabDockConverter"/> instance.
    /// </summary>
    public static readonly DocumentTabDockConverter Instance = new ();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DocumentTabLayout.Left => AC.Dock.Left,
            DocumentTabLayout.Right => AC.Dock.Right,
            DocumentTabLayout.Top => AC.Dock.Top,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
