using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Controls;
using Xunit;
using Avalonia.Controls;
using System;
using System.Linq;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Headless tests for optimized split layout functionality with tool dock panels containing multiple tools.
/// Tests validate that proportions are correctly calculated and maintained after split operations.
/// </summary>
public class OptimizedSplitLayoutTests
{
    private Factory CreateFactory()
    {
        return new Factory();
    }

    private Tool CreateTool(string id, string title)
    {
        return new Tool
        {
            Id = id,
            Title = title
        };
    }

    private ToolDock CreateToolDockWithTwoTools(Factory factory, string id, string title, string tool1Id, string tool2Id, double proportion = double.NaN)
    {
        var toolDock = new ToolDock
        {
            Id = id,
            Title = title,
            Proportion = proportion,
            VisibleDockables = factory.CreateList<IDockable>(),
            Alignment = Alignment.Left,
            Factory = factory
        };

        var tool1 = CreateTool(tool1Id, $"Tool {tool1Id}");
        var tool2 = CreateTool(tool2Id, $"Tool {tool2Id}");

        tool1.Owner = toolDock;
        tool2.Owner = toolDock;

        factory.AddDockable(toolDock, tool1);
        factory.AddDockable(toolDock, tool2);

        toolDock.ActiveDockable = tool1;

        return toolDock;
    }

    private ProportionalDock CreateTestLayout(Factory factory, out ToolDock toolDock1, out ToolDock toolDock2)
    {
        var layout = new ProportionalDock
        {
            Id = "MainLayout",
            Title = "Main Layout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        toolDock1 = CreateToolDockWithTwoTools(factory, "ToolDock1", "Tool Dock 1", "Tool1A", "Tool1B", 0.5);
        toolDock2 = CreateToolDockWithTwoTools(factory, "ToolDock2", "Tool Dock 2", "Tool2A", "Tool2B", 0.5);

        factory.AddDockable(layout, toolDock1);
        var splitter = factory.CreateProportionalDockSplitter();
        factory.AddDockable(layout, splitter);
        factory.AddDockable(layout, toolDock2);

        return layout;
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_HorizontalSplit_ValidatesProportions()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = CreateTestLayout(factory, out var toolDock1, out var toolDock2);
        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act - Split the first tool dock to the right
        factory.SplitToDock(toolDock1, newToolDock, DockOperation.Right);

        // Assert
        // Verify the layout structure is correct
        Assert.Single(rootDock.VisibleDockables!);
        var mainLayout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Horizontal, mainLayout.Orientation);

        // When splitting with the same orientation, the layout reuses the existing container
        // Should have: [toolDock1], [splitter], [newToolDock], [splitter], [toolDock2]
        Assert.Equal(5, mainLayout.VisibleDockables!.Count);
        
        // Verify the order: toolDock1, splitter, newToolDock, splitter, toolDock2
        Assert.Same(toolDock1, mainLayout.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[1]);
        Assert.Same(newToolDock, mainLayout.VisibleDockables[2]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[3]);
        Assert.Same(toolDock2, mainLayout.VisibleDockables[4]);

        // Verify proportions are correctly split (original 0.5 should be split into 0.25 each)
        Assert.Equal(0.25, toolDock1.Proportion, 3);
        Assert.Equal(0.25, newToolDock.Proportion, 3);

        // Verify the second tool dock maintains its proportion
        Assert.Equal(0.5, toolDock2.Proportion, 3);

        // Verify each tool dock still contains its two tools
        Assert.Equal(2, toolDock1.VisibleDockables!.Count);
        Assert.Equal("Tool1A", toolDock1.VisibleDockables[0].Id);
        Assert.Equal("Tool1B", toolDock1.VisibleDockables[1].Id);

        Assert.Equal(2, toolDock2.VisibleDockables!.Count);
        Assert.Equal("Tool2A", toolDock2.VisibleDockables[0].Id);
        Assert.Equal("Tool2B", toolDock2.VisibleDockables[1].Id);

        Assert.Equal(2, newToolDock.VisibleDockables!.Count);
        Assert.Equal("Tool3A", newToolDock.VisibleDockables[0].Id);
        Assert.Equal("Tool3B", newToolDock.VisibleDockables[1].Id);
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_VerticalSplit_ValidatesProportions()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = new ProportionalDock
        {
            Id = "MainLayout",
            Title = "Main Layout",
            Orientation = Orientation.Vertical, // Vertical orientation
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var toolDock1 = CreateToolDockWithTwoTools(factory, "ToolDock1", "Tool Dock 1", "Tool1A", "Tool1B", 0.6);
        var toolDock2 = CreateToolDockWithTwoTools(factory, "ToolDock2", "Tool Dock 2", "Tool2A", "Tool2B", 0.4);

        factory.AddDockable(layout, toolDock1);
        var splitter = factory.CreateProportionalDockSplitter();
        factory.AddDockable(layout, splitter);
        factory.AddDockable(layout, toolDock2);

        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act - Split the first tool dock to the bottom
        factory.SplitToDock(toolDock1, newToolDock, DockOperation.Bottom);

        // Assert
        Assert.Single(rootDock.VisibleDockables!);
        var mainLayout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Vertical, mainLayout.Orientation);

        // When splitting with the same orientation, the layout reuses the existing container
        // Should have: [toolDock1], [splitter], [newToolDock], [splitter], [toolDock2]
        Assert.Equal(5, mainLayout.VisibleDockables!.Count);
        
        // Verify the order: toolDock1, splitter, newToolDock, splitter, toolDock2
        Assert.Same(toolDock1, mainLayout.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[1]);
        Assert.Same(newToolDock, mainLayout.VisibleDockables[2]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[3]);
        Assert.Same(toolDock2, mainLayout.VisibleDockables[4]);

        // Verify proportions are correctly split (original 0.6 should be split into 0.3 each)
        Assert.Equal(0.3, toolDock1.Proportion, 3);
        Assert.Equal(0.3, newToolDock.Proportion, 3);

        // Verify the second tool dock maintains its proportion
        Assert.Equal(0.4, toolDock2.Proportion, 3);
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_LeftSplit_ValidatesProportions()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = CreateTestLayout(factory, out var toolDock1, out var toolDock2);
        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act - Split the second tool dock to the left
        factory.SplitToDock(toolDock2, newToolDock, DockOperation.Left);

        // Assert
        Assert.Single(rootDock.VisibleDockables!);
        var mainLayout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Horizontal, mainLayout.Orientation);

        // When splitting with the same orientation, the layout reuses the existing container
        // Should have: [toolDock1], [splitter], [newToolDock], [splitter], [toolDock2]
        Assert.Equal(5, mainLayout.VisibleDockables!.Count);
        
        // Verify the order: toolDock1, splitter, newToolDock, splitter, toolDock2
        Assert.Same(toolDock1, mainLayout.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[1]);
        Assert.Same(newToolDock, mainLayout.VisibleDockables[2]);
        Assert.IsType<ProportionalDockSplitter>(mainLayout.VisibleDockables[3]);
        Assert.Same(toolDock2, mainLayout.VisibleDockables[4]);

        // Verify proportions are correctly split (original 0.5 should be split into 0.25 each)
        Assert.Equal(0.25, newToolDock.Proportion, 3);
        Assert.Equal(0.25, toolDock2.Proportion, 3);

        // Verify the first tool dock maintains its proportion
        Assert.Equal(0.5, toolDock1.Proportion, 3);
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_TopSplit_ValidatesProportions()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = CreateTestLayout(factory, out var toolDock1, out var toolDock2);
        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act - Split the second tool dock to the top
        factory.SplitToDock(toolDock2, newToolDock, DockOperation.Top);

        // Assert
        Assert.Single(rootDock.VisibleDockables!);
        var mainLayout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);
        Assert.Equal(Orientation.Horizontal, mainLayout.Orientation);

        // When splitting with different orientation, it creates a nested layout
        // Should have: [toolDock1], [splitter], [nested vertical layout]
        Assert.Equal(3, mainLayout.VisibleDockables!.Count);
        
        // Last item should be a nested proportional dock with vertical orientation
        var nestedLayout = Assert.IsType<ProportionalDock>(mainLayout.VisibleDockables[2]);
        Assert.Equal(Orientation.Vertical, nestedLayout.Orientation);
        Assert.Equal(3, nestedLayout.VisibleDockables!.Count); // newToolDock, splitter, toolDock2

        // Verify the new dock is on the top of the split
        Assert.Same(newToolDock, nestedLayout.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(nestedLayout.VisibleDockables[1]);
        Assert.Same(toolDock2, nestedLayout.VisibleDockables[2]);

        // Verify proportions are correctly split 
        // The toolDock2 proportion becomes NaN when moved to nested layout, and newToolDock also gets NaN
        Assert.True(double.IsNaN(newToolDock.Proportion));
        Assert.True(double.IsNaN(toolDock2.Proportion));

        // Verify the first tool dock maintains its proportion
        Assert.Equal(0.5, toolDock1.Proportion, 3);
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_WithNaNProportions_MaintainsNaN()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = new ProportionalDock
        {
            Id = "MainLayout",
            Title = "Main Layout",
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        // Create tool docks with NaN proportions
        var toolDock1 = CreateToolDockWithTwoTools(factory, "ToolDock1", "Tool Dock 1", "Tool1A", "Tool1B", double.NaN);
        var toolDock2 = CreateToolDockWithTwoTools(factory, "ToolDock2", "Tool Dock 2", "Tool2A", "Tool2B", double.NaN);

        factory.AddDockable(layout, toolDock1);
        var splitter = factory.CreateProportionalDockSplitter();
        factory.AddDockable(layout, splitter);
        factory.AddDockable(layout, toolDock2);

        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act
        factory.SplitToDock(toolDock1, newToolDock, DockOperation.Right);

        // Assert - All proportions should remain NaN
        Assert.True(double.IsNaN(toolDock1.Proportion));
        Assert.True(double.IsNaN(newToolDock.Proportion));
        Assert.True(double.IsNaN(toolDock2.Proportion));
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_ComplexNesting_ValidatesDeepProportions()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = CreateTestLayout(factory, out var toolDock1, out var toolDock2);
        factory.AddDockable(rootDock, layout);

        var newToolDock1 = CreateToolDockWithTwoTools(factory, "NewToolDock1", "New Tool Dock 1", "Tool3A", "Tool3B");
        var newToolDock2 = CreateToolDockWithTwoTools(factory, "NewToolDock2", "New Tool Dock 2", "Tool4A", "Tool4B");

        // Act - Perform multiple splits
        factory.SplitToDock(toolDock1, newToolDock1, DockOperation.Right);
        factory.SplitToDock(toolDock2, newToolDock2, DockOperation.Bottom);

        // Assert
        Assert.Single(rootDock.VisibleDockables!);
        var mainLayout = Assert.IsType<ProportionalDock>(rootDock.VisibleDockables[0]);

        // Verify that all tool docks still contain their two tools
        var allToolDocks = new[] { toolDock1, toolDock2, newToolDock1, newToolDock2 };
        
        foreach (var toolDock in allToolDocks)
        {
            Assert.Equal(2, toolDock.VisibleDockables!.Count);
            Assert.All(toolDock.VisibleDockables, dockable => Assert.IsType<Tool>(dockable));
        }

        // Verify that proportions sum correctly in nested layouts
        // Note: The exact proportions will depend on the split implementation, but they should be valid numbers
        foreach (var toolDock in allToolDocks)
        {
            Assert.True(double.IsNaN(toolDock.Proportion) || (toolDock.Proportion > 0 && toolDock.Proportion <= 1));
        }
    }

    [AvaloniaFact]
    public void OptimizedSplitLayout_ValidatesToolAccessibility()
    {
        // Arrange
        var factory = CreateFactory();
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var layout = CreateTestLayout(factory, out var toolDock1, out var toolDock2);
        factory.AddDockable(rootDock, layout);

        var newToolDock = CreateToolDockWithTwoTools(factory, "NewToolDock", "New Tool Dock", "Tool3A", "Tool3B");

        // Act
        factory.SplitToDock(toolDock1, newToolDock, DockOperation.Right);

        // Assert - Verify all tools are still accessible and properly owned
        var tool1A = toolDock1.VisibleDockables![0];
        var tool1B = toolDock1.VisibleDockables![1];
        var tool2A = toolDock2.VisibleDockables![0];
        var tool2B = toolDock2.VisibleDockables![1];
        var tool3A = newToolDock.VisibleDockables![0];
        var tool3B = newToolDock.VisibleDockables![1];

        // Verify ownership
        Assert.Same(toolDock1, tool1A.Owner);
        Assert.Same(toolDock1, tool1B.Owner);
        Assert.Same(toolDock2, tool2A.Owner);
        Assert.Same(toolDock2, tool2B.Owner);
        Assert.Same(newToolDock, tool3A.Owner);
        Assert.Same(newToolDock, tool3B.Owner);

        // Verify active dockable is set
        Assert.NotNull(toolDock1.ActiveDockable);
        Assert.NotNull(toolDock2.ActiveDockable);
        Assert.NotNull(newToolDock.ActiveDockable);
    }
}