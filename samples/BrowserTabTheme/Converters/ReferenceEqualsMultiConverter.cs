using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BrowserTabTheme.Converters;

public sealed class ReferenceEqualsMultiConverter : IMultiValueConverter
{
    public static readonly ReferenceEqualsMultiConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
        {
            return false;
        }

        return ReferenceEquals(values[0], values[1]);
    }
}
