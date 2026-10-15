using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AvalonDock.Converters;

/// <summary>Preserves the current binding target for null values and passes other values through.</summary>
public class NullToDoNothingConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value ?? BindingOperations.DoNothing;

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
