using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Dock.Avalonia.Converters;

/// <summary>
/// Converts an <see cref="IList.Count"/> to a boolean indicating whether it is less than a specified value.
/// </summary>
public class ListCountLessThanConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the value to compare against. The converter will return true if the input <see cref="IList.Count"/> is less than this value.
    /// </summary>
    public int TrueIfLessThan { get; set; }

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IList list)
        {
            return list.Count < TrueIfLessThan;
        }
        return false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
