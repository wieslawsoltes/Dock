using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace DockFigmaSample.Converters;

public class EnumEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        return value.Equals(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is not null)
        {
            return parameter;
        }

        return AvaloniaProperty.UnsetValue;
    }
}
