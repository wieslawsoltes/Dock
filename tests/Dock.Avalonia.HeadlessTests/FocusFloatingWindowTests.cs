using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FocusFloatingWindowTests
{
    [AvaloniaFact]
    public void Focusing_Tool_In_Floating_Window_Does_Not_Close_Window()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;

        var mainDock = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        mainDock.Factory = factory;
        factory.AddDockable(root, mainDock);
        root.ActiveDockable = mainDock;
        root.FocusedDockable = mainDock;

        var floatingRoot = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        floatingRoot.Factory = factory;
        var toolDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };
        toolDock.Factory = factory;
        factory.AddDockable(floatingRoot, toolDock);
        floatingRoot.ActiveDockable = toolDock;
        floatingRoot.FocusedDockable = toolDock;

        var tool = new Tool();
        factory.AddDockable(toolDock, tool);
        factory.SetActiveDockable(tool);
        factory.SetFocusedDockable(toolDock, tool);

        var window = new DockWindow { Layout = floatingRoot };
        factory.AddWindow(root, window);

        factory.SetFocusedDockable(toolDock, tool);

        Assert.Contains(window, root.Windows);
    }
}

