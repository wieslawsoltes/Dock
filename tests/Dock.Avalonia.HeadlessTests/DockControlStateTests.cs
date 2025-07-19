using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockControlStateTests
{
    private static DockControlState CreateState(DockManager manager)
    {
        return new DockControlState(manager, new DefaultDragOffsetCalculator());
    }


    [AvaloniaFact]
    public void Process_CaptureLost_Ends_Drag()
    {
        var manager = new DockManager();
        var state = CreateState(manager);
        var dock = new DockControl(manager, state);
        var docks = new List<IDockControl> { dock };

        state.Process(new Point(0,0), new Vector(), EventType.Pressed, DragAction.Move, dock, docks);
        state.Process(new Point(10,10), new Vector(), EventType.Moved, DragAction.Move, dock, docks);
        state.Process(new Point(), new Vector(), EventType.CaptureLost, DragAction.None, dock, docks);

        Assert.False(dock.IsDraggingDock);
    }

    [AvaloniaFact]
    public void Process_Released_Ends_Drag()
    {
        var manager = new DockManager();
        var state = CreateState(manager);
        var dock = new DockControl(manager, state);
        var docks = new List<IDockControl> { dock };

        state.Process(new Point(0,0), new Vector(), EventType.Pressed, DragAction.Move, dock, docks);
        state.Process(new Point(10,10), new Vector(), EventType.Moved, DragAction.Move, dock, docks);
        state.Process(new Point(), new Vector(), EventType.Released, DragAction.None, dock, docks);

        Assert.False(dock.IsDraggingDock);
    }
}
