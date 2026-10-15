using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutDocumentWindowConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is IDockable { Factory: LayoutFactory factory } model
            ? factory.GetLayoutContainer(model) as LayoutDocumentFloatingWindow : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
