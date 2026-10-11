using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Native Avalonia models are logical elements. Passing the public wrapper as
// content avoids competing logical parents in tab and deferred-content hosts.
internal sealed class LayoutContentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is IDockable { Factory: LayoutFactory factory } model ? factory.GetLayoutItem(model)?.LayoutElement : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
