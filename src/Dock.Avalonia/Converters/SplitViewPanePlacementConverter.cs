// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts model <see cref="Model.Core.SplitViewPanePlacement"/> enum to avalonia <see cref="SplitViewPanePlacement"/> enum.
/// </summary>
public class SplitViewPanePlacementConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="SplitViewPanePlacementConverter"/> instance.
    /// </summary>
    public static readonly SplitViewPanePlacementConverter Instance = new();

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
            Model.Core.SplitViewPanePlacement placement => placement switch
            {
                Model.Core.SplitViewPanePlacement.Left => SplitViewPanePlacement.Left,
                Model.Core.SplitViewPanePlacement.Right => SplitViewPanePlacement.Right,
                Model.Core.SplitViewPanePlacement.Top => SplitViewPanePlacement.Top,
                Model.Core.SplitViewPanePlacement.Bottom => SplitViewPanePlacement.Bottom,
                _ => SplitViewPanePlacement.Left
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
            SplitViewPanePlacement placement => placement switch
            {
                SplitViewPanePlacement.Left => Model.Core.SplitViewPanePlacement.Left,
                SplitViewPanePlacement.Right => Model.Core.SplitViewPanePlacement.Right,
                SplitViewPanePlacement.Top => Model.Core.SplitViewPanePlacement.Top,
                SplitViewPanePlacement.Bottom => Model.Core.SplitViewPanePlacement.Bottom,
                _ => Model.Core.SplitViewPanePlacement.Left
            },
            _ => value
        };
    }
}
