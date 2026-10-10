using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Resolves the existing container; native controls keep their native model.
internal sealed class LayoutItemConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is IDockable { Factory: LayoutFactory factory } model ? factory.GetLayoutItem(model) : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
