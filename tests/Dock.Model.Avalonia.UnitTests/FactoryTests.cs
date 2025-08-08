using Avalonia.Headless.XUnit;
using Avalonia.Collections;
using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests;

public class FactoryTests
{
    [AvaloniaFact]
    public void TestFactory_Ctor()
    {
        var actual = new TestFactory();
        Assert.NotNull(actual);
    }

    [AvaloniaFact]
    public void CreateList_Creates_AvaloniaList_Empty()
    {
        var factory = new TestFactory();
        var actual = factory.CreateList<IDockable>();
        Assert.NotNull(actual);
        Assert.IsType<AvaloniaList<IDockable>>(actual);
        Assert.Empty(actual);
    }

    [AvaloniaFact]
    public void CreateRootDock_Creates_RootDock_Type()
    {
        var factory = new TestFactory();
        var actual = factory.CreateRootDock();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDock_Creates_ProportionalDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDock();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockDock_Creates_DockDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockDock();
        Assert.NotNull(actual);
        Assert.IsType<DockDock>(actual);
        Assert.True(actual.LastChildFill);
    }

    [AvaloniaFact]
    public void CreateStackDock_Creates_StackDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateStackDock();
        Assert.NotNull(actual);
        Assert.IsType<StackDock>(actual);
    }

    [AvaloniaFact]
    public void CreateGridDock_Creates_GridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDock();
        Assert.NotNull(actual);
        Assert.IsType<GridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateWrapDock_Creates_WrapDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateWrapDock();
        Assert.NotNull(actual);
        Assert.IsType<WrapDock>(actual);
    }

    [AvaloniaFact]
    public void CreateUniformGridDock_Creates_UniformGridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateUniformGridDock();
        Assert.NotNull(actual);
        Assert.IsType<UniformGridDock>(actual);
    }

    [AvaloniaFact]
    public void CreateProportionalDockSplitter_Creates_ProportionalDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDockSplitter>(actual);
        Assert.True(actual.CanResize);
        Assert.False(actual.ResizePreview);
    }

    [AvaloniaFact]
    public void CreateGridDockSplitter_Creates_GridDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<GridDockSplitter>(actual);
    }

    [AvaloniaFact]
    public void CreateToolDock_Creates_ToolDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateToolDock();
        Assert.NotNull(actual);
        Assert.IsType<ToolDock>(actual);
    }

    [AvaloniaFact]
    public void Tool_Default_Sizes_Are_NaN()
    {
        var tool = new Tool();
        Assert.True(double.IsNaN(tool.MinWidth));
        Assert.True(double.IsNaN(tool.MaxWidth));
        Assert.True(double.IsNaN(tool.MinHeight));
        Assert.True(double.IsNaN(tool.MaxHeight));
    }

    [AvaloniaFact]
    public void CreateDocumentDock_Creates_DocumentDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocumentDock();
        Assert.NotNull(actual);
        Assert.IsType<DocumentDock>(actual);
    }

    [AvaloniaFact]
    public void CreateDockWindow_Creates_DockWindow()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockWindow();
        Assert.NotNull(actual);
        Assert.IsType<DockWindow>(actual);
    }

    [AvaloniaFact]
    public void CreateLayout_Creates_RootDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateLayout();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [AvaloniaFact]
    public void OnWindowActivated_Raises_Event()
    {
        var factory = new TestFactory();
        var window = factory.CreateDockWindow();
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.OnWindowActivated(window);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }

    [AvaloniaFact]
    public void OnDockableActivated_Raises_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.OnDockableActivated(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void SetActiveDockable_Triggers_DockableActivated_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var dock = factory.CreateDocumentDock();
        
        // Set up the dock hierarchy
        dock.VisibleDockables = factory.CreateList<IDockable>(dockable);
        dockable.Owner = dock; // Set the owner relationship
        
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.SetActiveDockable(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void OnWindowDeactivated_Raises_Event()
    {
        var factory = new TestFactory();
        var window = factory.CreateDockWindow();
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowDeactivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.OnWindowDeactivated(window);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }

    [AvaloniaFact]
    public void OnDockableDeactivated_Raises_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var eventRaised = false;
        var raisedDockable = (IDockable?)null;

        factory.DockableDeactivated += (sender, args) =>
        {
            eventRaised = true;
            raisedDockable = args.Dockable;
        };

        factory.OnDockableDeactivated(dockable);

        Assert.True(eventRaised);
        Assert.Same(dockable, raisedDockable);
    }

    [AvaloniaFact]
    public void ActivateWindow_Triggers_WindowActivated_Event()
    {
        var factory = new TestFactory();
        var dockable = factory.CreateToolDock();
        var root = factory.CreateRootDock();
        var window = factory.CreateDockWindow();
        
        // Set up the window hierarchy
        root.VisibleDockables = factory.CreateList<IDockable>(dockable);
        root.ActiveDockable = dockable;
        dockable.Owner = root; // Set the owner relationship
        window.Layout = root;
        root.Window = window;
        
        var eventRaised = false;
        var raisedWindow = (IDockWindow?)null;

        factory.WindowActivated += (sender, args) =>
        {
            eventRaised = true;
            raisedWindow = args.Window;
        };

        factory.ActivateWindow(dockable);

        Assert.True(eventRaised);
        Assert.Same(window, raisedWindow);
    }

    // Integration tests for property copying functionality
    
    [AvaloniaFact]
    public void CopyPropertiesForSplitDock_RootSplit_SetsProportion_NaN_To_Original()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDock = new ProportionalDock { Proportion = 0.6 };
        var targetDock = new ProportionalDock { Proportion = 0.2 };

        // Act - Root split (isNestedLayout = false)
        factory.CopyPropertiesForSplitDock(sourceDock, targetDock, DockOperation.Right, isNestedLayout: false);

        // Assert
        Assert.True(double.IsNaN(sourceDock.Proportion), "Source dock proportion should become NaN for root split");
        Assert.Equal(0.6, targetDock.Proportion, 3); // Target dock should get original source proportion
    }

    [AvaloniaFact]
    public void CopyPropertiesForSplitDock_NestedSplit_HalvesProportions()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDock = new ProportionalDock { Proportion = 0.8 };
        var targetDock = new ProportionalDock { Proportion = 0.1 };

        // Act - Nested split (isNestedLayout = true)
        factory.CopyPropertiesForSplitDock(sourceDock, targetDock, DockOperation.Left, isNestedLayout: true);

        // Assert
        Assert.Equal(0.4, sourceDock.Proportion, 3); // Source dock should get half proportion for nested split
        Assert.Equal(0.4, targetDock.Proportion, 3); // Target dock should get half proportion for nested split
    }

    [AvaloniaFact]
    public void CopyPropertiesForSplitDock_NaNProportion_PreservesNaN()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDock = new ProportionalDock { Proportion = double.NaN };
        var targetDock = new ProportionalDock { Proportion = 0.5 };

        // Act
        factory.CopyPropertiesForSplitDock(sourceDock, targetDock, DockOperation.Top, isNestedLayout: true);

        // Assert
        Assert.True(double.IsNaN(sourceDock.Proportion), "NaN proportion should remain NaN");
        Assert.True(double.IsNaN(targetDock.Proportion), "NaN proportion should result in NaN for target");
    }

    [AvaloniaFact]
    public void CopyDockProperties_DocumentDocks_CopiesAllProperties()
    {
        // Arrange
        var factory = new TestFactory();
        var template = new DocumentTemplate { Content = "TestTemplate" };
        var sourceDock = new DocumentDock
        {
            Id = "SourceId",
            CanCreateDocument = true,
            EnableWindowDrag = true,
            DocumentTemplate = template
        };
        var targetDock = new DocumentDock
        {
            Id = "TargetId",
            CanCreateDocument = false,
            EnableWindowDrag = false,
            DocumentTemplate = null
        };

        // Act
        factory.CopyDockProperties(sourceDock, targetDock, DockOperation.Window);

        // Assert
        Assert.Equal("SourceId", targetDock.Id);
        Assert.True(targetDock.CanCreateDocument);
        Assert.True(targetDock.EnableWindowDrag);
        Assert.Same(template, targetDock.DocumentTemplate);
    }

    [AvaloniaFact]
    public void CopyDockProperties_ToolDocks_CopiesProperties()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDock = new ToolDock { Id = "SourceId", Alignment = Alignment.Left };
        var targetDock = new ToolDock { Id = "TargetId", Alignment = Alignment.Right };

        // Act
        factory.CopyDockProperties(sourceDock, targetDock, DockOperation.Window);

        // Assert - Should copy tool dock properties
        Assert.Equal("SourceId", targetDock.Id);
        Assert.Equal(Alignment.Left, targetDock.Alignment);
    }

    [AvaloniaFact]
    public void CopyPropertiesForFloatingWindow_WithDocumentDockOwner_CopiesProperties()
    {
        // Arrange
        var factory = new TestFactory();
        var template = new DocumentTemplate { Content = "OwnerTemplate" };
        var sourceOwner = new DocumentDock
        {
            Id = "OwnerSourceId",
            CanCreateDocument = true,
            EnableWindowDrag = true,
            DocumentTemplate = template
        };
        var sourceDockable = new Document { Owner = sourceOwner };
        var window = new DockWindow();
        var targetDock = new DocumentDock
        {
            Id = "TargetId",
            CanCreateDocument = false,
            EnableWindowDrag = false,
            DocumentTemplate = null
        };

        // Act
        factory.CopyPropertiesForFloatingWindow(sourceDockable, window, targetDock);

        // Assert
        Assert.Equal("OwnerSourceId", targetDock.Id);
        Assert.True(targetDock.CanCreateDocument);
        Assert.True(targetDock.EnableWindowDrag);
        Assert.Same(template, targetDock.DocumentTemplate);
    }

    [AvaloniaFact]
    public void CopyDimensionProperties_Window_SetsAllDimensions()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDockable = new Document();
        var window = new DockWindow
        {
            X = 0,
            Y = 0,
            Width = 100,
            Height = 100
        };

        // Act
        factory.CopyDimensionProperties(sourceDockable, window, 200.0, 300.0, 800.0, 600.0);

        // Assert
        Assert.Equal(200.0, window.X);
        Assert.Equal(300.0, window.Y);
        Assert.Equal(800.0, window.Width);
        Assert.Equal(600.0, window.Height);
    }

    [AvaloniaFact]
    public void CopyDimensionProperties_NonWindow_DoesNotThrow()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDockable = new Document();
        var nonWindow = new Document();

        // Act & Assert - Should not throw exception
        factory.CopyDimensionProperties(sourceDockable, nonWindow, 100.0, 200.0, 800.0, 600.0);
        // No assertion needed - the test passes if no exception is thrown
    }

    [AvaloniaFact]
    public void CopyDockableProperties_CallsSuccessfully()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDockable = new Document { Id = "Source" };
        var targetDockable = new Document { Id = "Target" };

        // Act & Assert - Should not throw exception (default implementation is empty)
        factory.CopyDockableProperties(sourceDockable, targetDockable, DockOperation.Fill);
        // No assertion needed - the test passes if no exception is thrown
    }

    [AvaloniaFact]
    public void CopyDockProperties_ToolDock_CopiesAllProperties()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceToolDock = new ToolDock
        {
            Id = "SourceToolId",
            Alignment = Alignment.Left,
            GripMode = GripMode.Visible,
            IsExpanded = true,
            AutoHide = false
        };
        var targetToolDock = new ToolDock
        {
            Id = "TargetToolId",
            Alignment = Alignment.Right,
            GripMode = GripMode.Hidden,
            IsExpanded = false,
            AutoHide = true
        };

        // Act
        factory.CopyDockProperties(sourceToolDock, targetToolDock, DockOperation.Window);

        // Assert
        Assert.Equal("SourceToolId", targetToolDock.Id);
        Assert.Equal(Alignment.Left, targetToolDock.Alignment);
        Assert.Equal(GripMode.Visible, targetToolDock.GripMode);
        Assert.True(targetToolDock.IsExpanded);
        Assert.False(targetToolDock.AutoHide);
    }

    [AvaloniaFact]
    public void CopyDockProperties_NonToolDock_DoesNotCopyToolProperties()
    {
        // Arrange
        var factory = new TestFactory();
        var sourceDocumentDock = new DocumentDock { Id = "SourceDoc" };
        var targetToolDock = new ToolDock
        {
            Id = "TargetTool",
            Alignment = Alignment.Right,
            GripMode = GripMode.Hidden
        };

        // Act
        factory.CopyDockProperties(sourceDocumentDock, targetToolDock, DockOperation.Window);

        // Assert - Tool dock properties should remain unchanged
        Assert.Equal("TargetTool", targetToolDock.Id);
        Assert.Equal(Alignment.Right, targetToolDock.Alignment);
        Assert.Equal(GripMode.Hidden, targetToolDock.GripMode);
    }
}

public class TestFactory : Factory
{
}
