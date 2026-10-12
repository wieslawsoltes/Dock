using Avalonia;
using Avalonia.Controls;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Template bindings supply the public model and the XAML-defined preview.
// The native PinnedDockControl continues to own layout and pinned bounds.
internal sealed class LayoutAutoHideResize : AvaloniaObject
{
    public static readonly AttachedProperty<LayoutAnchorable?> ModelProperty =
        AvaloniaProperty.RegisterAttached<LayoutAutoHideResize, LayoutGridResizerControl, LayoutAnchorable?>("Model");
    public static readonly AttachedProperty<Border?> PreviewProperty =
        AvaloniaProperty.RegisterAttached<LayoutAutoHideResize, LayoutGridResizerControl, Border?>("Preview");
    private static readonly AttachedProperty<LayoutAutoHideResizeBehavior?> BehaviorProperty =
        AvaloniaProperty.RegisterAttached<LayoutAutoHideResize, LayoutGridResizerControl, LayoutAutoHideResizeBehavior?>("Behavior");

    static LayoutAutoHideResize()
    {
        ModelProperty.Changed.AddClassHandler<LayoutGridResizerControl>((control, _) => EnsureBehavior(control));
        PreviewProperty.Changed.AddClassHandler<LayoutGridResizerControl>((control, _) => EnsureBehavior(control));
    }

    public static LayoutAnchorable? GetModel(LayoutGridResizerControl control) => control.GetValue(ModelProperty);
    public static void SetModel(LayoutGridResizerControl control, LayoutAnchorable? value) => control.SetValue(ModelProperty, value);
    public static Border? GetPreview(LayoutGridResizerControl control) => control.GetValue(PreviewProperty);
    public static void SetPreview(LayoutGridResizerControl control, Border? value) => control.SetValue(PreviewProperty, value);

    internal static void Cancel(LayoutGridResizerControl control) => control.GetValue(BehaviorProperty)?.Cancel();
    internal static bool IsResizing(LayoutGridResizerControl control) => control.GetValue(BehaviorProperty)?.IsResizing == true;
    internal static bool IsAutoHideResizer(LayoutGridResizerControl control) => control.GetValue(BehaviorProperty) is not null;

    private static void EnsureBehavior(LayoutGridResizerControl control)
    {
        if (control.GetValue(BehaviorProperty) is null)
            control.SetValue(BehaviorProperty, new LayoutAutoHideResizeBehavior(control));
    }
}
