using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using AvalonDock.Layout;

namespace AvalonDock.Converters;

/// <summary>Maps left and right auto-hide sides to vertical orientation and other sides to horizontal.</summary>
public class AnchorSideToOrientationConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (AnchorSide)value! is AnchorSide.Left or AnchorSide.Right ? Orientation.Vertical : Orientation.Horizontal;

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
