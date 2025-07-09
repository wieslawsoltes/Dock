using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="Alignment"/> to <see cref="VerticalAlignment"/>.
/// </summary>
public class AlignmentToVerticalAlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets converter instance.
    /// </summary>
    public static readonly AlignmentToVerticalAlignmentConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Alignment.Top => VerticalAlignment.Top,
            Alignment.Bottom => VerticalAlignment.Bottom,
            _ => AvaloniaProperty.UnsetValue,
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            VerticalAlignment.Top => Alignment.Top,
            VerticalAlignment.Bottom => Alignment.Bottom,
            _ => Alignment.Unset,
        };
    }
}
