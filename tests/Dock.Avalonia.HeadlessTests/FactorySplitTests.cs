using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Controls;
using Xunit;
using Avalonia.Controls;
using System.ComponentModel;

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
        
        // The second item should be a splitter, and the third should be the new dock container
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        var newDocContainer = Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables![2]);
        Assert.True(double.IsNaN(newDocContainer.Proportion));
    }

    [AvaloniaFact]
    public void ProportionalDock_Proportion_Changes_Trigger_PropertyChanged()
    {
        var factory = new Factory();
        var dock = new ProportionalDock
        {
            Proportion = 0.5,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var propertyChangedCount = 0;
        string? changedPropertyName = null;

        dock.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == nameof(ProportionalDock.Proportion))
            {
                propertyChangedCount++;
                changedPropertyName = args.Property.Name;
            }
        };

        // Change the proportion
        dock.Proportion = 0.8;

        // Verify PropertyChanged was raised
        Assert.Equal(1, propertyChangedCount);
        Assert.Equal(nameof(ProportionalDock.Proportion), changedPropertyName);
    }

    [AvaloniaFact]
    public void SplitToDock_Optimization_CleansUp_OrphanedSplitters()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        // Create a scenario where we might have orphaned splitters
        var splitter1 = factory.CreateProportionalDockSplitter(); // This will become orphaned
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.5 };
        var splitter2 = factory.CreateProportionalDockSplitter();
        var dock2 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.5 };
        
        // Add them in an order that creates a splitter at the beginning (orphaned)
        factory.AddDockable(proportionalDock, splitter1); // Orphaned splitter at beginning
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, splitter2);
        factory.AddDockable(proportionalDock, dock2);
        
        var newDoc = new Document();

        // Split dock1 - this should trigger splitter cleanup
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Verify that orphaned splitters were cleaned up
        // The optimization should reuse the existing container and clean up the orphaned splitter1
        var visibleCount = proportionalDock.VisibleDockables!.Count;
        
        // Should have: dock1, splitter (new), newDoc container, splitter2, dock2 = 5 items
        // The orphaned splitter1 at the beginning should have been removed
        Assert.Equal(5, visibleCount);
        
        // Verify the first item is dock1, not a splitter
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]); // New splitter
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[2]); // New doc container
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[3]); // Original splitter2
        Assert.Same(dock2, proportionalDock.VisibleDockables[4]);
    }

    [AvaloniaFact]
    public void SplitToDock_Optimization_CleansUp_ConsecutiveSplitters()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        // Create a scenario with consecutive splitters
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.3 };
        var splitter1 = factory.CreateProportionalDockSplitter();
        var splitter2 = factory.CreateProportionalDockSplitter(); // Consecutive splitter
        var dock2 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.7 };
        
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, splitter1);
        factory.AddDockable(proportionalDock, splitter2); // This creates consecutive splitters
        factory.AddDockable(proportionalDock, dock2);
        
        var newDoc = new Document();

        // Split dock1 - this should trigger splitter cleanup
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Verify that consecutive splitters were cleaned up
        var visibleCount = proportionalDock.VisibleDockables!.Count;
        
        // Should clean up one of the consecutive splitters
        // Expected: dock1, splitter (new), newDoc container, splitter (one of the original), dock2 = 5 items
        Assert.Equal(5, visibleCount);
        
        // Verify no consecutive splitters remain
        for (int i = 0; i < visibleCount - 1; i++)
        {
            if (proportionalDock.VisibleDockables[i] is IProportionalDockSplitter)
            {
                Assert.False(proportionalDock.VisibleDockables[i + 1] is IProportionalDockSplitter, 
                    $"Found consecutive splitters at positions {i} and {i + 1}");
            }
        }
    }

    [AvaloniaFact]
    public void SplitToDock_And_RemoveDockable_Keeps_Functional_Splitter()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.5 };
        var dock2 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.5 };
        
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, dock2);
        
        var newDoc = new Document();

        // Split dock1 - this should use optimization and add splitter
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Verify we have the expected layout: dock1, splitter, newDoc container, dock2
        Assert.Equal(4, proportionalDock.VisibleDockables!.Count);
        
        // Now remove the new document container - this should keep the splitter between dock1 and dock2
        var newDocContainer = proportionalDock.VisibleDockables[2];
        factory.RemoveDockable(newDocContainer, false);
        
        // Should be: dock1, splitter, dock2 (splitter between two docks is NOT orphaned)
        Assert.Equal(3, proportionalDock.VisibleDockables.Count);
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.Same(dock2, proportionalDock.VisibleDockables[2]);
        
        // Verify the splitter is functional (between two non-splitter dockables)
        Assert.False(proportionalDock.VisibleDockables[0] is IProportionalDockSplitter);
        Assert.True(proportionalDock.VisibleDockables[1] is IProportionalDockSplitter);
        Assert.False(proportionalDock.VisibleDockables[2] is IProportionalDockSplitter);
    }
}
