// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts dockable objects to display title with a fallback to type name.
/// </summary>
public class TitleOrTypeNameConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="TitleOrTypeNameConverter"/> instance.
    /// </summary>
    public static readonly TitleOrTypeNameConverter Instance = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IDockable dockable)
        {
            return string.IsNullOrEmpty(dockable.Title) ? dockable.GetType().Name : dockable.Title;
        }

        if (value is IDockWindow window)
        {
            return string.IsNullOrEmpty(window.Title) ? window.GetType().Name : window.Title;
        }

        return value?.GetType().Name ?? AvaloniaProperty.UnsetValue;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
