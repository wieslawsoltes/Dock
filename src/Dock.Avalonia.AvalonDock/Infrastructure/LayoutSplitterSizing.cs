using Avalonia;
using Avalonia.Controls;

namespace AvalonDock.Infrastructure;

// Inherited scalar values keep floating hosts independent of manager lifetime.
internal sealed class LayoutSplitterSizing : AvaloniaObject
{
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<LayoutSplitterSizing, Control, double>("Width", 6, inherits: true);
    public static readonly AttachedProperty<double> HeightProperty =
        AvaloniaProperty.RegisterAttached<LayoutSplitterSizing, Control, double>("Height", 6, inherits: true);

    public static double GetWidth(Control control) => control.GetValue(WidthProperty);
    public static void SetWidth(Control control, double value) => control.SetValue(WidthProperty, value);
    public static double GetHeight(Control control) => control.GetValue(HeightProperty);
    public static void SetHeight(Control control, double value) => control.SetValue(HeightProperty, value);
}
