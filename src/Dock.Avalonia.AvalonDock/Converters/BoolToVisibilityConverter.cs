using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvalonDock.Converters;

/// <summary>Converts Boolean values to Avalonia's Boolean IsVisible value.</summary>
/// <remarks>A Boolean parameter supplies the false/null result. Hidden and collapsed
/// WPF visibility both correspond to false in Avalonia.</remarks>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return parameter is true;
        return value is not bool flag || targetType != typeof(bool) || flag || parameter is true;
    }

    /// <summary>Converts an IsVisible value back to Boolean; both input and target must be Boolean.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool flag && targetType == typeof(bool) ? flag :
            throw new ArgumentException("The value and target type must be Boolean.");
}
