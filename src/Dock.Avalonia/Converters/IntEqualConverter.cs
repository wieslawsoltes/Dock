using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts an integer to a boolean indicating whether it is equal to a specified value.
/// </summary>
public class IntEqualConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the value to compare against. The converter will return true if the input integer is equal to this value.
    /// </summary>
    public int TrueIfEqual { get; set; }

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue == TrueIfEqual;
        }
        return false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
