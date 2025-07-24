using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class WindowDragHelperTests
{
    [AvaloniaFact]
    public void CaptureLost_Cleans_Up_State()
    {
        var owner = new Control();
        var helper = new WindowDragHelper(owner, () => true, _ => true);
        var window = new Window();
        var handler = new EventHandler<PixelPointEventArgs>((_, _) => { });
        var fieldDragWindow = typeof(WindowDragHelper).GetField("_dragWindow", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var fieldHandler = typeof(WindowDragHelper).GetField("_positionChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic)!;
        fieldDragWindow.SetValue(helper, window);
        fieldHandler.SetValue(helper, handler);
        window.PositionChanged += handler;
        typeof(WindowDragHelper).GetMethod("Attach")!.Invoke(helper, null);

        var captureArgsCtor = typeof(PointerCaptureLostEventArgs).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(IPointer) }, null)!;
        var args = (PointerCaptureLostEventArgs)captureArgsCtor.Invoke(new object?[] { owner, new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true) });
        args.RoutedEvent = InputElement.PointerCaptureLostEvent;

        typeof(WindowDragHelper).GetMethod("OnPointerCaptureLost", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(helper, new object?[] { owner, args });

        Assert.Null(fieldDragWindow.GetValue(helper));
        Assert.Null(fieldHandler.GetValue(helper));
    }

    [AvaloniaFact]
    public void Detach_Cleans_Up_Drag_State()
    {
        var owner = new Control();
        var helper = new WindowDragHelper(owner, () => true, _ => true);
        var window = new Window();
        var handler = new EventHandler<PixelPointEventArgs>((_, _) => { });
        var fieldDragWindow = typeof(WindowDragHelper).GetField("_dragWindow", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var fieldHandler = typeof(WindowDragHelper).GetField("_positionChangedHandler", BindingFlags.Instance | BindingFlags.NonPublic)!;
        fieldDragWindow.SetValue(helper, window);
        fieldHandler.SetValue(helper, handler);
        window.PositionChanged += handler;

        helper.Detach();

        Assert.Null(fieldDragWindow.GetValue(helper));
        Assert.Null(fieldHandler.GetValue(helper));
    }
}
