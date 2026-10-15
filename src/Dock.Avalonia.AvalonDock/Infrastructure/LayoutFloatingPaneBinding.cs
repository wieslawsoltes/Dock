using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Native restoration may make the pane visible after its tree notification.
// Observe visibility as well as SinglePane, then bind selection on that pane.
// This keeps presentation reactive without adding public model notifications.
internal sealed class LayoutFloatingPaneBinding : AvaloniaObject, IMultiValueConverter
{
    public static readonly AttachedProperty<ILayoutContentSelector?> PaneProperty =
        AvaloniaProperty.RegisterAttached<LayoutFloatingPaneBinding, Control, ILayoutContentSelector?>("Pane");

    public static ILayoutContentSelector? GetPane(Control control) => control.GetValue(PaneProperty);
    public static void SetPane(Control control, ILayoutContentSelector? value) => control.SetValue(PaneProperty, value);

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Count > 0 && values[0] is LayoutAnchorableFloatingWindow model ? model.SinglePane : null;
}
