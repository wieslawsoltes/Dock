using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutDocumentPaneConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is IDockable { Factory: LayoutFactory factory } model ? factory.GetLayoutContainer(model) as LayoutDocumentPane : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
