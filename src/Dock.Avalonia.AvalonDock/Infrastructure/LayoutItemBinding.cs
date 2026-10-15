using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Command chrome can be realized while a source strategy pumps the dispatcher.
// Keep the native model as the input and publish its container when registered.
internal sealed class LayoutItemBinding : AvaloniaObject
{
    public static readonly AttachedProperty<object?> ModelProperty =
        AvaloniaProperty.RegisterAttached<LayoutItemBinding, Control, object?>("Model");
    public static readonly AttachedProperty<LayoutItem?> ItemProperty =
        AvaloniaProperty.RegisterAttached<LayoutItemBinding, Control, LayoutItem?>("Item");
    private static readonly AttachedProperty<LayoutControlModelBinding?> BindingProperty =
        AvaloniaProperty.RegisterAttached<LayoutItemBinding, Control, LayoutControlModelBinding?>("Binding");

    static LayoutItemBinding() => ModelProperty.Changed.AddClassHandler<Control>((control, _) => Bind(control));

    public static object? GetModel(Control control) => control.GetValue(ModelProperty);
    public static void SetModel(Control control, object? value) => control.SetValue(ModelProperty, value);
    public static LayoutItem? GetItem(Control control) => control.GetValue(ItemProperty);

    private static void Bind(Control control)
    {
        LayoutContent? model = GetModel(control) switch
        {
            LayoutContent content => content,
            IDockable { Factory: LayoutFactory factory } native => factory.GetLayoutContent(native),
            _ => null
        };
        LayoutControlModelBinding? binding = control.GetValue(BindingProperty);
        if (binding is null)
        {
            binding = new(control, item =>
            {
                control.SetValue(ItemProperty, item);
                binding?.WatchItem(item);
            }, () => control.SetCurrentValue(ModelProperty, null))
                { IsNativePresentation = control is not LayoutFloatingWindowControl };
            control.SetValue(BindingProperty, binding);
        }
        binding.SetModel(model);
    }
}
