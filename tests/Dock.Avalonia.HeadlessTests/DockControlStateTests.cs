using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlStateTests
{
    private static object CreateState(DockManager manager)
    {
        var type = typeof(DockControl).Assembly.GetType("Dock.Avalonia.Internal.DockControlState", throwOnError: true)!;
        var dragOffsetCalculator = new DefaultDragOffsetCalculator();
        return Activator.CreateInstance(type, manager, dragOffsetCalculator)!;
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
        var state = CreateState(manager);
        dynamic context = GetContext(state);

        var dragControl = new Control();
        context.Start(dragControl, new Point(0, 0));
        var dock = new DockControl();
        var docks = new List<IDockControl> { dock };

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new Point(10,10), new Vector(),
            EventType.Moved,
            DragAction.Move, dock, docks
        });

        Assert.True(context.DoDragDrop);
    }

    [AvaloniaFact]
    public void CaptureLost_Ends_Drag()
    {
        var manager = new DockManager();
        var state = CreateState(manager);
        dynamic context = GetContext(state);

        var dragControl = new Control();
        context.Start(dragControl, new Point(0, 0));
        context.DoDragDrop = true;
        var dock = new DockControl { IsDraggingDock = true };
        var docks = new List<IDockControl> { dock };

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new Point(), new Vector(),
            EventType.CaptureLost,
            DragAction.None, dock, docks
        });

        Assert.False(context.DoDragDrop);
        Assert.False(context.PointerPressed);
        Assert.False(dock.IsDraggingDock);
    }

    [AvaloniaFact]
    public void Small_Move_Does_Not_Start_Drag()
    {
        var manager = new DockManager();
        var state = CreateState(manager);
        dynamic context = GetContext(state);

        var dragControl = new Control();
        context.Start(dragControl, new Point(0, 0));
        var dock = new DockControl();
        var docks = new List<IDockControl> { dock };

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new Point(1,1), new Vector(),
            EventType.Moved,
            DragAction.Move, dock, docks
        });

        Assert.False(context.DoDragDrop);
    }

    [AvaloniaFact]
    public void Released_Ends_Drag()
    {
        var manager = new DockManager();
        var state = CreateState(manager);
        dynamic context = GetContext(state);

        var dragControl = new Control();
        context.Start(dragControl, new Point(0, 0));
        context.DoDragDrop = true;
        var dock = new DockControl { IsDraggingDock = true };
        var docks = new List<IDockControl> { dock };

        GetProcessMethod(state).Invoke(state, new object?[]
        {
            new Point(), new Vector(),
            EventType.Released,
            DragAction.None, dock, docks
        });

        Assert.False(context.PointerPressed);
        Assert.False(dock.IsDraggingDock);
    }
}
