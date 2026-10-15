using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvalonDock.Layout;

namespace AvalonDock.Converters;

/// <summary>Rotates left and right auto-hide headers by 90 degrees, leaving other angles unchanged.</summary>
public class AnchorSideToAngleConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (AnchorSide)value! is AnchorSide.Left or AnchorSide.Right ? 90d : BindingOperations.DoNothing;

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
