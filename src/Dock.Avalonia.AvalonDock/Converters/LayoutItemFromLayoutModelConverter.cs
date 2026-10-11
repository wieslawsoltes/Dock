using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvalonDock.Layout;

namespace AvalonDock.Converters;

/// <summary>Resolves a public layout content model to its manager-owned item container.</summary>
public class LayoutItemFromLayoutModelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LayoutContent { Root.Manager: { } manager } content) return null;
        return (object?)manager.GetLayoutItemFromModel(content) ?? BindingOperations.DoNothing;
    }

    /// <summary>Reverse conversion is not supported by the AvalonDock contract.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
