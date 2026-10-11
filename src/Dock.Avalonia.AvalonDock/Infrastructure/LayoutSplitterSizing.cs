using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;

namespace AvalonDock.Infrastructure;

// Inherited scalar values keep floating hosts independent of manager lifetime.
internal sealed class LayoutSplitterSizing : AvaloniaObject, IMultiValueConverter
{
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<LayoutSplitterSizing, Control, double>("Width", 6, inherits: true);
    public static readonly AttachedProperty<double> HeightProperty =
        AvaloniaProperty.RegisterAttached<LayoutSplitterSizing, Control, double>("Height", 6, inherits: true);

    public static double GetWidth(Control control) => control.GetValue(WidthProperty);
    public static void SetWidth(Control control, double value) => control.SetValue(WidthProperty, value);
    public static double GetHeight(Control control) => control.GetValue(HeightProperty);
    public static void SetHeight(Control control, double value) => control.SetValue(HeightProperty, value);

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count == 3 && values[0] is double width && values[1] is double height && values[2] is Orientation orientation
            ? orientation == Orientation.Horizontal ? width : height : 6d;
}
