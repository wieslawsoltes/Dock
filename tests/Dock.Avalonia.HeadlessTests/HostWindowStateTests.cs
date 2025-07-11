using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using System.Reflection;
using Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowStateTests
{
    private static HostWindowState CreateState(DockManager manager, HostWindow window)
    {
        return new HostWindowState(manager, window);
    }

    [AvaloniaFact]
    public void Process_CaptureLost_Resets_State()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);

        state.Process(new PixelPoint(0,0), EventType.Pressed);
        state.Process(new PixelPoint(10,10), EventType.Moved);
        state.Process(new PixelPoint(), EventType.CaptureLost);

        Assert.NotNull(window.HostWindowState);
    }

    [AvaloniaFact]
    public void Process_Released_Completes_Drag()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);

        state.Process(new PixelPoint(0,0), EventType.Pressed);
        state.Process(new PixelPoint(10,10), EventType.Moved);
        state.Process(new PixelPoint(), EventType.Released);

        Assert.NotNull(window.HostWindowState);
    }

    [AvaloniaFact]
    public void Move_OutsideDockControl_Resets_DropControl()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var factory = new Factory();
        var layout = factory.CreateRootDock();
        var dockWindow = new DockWindow { Factory = factory, Layout = layout };
        window.Window = dockWindow;
        var state = CreateState(manager, window);

        state.Process(new PixelPoint(0,0), EventType.Pressed);
        state.Process(new PixelPoint(10,10), EventType.Moved);

        var dropControlProperty = typeof(DockManagerState)
            .GetProperty("DropControl", BindingFlags.Instance | BindingFlags.NonPublic);
        dropControlProperty!.SetValue(state, new Control());

        var contextField = typeof(HostWindowState)
            .GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var context = contextField.GetValue(state)!;
        context.GetType().GetProperty("TargetDockControl")!.SetValue(context, new DockControl());
        context.GetType().GetProperty("TargetPoint")!.SetValue(context, new Point(5,5));
        context.GetType().GetProperty("DoDragDrop")!.SetValue(context, true);
        context.GetType().GetProperty("PointerPressed")!.SetValue(context, true);

        state.Process(new PixelPoint(20,20), EventType.Moved);

        Assert.Null(dropControlProperty.GetValue(state));
        Assert.Null(context.GetType().GetProperty("TargetDockControl")!.GetValue(context));
        Assert.Equal(default(Point), (Point)context.GetType().GetProperty("TargetPoint")!.GetValue(context)!);
    }
}
