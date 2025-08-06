using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Headless tests for docking group functionality in the UI.
/// </summary>
public class DockGroupHeadlessTests
{
    private Factory CreateFactory()
    {
        return new Factory();
    }

    private Tool CreateTool(string id, string title, string? dockGroup = null)
    {
        return new Tool
        {
            Id = id,
            Title = title,
            DockGroup = dockGroup
        };
    }

    private Document CreateDocument(string id, string title, string? dockGroup = null)
    {
        return new Document
        {
            Id = id,
            Title = title,
            DockGroup = dockGroup
        };
    }

    private ToolDock CreateToolDock(string id, string title, string? dockGroup = null, IList<IDockable>? visibleDockables = null)
    {
        return new ToolDock
        {
            Id = id,
            Title = title,
            DockGroup = dockGroup,
            VisibleDockables = visibleDockables ?? new global::Avalonia.Collections.AvaloniaList<IDockable>()
        };
    }

    [AvaloniaFact]
    public void DockControl_Layout_Should_Respect_DockGroup_Restrictions()
    {
        var factory = CreateFactory();
        
        // Create tools with different groups
        var toolA1 = CreateTool("ToolA1", "Tool A1", "GroupA");
        var toolA2 = CreateTool("ToolA2", "Tool A2", "GroupA");
        var toolB1 = CreateTool("ToolB1", "Tool B1", "GroupB");
        
        // Create tool dock with GroupA
        var toolDockA = CreateToolDock("ToolDockA", "Tools A", "GroupA");
        toolDockA.VisibleDockables?.Add(toolA1);
        toolDockA.VisibleDockables?.Add(toolA2);
        
        // Set proper ownership hierarchy
        toolA1.Owner = toolDockA;
        toolA2.Owner = toolDockA;
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable> { toolDockA };
        
        var dockControl = new DockControl
        {
            InitializeLayout = true,
            InitializeFactory = true,
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that a tool from GroupB cannot be dropped into GroupA dock
        // Use the properly initialized DockManager from the DockControl
        var canDrop = dockControl.DockManager.ValidateTool(toolB1, toolDockA, DragAction.Copy, DockOperation.Fill, false);
        Assert.False(canDrop);
    }

    [AvaloniaFact]
    public void DockControl_Layout_Should_Allow_SameGroup_Docking()
    {
        var factory = CreateFactory();
        
        // Create tools with same group
        var toolA1 = CreateTool("ToolA1", "Tool A1", "GroupA");
        var toolA2 = CreateTool("ToolA2", "Tool A2", "GroupA");
        var toolA3 = CreateTool("ToolA3", "Tool A3", "GroupA");
        
        // Create tool dock with GroupA
        var toolDockA = CreateToolDock("ToolDockA", "Tools A", "GroupA");
        toolDockA.VisibleDockables?.Add(toolA1);
        toolDockA.VisibleDockables?.Add(toolA2);
        
        // Set proper ownership hierarchy
        toolA1.Owner = toolDockA;
        toolA2.Owner = toolDockA;
        
        // Create a source dock for toolA3 that we're trying to drag
        var sourceDockForA3 = CreateToolDock("SourceDockA3", "Source for A3", "GroupA");
        sourceDockForA3.VisibleDockables?.Add(toolA3);
        toolA3.Owner = sourceDockForA3;
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable> { toolDockA };
        
        var dockControl = new DockControl
        {
            InitializeLayout = true,
            InitializeFactory = true,
            Factory = factory,
            Layout = rootDock
        };
        
        // Disable size conflict prevention for test scenarios
        dockControl.DockManager.PreventSizeConflicts = false;
        
        // Test that a tool from the same group can be dropped
        // Use the properly initialized DockManager from the DockControl
        var canDrop = dockControl.DockManager.ValidateTool(toolA3, toolDockA, DragAction.Move, DockOperation.Fill, false);
        Assert.True(canDrop);
    }

    [AvaloniaFact]
    public void DockControl_Documents_Should_Respect_DockGroup_Restrictions()
    {
        var factory = CreateFactory();
        
        // Create documents with different groups
        var docA1 = CreateDocument("DocA1", "Document A1", "GroupA");
        var docA2 = CreateDocument("DocA2", "Document A2", "GroupA");
        var docB1 = CreateDocument("DocB1", "Document B1", "GroupB");
        
        // Create document dock with GroupA
        var documentDock = new DocumentDock
        {
            Id = "DocumentDockA",
            Title = "Documents A",
            DockGroup = "GroupA"
        };
        documentDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable> { docA1, docA2 };
        
        // Set proper ownership hierarchy
        docA1.Owner = documentDock;
        docA2.Owner = documentDock;
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<IDockable> { documentDock };
        
        var dockControl = new DockControl
        {
            InitializeLayout = true,
            InitializeFactory = true,
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that a document from GroupB cannot be dropped into GroupA dock
        // Use the properly initialized DockManager from the DockControl
        var canDrop = dockControl.DockManager.ValidateDocument(docB1, documentDock, DragAction.Move, DockOperation.Fill, false);
        Assert.False(canDrop);
    }
}
