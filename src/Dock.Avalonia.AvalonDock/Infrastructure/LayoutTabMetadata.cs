using Avalonia;
using Avalonia.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Gives compiled native-container styles access to the public metadata while
// retaining the native model as their DataContext. Bindings own subscriptions.
internal sealed class LayoutTabMetadata : AvaloniaObject
{
    public static readonly AttachedProperty<LayoutContent?> ModelProperty =
        AvaloniaProperty.RegisterAttached<LayoutTabMetadata, Control, LayoutContent?>("Model");

    public static LayoutContent? GetModel(Control control) => control.GetValue(ModelProperty);
    public static void SetModel(Control control, LayoutContent? value) => control.SetValue(ModelProperty, value);
}
