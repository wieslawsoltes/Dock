using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using System.Linq;

namespace Dock.Avalonia.HeadlessTests;

public class FactorySplitterDragRestoreTests
{
    [AvaloniaFact]
    public void FloatDockable_RemovesSplitters_FromOriginalLocation()
    {
        var factory = new Factory();
        var root = new RootDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        mainLayout.Factory = factory;

        var leftPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        leftPane.Factory = factory;

        var splitter = factory.CreateProportionalDockSplitter();

        var rightPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        rightPane.Factory = factory;

        factory.AddDockable(mainLayout, leftPane);
        factory.AddDockable(mainLayout, splitter);
        factory.AddDockable(mainLayout, rightPane);

        factory.AddDockable(root, mainLayout);

        // Float the rightPane
        factory.FloatDockable(rightPane);

        // Verify the splitter is removed
        Assert.Single(mainLayout.VisibleDockables!);
        Assert.Equal(leftPane, mainLayout.VisibleDockables[0]);
        Assert.DoesNotContain(splitter, mainLayout.VisibleDockables);
    }

    [AvaloniaFact]
    public void FloatDockable_HandlesComplexLayout_WithMultipleSplitters()
    {
        var factory = new Factory();
        var root = new RootDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        mainLayout.Factory = factory;

        var leftPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        leftPane.Factory = factory;

        var splitter1 = factory.CreateProportionalDockSplitter();

        var middlePane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        middlePane.Factory = factory;

        var splitter2 = factory.CreateProportionalDockSplitter();

        var rightPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        rightPane.Factory = factory;

        factory.AddDockable(mainLayout, leftPane);
        factory.AddDockable(mainLayout, splitter1);
        factory.AddDockable(mainLayout, middlePane);
        factory.AddDockable(mainLayout, splitter2);
        factory.AddDockable(mainLayout, rightPane);

        factory.AddDockable(root, mainLayout);

        // Float the middlePane
        factory.FloatDockable(middlePane);

        // Verify both splitters are removed
        Assert.Equal(3, mainLayout.VisibleDockables!.Count);
        Assert.Equal(leftPane, mainLayout.VisibleDockables[0]);
        Assert.Equal(splitter1, mainLayout.VisibleDockables[1]);
        Assert.Equal(rightPane, mainLayout.VisibleDockables[2]);
        Assert.DoesNotContain(splitter2, mainLayout.VisibleDockables);
    }

    [AvaloniaFact]
    public void FloatDockable_PreservesProportions_OfRemainingDockables()
    {
        var factory = new Factory();
        var root = new RootDock 
        { 
            VisibleDockables = factory.CreateList<IDockable>(),
            Windows = factory.CreateList<IDockWindow>()
        };
        root.Factory = factory;

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        mainLayout.Factory = factory;

        var leftPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.3
        };
        leftPane.Factory = factory;

        var splitter = factory.CreateProportionalDockSplitter();

        var rightPane = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.7
        };
        rightPane.Factory = factory;

        factory.AddDockable(mainLayout, leftPane);
        factory.AddDockable(mainLayout, splitter);
        factory.AddDockable(mainLayout, rightPane);

        factory.AddDockable(root, mainLayout);

        // Float the rightPane
        factory.FloatDockable(rightPane);

        // Verify the leftPane proportion is preserved
        Assert.Equal(0.3, leftPane.Proportion);
    }
} 