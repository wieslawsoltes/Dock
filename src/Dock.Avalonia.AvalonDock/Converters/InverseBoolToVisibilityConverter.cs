using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvalonDock.Converters;

/// <summary>Inverts Boolean values for Avalonia's Boolean IsVisible property.</summary>
/// <remarks>A Boolean parameter supplies the non-visible result. Both input and target must be Boolean.</remarks>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool flag && targetType == typeof(bool) ? !flag || parameter is true :
            throw new ArgumentException("The value and target type must be Boolean.");

    /// <summary>Inverts an IsVisible value; both input and target must be Boolean.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool flag && targetType == typeof(bool) ? !flag :
            throw new ArgumentException("The value and target type must be Boolean.");
}
