using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Dock.Avalonia.Internal;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class WindowDragHelperTests
{
    private static FieldInfo DragWindowField => typeof(WindowDragHelper).GetField("_dragWindow", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static FieldInfo PositionHandlerField => typeof(WindowDragHelper).GetField("_positionChangedHandler", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static FieldInfo PointerPressedField => typeof(WindowDragHelper).GetField("_pointerPressed", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static FieldInfo IsDraggingField => typeof(WindowDragHelper).GetField("_isDragging", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static int GetPositionChangedCount(WindowBase window)
    {
        var field = typeof(WindowBase).GetField("PositionChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var del = field.GetValue(window) as Delegate;
        return del?.GetInvocationList().Length ?? 0;
    }

    [AvaloniaFact]
    public void CaptureLost_Cleans_Drag_State()
    {
        var owner = new Control();
        var helper = new WindowDragHelper(owner, () => true, _ => true);
        var window = new Window();
        EventHandler<PixelPointEventArgs> handler = (_, _) => { };
        window.PositionChanged += handler;

        DragWindowField.SetValue(helper, window);
        PositionHandlerField.SetValue(helper, handler);
        PointerPressedField.SetValue(helper, true);
        IsDraggingField.SetValue(helper, true);

        Assert.Equal(1, GetPositionChangedCount(window));

        var pointer = new Pointer(0, PointerType.Mouse, true);
        var args = new PointerCaptureLostEventArgs(owner, pointer);
        var method = typeof(WindowDragHelper).GetMethod("OnPointerCaptureLost", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(helper, new object?[] { owner, args });

        Assert.Equal(0, GetPositionChangedCount(window));
        Assert.False((bool)PointerPressedField.GetValue(helper)!);
        Assert.False((bool)IsDraggingField.GetValue(helper)!);
        Assert.Null(DragWindowField.GetValue(helper));
        Assert.Null(PositionHandlerField.GetValue(helper));
    }

    [AvaloniaFact]
    public void Detach_WhenDragging_Cleans_State()
    {
        var owner = new Control();
        var helper = new WindowDragHelper(owner, () => true, _ => true);
        var window = new Window();
        EventHandler<PixelPointEventArgs> handler = (_, _) => { };
        window.PositionChanged += handler;

        DragWindowField.SetValue(helper, window);
        PositionHandlerField.SetValue(helper, handler);
        IsDraggingField.SetValue(helper, true);

        Assert.Equal(1, GetPositionChangedCount(window));

        helper.Detach();

        Assert.Equal(0, GetPositionChangedCount(window));
        Assert.Null(DragWindowField.GetValue(helper));
        Assert.Null(PositionHandlerField.GetValue(helper));
    }
}
