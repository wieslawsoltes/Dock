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

    [AvaloniaFact]
    public void SplitToDock_In_ProportionalDock_With_Same_Orientation_Reuses_Existing_Container()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.6 };
        var dock2 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, dock2);
        
        var newDoc = new Document();

        // Split dock1 to the right - should reuse the existing horizontal ProportionalDock
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Verify the proportional dock was reused (no nested ProportionalDock created)
        Assert.Equal(4, proportionalDock.VisibleDockables!.Count); // dock1, splitter, newDoc container, dock2
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[2]); // newDoc container
        Assert.Same(dock2, proportionalDock.VisibleDockables[3]);
        
        // Verify the document is in its own ProportionalDock container
        var newDocContainer = Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[2]);
        Assert.Single(newDocContainer.VisibleDockables!);
        Assert.Same(newDoc, newDocContainer.VisibleDockables![0]);
        
        // Verify proportions are correctly split - original 0.6 should be split into 0.3 each
        Assert.Equal(0.3, dock1.Proportion, 3);
        Assert.Equal(0.3, newDocContainer.Proportion, 3);
    }

    [AvaloniaFact]
    public void SplitToDock_In_ProportionalDock_With_Different_Orientation_Creates_Nested_Layout()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(proportionalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the top - should create nested layout due to different orientation
        factory.SplitToDock(dock1, newDoc, DockOperation.Top);

        // Verify a nested ProportionalDock was created
        Assert.Single(proportionalDock.VisibleDockables!);
        var nestedLayout = Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Vertical, nestedLayout.Orientation);
        Assert.Equal(3, nestedLayout.VisibleDockables!.Count); // newDoc container, splitter, dock1
    }

    [AvaloniaFact]
    public void SplitToDock_With_NaN_Proportion_Maintains_NaN_For_Both_Docks()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = double.NaN };
        factory.AddDockable(proportionalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the right - should reuse the existing horizontal ProportionalDock
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Verify proportions remain NaN when original was NaN
        Assert.True(double.IsNaN(dock1.Proportion));
        
        var newDocContainer = Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables![1]);
        Assert.True(double.IsNaN(newDocContainer.Proportion));
    }
}
