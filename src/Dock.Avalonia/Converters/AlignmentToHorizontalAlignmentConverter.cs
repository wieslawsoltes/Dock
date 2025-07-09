using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="Alignment"/> to <see cref="HorizontalAlignment"/>.
/// </summary>
public class AlignmentToHorizontalAlignmentConverter : IValueConverter
{
    /// <summary>
    /// Gets converter instance.
    /// </summary>
    public static readonly AlignmentToHorizontalAlignmentConverter Instance = new();

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Alignment.Left => HorizontalAlignment.Left,
            Alignment.Right => HorizontalAlignment.Right,
            _ => AvaloniaProperty.UnsetValue,
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            HorizontalAlignment.Left => Alignment.Left,
            HorizontalAlignment.Right => Alignment.Right,
            _ => Alignment.Unset,
        };
    }
}
