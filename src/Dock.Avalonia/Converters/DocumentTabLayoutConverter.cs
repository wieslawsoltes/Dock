using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using AC = Avalonia.Controls;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts <see cref="DocumentTabLayout"/> to Avalonia <see cref="Dock"/> or <see cref="Orientation"/>.
/// </summary>
public class DocumentTabLayoutConverter : IValueConverter
{
    /// <summary>
    /// Gets <see cref="DocumentTabLayoutConverter"/> instance.
    /// </summary>
    public static readonly DocumentTabLayoutConverter Instance = new DocumentTabLayoutConverter();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            DocumentTabLayout.Left => targetType == typeof(AC.Dock) ? AC.Dock.Left : Orientation.Vertical,
            DocumentTabLayout.Right => targetType == typeof(AC.Dock) ? AC.Dock.Right : Orientation.Vertical,
            DocumentTabLayout.Top => targetType == typeof(AC.Dock) ? AC.Dock.Top : Orientation.Horizontal,
            _ => AvaloniaProperty.UnsetValue
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            AC.Dock.Left => DocumentTabLayout.Left,
            AC.Dock.Right => DocumentTabLayout.Right,
            AC.Dock.Top => DocumentTabLayout.Top,
            Orientation.Vertical => DocumentTabLayout.Left,
            Orientation.Horizontal => DocumentTabLayout.Top,
            _ => AvaloniaProperty.UnsetValue
        };
    }
}
