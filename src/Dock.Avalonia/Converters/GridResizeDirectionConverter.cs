// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts Dock grid splitter directions to Avalonia grid splitter directions.
/// </summary>
public sealed class GridResizeDirectionConverter : IValueConverter
{
    /// <summary>
    /// Gets the shared, stateless converter.
    /// </summary>
    public static readonly GridResizeDirectionConverter Instance = new();

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Model.Core.GridResizeDirection.Rows => GridResizeDirection.Rows,
            Model.Core.GridResizeDirection.Columns => GridResizeDirection.Columns,
            Model.Core.GridResizeDirection => GridResizeDirection.Auto,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
