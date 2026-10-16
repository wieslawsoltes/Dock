using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutPaneModelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as LayoutElement)?.DockModel;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
