using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia.Data.Converters;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutToolCloseCommandConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count == 3 && values[0] is bool canClose ? values[canClose ? 1 : 2] as ICommand : null;
}
