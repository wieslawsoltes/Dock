using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Controls;
using Xunit;
using Avalonia.Controls;
using System;
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

    [AvaloniaFact]
    public void SplitToDock_Left_With_Compatible_Horizontal_Owner_Uses_Optimization()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.5 };
        factory.AddDockable(proportionalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the left - should use optimization path
        factory.SplitToDock(dock1, newDoc, DockOperation.Left);

        // Should have: newDoc container, splitter, dock1
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[0]); // newDoc container
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.Same(dock1, proportionalDock.VisibleDockables[2]);
        
        // Verify proportions are split correctly
        Assert.Equal(0.25, dock1.Proportion, 3);
        var newDocContainer = (ProportionalDock)proportionalDock.VisibleDockables[0];
        Assert.Equal(0.25, newDocContainer.Proportion, 3);
    }

    [AvaloniaFact]
    public void SplitToDock_Top_With_Compatible_Vertical_Owner_Uses_Optimization()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.8 };
        factory.AddDockable(proportionalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the top - should use optimization path
        factory.SplitToDock(dock1, newDoc, DockOperation.Top);

        // Should have: newDoc container, splitter, dock1
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[0]); // newDoc container
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.Same(dock1, proportionalDock.VisibleDockables[2]);
    }

    [AvaloniaFact]
    public void SplitToDock_Bottom_With_Compatible_Vertical_Owner_Uses_Optimization()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.6 };
        factory.AddDockable(proportionalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the bottom - should use optimization path
        factory.SplitToDock(dock1, newDoc, DockOperation.Bottom);

        // Should have: dock1, splitter, newDoc container
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[2]); // newDoc container
    }

    [AvaloniaFact]
    public void SplitToDock_With_IDock_Dockable_Uses_Dockable_Directly()
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
        
        var dockToSplit = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };

        // Split with IDock - should use the dock directly instead of wrapping it
        factory.SplitToDock(dock1, dockToSplit, DockOperation.Right);

        // Should have: dock1, splitter, dockToSplit
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.Same(dockToSplit, proportionalDock.VisibleDockables[2]); // Used directly, not wrapped
    }

    [AvaloniaFact]
    public void SplitToDock_Left_With_Incompatible_Vertical_Owner_Creates_Nested_Layout()
    {
        var factory = new Factory();
        var verticalDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        verticalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.7 };
        factory.AddDockable(verticalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the left - incompatible with vertical owner, should create nested layout
        factory.SplitToDock(dock1, newDoc, DockOperation.Left);

        // Should have created a new horizontal layout that replaced dock1
        Assert.Single(verticalDock.VisibleDockables!);
        var nestedLayout = Assert.IsType<ProportionalDock>(verticalDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Horizontal, nestedLayout.Orientation);
        Assert.Equal(0.7, nestedLayout.Proportion, 3); // Inherited from dock1
        Assert.True(double.IsNaN(dock1.Proportion)); // dock1's proportion reset
        
        // The nested layout should contain: newDoc container, splitter, dock1
        Assert.Equal(3, nestedLayout.VisibleDockables!.Count);
        Assert.IsType<ProportionalDock>(nestedLayout.VisibleDockables[0]); // newDoc container
        Assert.IsType<ProportionalDockSplitter>(nestedLayout.VisibleDockables[1]);
        Assert.Same(dock1, nestedLayout.VisibleDockables[2]);
    }

    [AvaloniaFact]
    public void SplitToDock_Top_With_Incompatible_Horizontal_Owner_Creates_Nested_Layout()
    {
        var factory = new Factory();
        var horizontalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        horizontalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Proportion = 0.4 };
        factory.AddDockable(horizontalDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 to the top - incompatible with horizontal owner, should create nested layout
        factory.SplitToDock(dock1, newDoc, DockOperation.Top);

        // Should have created a new vertical layout
        Assert.Single(horizontalDock.VisibleDockables!);
        var nestedLayout = Assert.IsType<ProportionalDock>(horizontalDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Vertical, nestedLayout.Orientation);
        Assert.Equal(0.4, nestedLayout.Proportion, 3);
    }

    [AvaloniaFact]
    public void SplitToDock_With_Non_ProportionalDock_Owner_Creates_Nested_Layout()
    {
        var factory = new Factory();
        var rootDock = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        rootDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(rootDock, dock1);
        
        var newDoc = new Document();

        // Split dock1 - owner is not ProportionalDock, should use fallback path
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Should have replaced dock1 with a new layout
        Assert.Single(rootDock.VisibleDockables!);
        var layout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Horizontal, layout.Orientation);
    }

    [AvaloniaFact]
    public void SplitToDock_With_Multiple_Docks_In_Compatible_Owner_Maintains_Order()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var dock2 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var dock3 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, dock2);
        factory.AddDockable(proportionalDock, dock3);
        
        var newDoc = new Document();

        // Split dock2 (middle dock) to the right
        factory.SplitToDock(dock2, newDoc, DockOperation.Right);

        // Should have: dock1, dock2, splitter, newDoc container, dock3
        Assert.Equal(5, proportionalDock.VisibleDockables!.Count);
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.Same(dock2, proportionalDock.VisibleDockables[1]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[2]);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[3]); // newDoc container
        Assert.Same(dock3, proportionalDock.VisibleDockables[4]);
    }

    [AvaloniaFact]
    public void SplitToDock_Cleans_Up_Orphaned_Splitters_After_Optimization()
    {
        var factory = new Factory();
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        proportionalDock.Factory = factory;
        
        // Create layout with orphaned splitter at the end
        var dock1 = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var orphanedSplitter = factory.CreateProportionalDockSplitter();
        
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, orphanedSplitter); // Orphaned at end
        
        var newDoc = new Document();

        // Split dock1 - should trigger cleanup of orphaned splitter
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        // Should have: dock1, splitter (new), newDoc container
        // The orphaned splitter at the end should be removed
        Assert.Equal(3, proportionalDock.VisibleDockables!.Count);
        Assert.Same(dock1, proportionalDock.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(proportionalDock.VisibleDockables[1]);
        Assert.IsType<ProportionalDock>(proportionalDock.VisibleDockables[2]);
    }

    [AvaloniaFact]
    public void SplitToDock_Invalid_Operation_Throws_NotSupportedException()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var newDoc = new Document();

        // Invalid operation should throw
        Assert.Throws<NotSupportedException>(() => 
            factory.SplitToDock(dock, newDoc, DockOperation.Fill));
        
        Assert.Throws<NotSupportedException>(() => 
            factory.SplitToDock(dock, newDoc, DockOperation.Window));
    }

    [AvaloniaFact]
    public void SplitToDock_Dock_Without_Owner_Does_Nothing()
    {
        var factory = new Factory();
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        var newDoc = new Document();
        
        var originalCount = dock.VisibleDockables!.Count;

        // Dock without owner should not cause changes
        factory.SplitToDock(dock, newDoc, DockOperation.Right);

        // Should remain unchanged
        Assert.Equal(originalCount, dock.VisibleDockables.Count);
    }

    [AvaloniaFact]
    public void SplitToDock_Dock_Not_Found_In_Owner_Does_Nothing()
    {
        var factory = new Factory();
        var owner = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var dock = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(), Owner = owner };
        var newDoc = new Document();
        
        // dock is set as child of owner but not added to VisibleDockables
        var originalCount = owner.VisibleDockables!.Count;

        factory.SplitToDock(dock, newDoc, DockOperation.Right);

        // Should remain unchanged
        Assert.Equal(originalCount, owner.VisibleDockables.Count);
    }

    [AvaloniaFact]
    public void SplitToDock_With_NaN_Proportion_Preserves_NaN_In_Optimization()
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

        // Split dock1 - should preserve NaN proportions
        factory.SplitToDock(dock1, newDoc, DockOperation.Right);

        Assert.True(double.IsNaN(dock1.Proportion));
        var newDocContainer = (ProportionalDock)proportionalDock.VisibleDockables![2];
        Assert.True(double.IsNaN(newDocContainer.Proportion));
    }

    [AvaloniaFact]
    public void CollapseDock_WithSingleDockable_CleansUpProportionalDockTree()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        // Create nested proportional docks: root -> outerDock -> innerDock -> toolDock
        var outerDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.5
        };

        var innerDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.8
        };

        var toolDock = new ToolDock
        {
            IsCollapsable = true,
            Alignment = Alignment.Left,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        factory.AddDockable(root, outerDock);
        factory.AddDockable(outerDock, innerDock);
        factory.AddDockable(innerDock, toolDock);

        // Collapse the tool dock - this should trigger tree cleanup
        factory.CollapseDock(toolDock);

        // After cleanup, the root should be empty because the nested structure collapsed
        Assert.Empty(root.VisibleDockables!);
    }

    [AvaloniaFact]
    public void CollapseDock_WithSingleDockable_StopsCleanupAtRootDock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        // Create: root -> proportionalDock -> documentDock
        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.7
        };

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        factory.AddDockable(root, proportionalDock);
        factory.AddDockable(proportionalDock, documentDock);

        // Add an empty tool dock that will be collapsed
        var emptyToolDock = new ToolDock
        {
            IsCollapsable = true,
            Alignment = Alignment.Right,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(proportionalDock, emptyToolDock);

        // Remove the empty tool dock, leaving only documentDock in proportionalDock
        factory.RemoveDockable(emptyToolDock, false);

        // Collapse the now-empty tool dock to trigger cleanup
        factory.CollapseDock(emptyToolDock);

        // The proportionalDock should remain under root (cleanup stops at root boundary)
        Assert.Single(root.VisibleDockables!);
        Assert.Same(proportionalDock, root.VisibleDockables[0]);
        Assert.Same(root, proportionalDock.Owner);
    }

    [AvaloniaFact]
    public void CollapseDock_PreservesProportionFromCollapsedDock()
    {
        var factory = new Factory();

        // Create a non-root dock as owner to allow cleanup
        var containerDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.6 // This should be preserved
        };

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = double.NaN
        };

        factory.AddDockable(containerDock, proportionalDock);
        factory.AddDockable(proportionalDock, documentDock);

        // Create and remove another dock to leave proportionalDock with single child
        var tempDock = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(proportionalDock, tempDock);
        factory.RemoveDockable(tempDock, false);

        // Trigger cleanup by collapsing an empty dock
        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = proportionalDock
        };
        factory.CollapseDock(emptyDock);

        // documentDock should inherit the proportion from proportionalDock
        Assert.Single(containerDock.VisibleDockables!);
        Assert.Same(documentDock, containerDock.VisibleDockables[0]);
        Assert.Equal(0.6, documentDock.Proportion, 3);
    }

    [AvaloniaFact]
    public void CollapseDock_CleansUpMultipleLevelsButStopsAtRootDock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        // Create deep nesting: root -> level1 -> level2 -> level3 -> document
        var level1 = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var level2 = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var level3 = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var document = new Document();

        factory.AddDockable(root, level1);
        factory.AddDockable(level1, level2);
        factory.AddDockable(level2, level3);
        factory.AddDockable(level3, document);

        // Collapse an empty dock to trigger recursive cleanup
        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = level3
        };
        factory.CollapseDock(emptyDock);

        // Cleanup should stop at root boundary - level1 should remain under root
        Assert.Single(root.VisibleDockables!);
        Assert.Same(level1, root.VisibleDockables[0]);
        Assert.Same(root, level1.Owner);
    }

    [AvaloniaFact]
    public void CollapseDock_DoesNotCleanUpWithMultipleDockables()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var dock1 = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        var dock2 = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };

        factory.AddDockable(root, proportionalDock);
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, dock2);

        // Collapse an empty dock - should not trigger cleanup since proportionalDock has 2 children
        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = proportionalDock
        };
        factory.CollapseDock(emptyDock);

        // Structure should remain unchanged
        Assert.Single(root.VisibleDockables!);
        Assert.Same(proportionalDock, root.VisibleDockables[0]);
        Assert.Equal(2, proportionalDock.VisibleDockables!.Count);
    }

    [AvaloniaFact]
    public void CollapseDock_DoesNotCleanUpNonProportionalDockOwner()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var document = new Document();

        factory.AddDockable(root, documentDock);
        factory.AddDockable(documentDock, document);

        // Collapse an empty dock - should not affect DocumentDock structure
        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = documentDock
        };
        factory.CollapseDock(emptyDock);

        // Structure should remain: root -> documentDock -> document
        Assert.Single(root.VisibleDockables!);
        Assert.Same(documentDock, root.VisibleDockables[0]);
        Assert.Single(documentDock.VisibleDockables!);
        Assert.Same(document, documentDock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void CollapseDock_CleansUpAfterRemovingDockableFromProportionalDock()
    {
        var factory = new Factory();

        // Use a non-root container to allow cleanup
        var containerDock = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var dock1 = new DocumentDock { VisibleDockables = factory.CreateList<IDockable>() };
        var dock2 = new ToolDock { VisibleDockables = factory.CreateList<IDockable>() };

        factory.AddDockable(containerDock, proportionalDock);
        factory.AddDockable(proportionalDock, dock1);
        factory.AddDockable(proportionalDock, dock2);

        // Remove one dock, leaving only one child in proportionalDock
        factory.RemoveDockable(dock2, false);

        // Now collapse an empty dock to trigger cleanup
        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = proportionalDock
        };
        factory.CollapseDock(emptyDock);

        // dock1 should now be directly under containerDock
        Assert.Single(containerDock.VisibleDockables!);
        Assert.Same(dock1, containerDock.VisibleDockables[0]);
        Assert.Same(containerDock, dock1.Owner);
    }

    [AvaloniaFact]
    public void CollapseDock_HandlesNullOwner()
    {
        var factory = new Factory();
        var orphanedDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = null // No owner
        };

        var document = new Document();
        factory.AddDockable(orphanedDock, document);

        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = orphanedDock
        };

        // Should not throw when owner is null
        factory.CollapseDock(emptyDock);

        // Structure should remain unchanged
        Assert.Single(orphanedDock.VisibleDockables!);
        Assert.Same(document, orphanedDock.VisibleDockables[0]);
    }

    [AvaloniaFact]
    public void CollapseDock_HandlesEmptyVisibleDockables()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var proportionalDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = null // Null collection
        };

        factory.AddDockable(root, proportionalDock);

        var emptyDock = new ToolDock 
        { 
            IsCollapsable = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            Owner = proportionalDock
        };

        // Should not throw when VisibleDockables is null
        factory.CollapseDock(emptyDock);

        // Structure should remain unchanged
        Assert.Single(root.VisibleDockables!);
        Assert.Same(proportionalDock, root.VisibleDockables[0]);
    }
}
