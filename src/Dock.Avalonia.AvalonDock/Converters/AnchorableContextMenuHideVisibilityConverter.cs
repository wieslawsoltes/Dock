using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AvalonDock.Converters;

/// <summary>Hides a tool's hide-menu entry when closing is available, preserving its existing visibility otherwise.</summary>
public class AnchorableContextMenuHideVisibilityConverter : IMultiValueConverter
{
    /// <summary>Combines the existing IsVisible value and CanClose using the AvalonDock array API.</summary>
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
        Combine(values.Length, values[0], values.Length > 1 ? values[1] : null);

    object? IMultiValueConverter.Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        Combine(values.Count, values[0], values.Count > 1 ? values[1] : null);

    /// <summary>Reverse conversion is not supported.</summary>
    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    private static object? Combine(int count, object? visible, object? canClose) =>
        count == 2 && !ReferenceEquals(visible, AvaloniaProperty.UnsetValue) && canClose is true ? false : visible;
}
