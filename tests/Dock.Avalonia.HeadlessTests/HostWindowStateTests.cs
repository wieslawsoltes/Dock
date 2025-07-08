using System;
using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowStateTests
{
    private static object CreateState(DockManager manager, HostWindow window)
    {
        var type = typeof(HostWindow).Assembly.GetType("Dock.Avalonia.Internal.HostWindowState", throwOnError: true)!;
        return Activator.CreateInstance(type, manager, window)!;
    }

    private static dynamic GetContext(object state)
    {
        var field = state.GetType().GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return field.GetValue(state)!;
    }

    private static MethodInfo GetProcessMethod(object state) =>
        state.GetType().GetMethod("Process", BindingFlags.Public | BindingFlags.Instance)!;

    [AvaloniaFact]
    public void StartDrag_OnMove_Sets_DoDragDrop()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);
        dynamic context = GetContext(state);

        context.Start(new PixelPoint(0,0));

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new PixelPoint(10,10),
            EventType.Moved
        });

        Assert.True(context.DoDragDrop);
    }

    [AvaloniaFact]
    public void CaptureLost_Ends_Drag()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);
        dynamic context = GetContext(state);

        context.Start(new PixelPoint(0,0));
        context.DoDragDrop = true;

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new PixelPoint(),
            EventType.CaptureLost
        });

        Assert.False(context.DoDragDrop);
        Assert.False(context.PointerPressed);
    }

    [AvaloniaFact]
    public void Small_Move_Does_Not_Start_Drag()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);
        dynamic context = GetContext(state);

        context.Start(new PixelPoint(0,0));

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new PixelPoint(1,1),
            EventType.Moved
        });

        Assert.False(context.DoDragDrop);
    }

    [AvaloniaFact]
    public void Released_Ends_Drag()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);
        dynamic context = GetContext(state);

        context.Start(new PixelPoint(0,0));
        context.DoDragDrop = true;

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new PixelPoint(),
            EventType.Released
        });

        Assert.False(context.PointerPressed);
    }
}
