using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlPointerButtonTests
{
    private static DockControl CreateDockControl()
    {
        var factory = new Factory();
        var layout = factory.CreateLayout();
        layout.Factory = factory;
        var control = new DockControl { Factory = factory, Layout = layout };
        DockProperties.SetIsDragArea(control, true);
        return control;
    }

    private static PointerPressedEventArgs CreateArgs(DockControl control, MouseButton button)
    {
        var pointer = new global::Avalonia.Input.Pointer(0, PointerType.Mouse, true);
        var updateKind = button switch
        {
            MouseButton.Left => PointerUpdateKind.LeftButtonPressed,
            MouseButton.Right => PointerUpdateKind.RightButtonPressed,
            _ => PointerUpdateKind.Other
        };
        var props = new PointerPointProperties(RawInputModifiers.None, updateKind);
        return new PointerPressedEventArgs(control, pointer, control, new Point(0, 0), 0, props, KeyModifiers.None);
    }

    [AvaloniaFact]
    public void RightButton_Does_Not_Start_Drag()
    {
        var control = CreateDockControl();
        var method = typeof(DockControl).GetMethod("PressedHandler", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var args = CreateArgs(control, MouseButton.Right);

        method.Invoke(control, new object?[] { control, args });

        Assert.False(control.IsDraggingDock);
    }
}
