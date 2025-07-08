using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactorySplitTests
{
    [AvaloniaFact]
    public void CollapseDock_Removes_Empty_Dock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock
        {
            IsCollapsable = true,
            Alignment = Alignment.Left,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, toolDock);

        factory.CollapseDock(toolDock);

        Assert.Empty(root.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CreateSplitLayout_Left_Creates_Horizontal_Layout()
    {
        var factory = new Factory();
        var container = new ProportionalDock
        {
            Proportion = 0.3,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        container.Factory = factory;
        var doc = new Document();

        var layout = factory.CreateSplitLayout(container, doc, DockOperation.Left);

        Assert.IsType<ProportionalDock>(layout);
        Assert.Equal(Orientation.Horizontal, (layout as ProportionalDock)!.Orientation);
        Assert.Equal(3, layout.VisibleDockables!.Count);
        Assert.Same(container, layout.VisibleDockables[2]);
        Assert.True(double.IsNaN(container.Proportion));
    }

    [AvaloniaFact]
    public void SplitToDock_Right_Replaces_Dock_With_Layout()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, dock);
        var doc = new Document();

        factory.SplitToDock(dock, doc, DockOperation.Right);

        var layout = Assert.IsType<ProportionalDock>(root.VisibleDockables![0]);
        Assert.Equal(Orientation.Horizontal, layout.Orientation);
        Assert.Equal(3, layout.VisibleDockables!.Count);
    }

    [AvaloniaFact]
    public void SplitToWindow_Creates_New_Window_With_Dockable()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;
        var doc = new Document();
        factory.AddDockable(root, doc);

        factory.SplitToWindow(root, doc, 10, 20, 300, 200);

        Assert.Empty(root.VisibleDockables!);
        Assert.Single(root.Windows!);
        var window = root.Windows[0];
        Assert.NotNull(window.Layout);
    }
}
