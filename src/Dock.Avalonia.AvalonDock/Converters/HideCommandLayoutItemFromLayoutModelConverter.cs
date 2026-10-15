using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvalonDock.Controls;

namespace AvalonDock.Converters;

/// <summary>Resolves layout content to its current anchorable container hide command.</summary>
public class HideCommandLayoutItemFromLayoutModelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        LayoutItemFromLayoutModelConverter.Resolve(value) switch
        {
            LayoutAnchorableItem item => item.HideCommand,
            LayoutItem => BindingOperations.DoNothing,
            object unavailable => unavailable,
            _ => null
        };

    /// <summary>Reverse conversion is not supported.</summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
