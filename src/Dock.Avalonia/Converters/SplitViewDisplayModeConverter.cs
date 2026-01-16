// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Model.Core.SplitViewDisplayMode"/> enum to avalonia <see cref="SplitViewDisplayMode"/> enum.
/// </summary>
public class SplitViewDisplayModeConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="SplitViewDisplayModeConverter"/> instance.
    /// </summary>
    public static readonly SplitViewDisplayModeConverter Instance = new();

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            Model.Core.SplitViewDisplayMode mode => mode switch
            {
                Model.Core.SplitViewDisplayMode.Inline => SplitViewDisplayMode.Inline,
                Model.Core.SplitViewDisplayMode.CompactInline => SplitViewDisplayMode.CompactInline,
                Model.Core.SplitViewDisplayMode.Overlay => SplitViewDisplayMode.Overlay,
                Model.Core.SplitViewDisplayMode.CompactOverlay => SplitViewDisplayMode.CompactOverlay,
                _ => SplitViewDisplayMode.Overlay
            },
            _ => value
        };
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => AvaloniaProperty.UnsetValue,
            SplitViewDisplayMode mode => mode switch
            {
                SplitViewDisplayMode.Inline => Model.Core.SplitViewDisplayMode.Inline,
                SplitViewDisplayMode.CompactInline => Model.Core.SplitViewDisplayMode.CompactInline,
                SplitViewDisplayMode.Overlay => Model.Core.SplitViewDisplayMode.Overlay,
                SplitViewDisplayMode.CompactOverlay => Model.Core.SplitViewDisplayMode.CompactOverlay,
                _ => Model.Core.SplitViewDisplayMode.Overlay
            },
            _ => value
        };
    }
}
