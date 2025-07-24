using System.Reflection;
using Avalonia.Input;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowCaptureTests
{
    [AvaloniaFact]
    public void CaptureLost_Resets_Dragging_State()
    {
        var window = new HostWindow();
        typeof(HostWindow).GetField("_mouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, true);
        typeof(HostWindow).GetField("_draggingWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, true);

        var ctor = typeof(PointerCaptureLostEventArgs).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(IPointer) }, null)!;
        var args = (PointerCaptureLostEventArgs)ctor.Invoke(new object?[] { window, new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true) });
        args.RoutedEvent = InputElement.PointerCaptureLostEvent;

        typeof(HostWindow).GetMethod("OnPointerCaptureLost", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(window, new object?[] { args });

        var mouseDown = (bool)typeof(HostWindow).GetField("_mouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;
        var dragging = (bool)typeof(HostWindow).GetField("_draggingWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;

        Assert.False(mouseDown);
        Assert.False(dragging);
    }
}
