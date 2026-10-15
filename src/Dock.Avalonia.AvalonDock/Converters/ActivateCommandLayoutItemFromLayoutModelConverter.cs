using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvalonDock.Controls;

namespace AvalonDock.Converters;

/// <summary>Resolves layout content to its current container activation command.</summary>
public class ActivateCommandLayoutItemFromLayoutModelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        object? item = LayoutItemFromLayoutModelConverter.Resolve(value);
        return item is LayoutItem container ? container.ActivateCommand : item;
    }

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
