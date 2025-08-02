using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
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
            VisibleDockables = visibleDockables ?? new Avalonia.Collections.AvaloniaList<IDockable>()
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
        toolDockA.VisibleDockables.Add(toolA1);
        toolDockA.VisibleDockables.Add(toolA2);
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { toolDockA };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that a tool from GroupB cannot be dropped into GroupA dock
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            var canDrop = dockManager.ValidateTool(toolB1, toolDockA, DragAction.Copy, DockOperation.Fill, false);
            Assert.False(canDrop);
        }
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
        toolDockA.VisibleDockables.Add(toolA1);
        toolDockA.VisibleDockables.Add(toolA2);
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { toolDockA };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that a tool from the same group can be dropped
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            var canDrop = dockManager.ValidateTool(toolA3, toolDockA, DragAction.Copy, DockOperation.Fill, false);
            Assert.True(canDrop);
        }
    }

    [AvaloniaFact]
    public void DockControl_Layout_Should_Allow_NullGroup_Docking()
    {
        var factory = CreateFactory();
        
        // Create tools with null groups
        var tool1 = CreateTool("Tool1", "Tool 1", null);
        var tool2 = CreateTool("Tool2", "Tool 2", null);
        var tool3 = CreateTool("Tool3", "Tool 3", "GroupA");
        
        // Create tool dock with null group
        var toolDock = CreateToolDock("ToolDock", "Tools", null);
        toolDock.VisibleDockables.Add(tool1);
        toolDock.VisibleDockables.Add(tool2);
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { toolDock };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that tools with any group can be dropped into null group dock
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            var canDropNull = dockManager.ValidateTool(tool1, toolDock, DragAction.Copy, DockOperation.Fill, false);
            var canDropGroupA = dockManager.ValidateTool(tool3, toolDock, DragAction.Copy, DockOperation.Fill, false);
            
            Assert.True(canDropNull);
            Assert.True(canDropGroupA);
        }
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
        documentDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { docA1, docA2 };
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { documentDock };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        // Test that a document from GroupB cannot be dropped into GroupA dock
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            var canDrop = dockManager.ValidateDocument(docB1, documentDock, DragAction.Copy, DockOperation.Fill, false);
            Assert.False(canDrop);
        }
    }

    [AvaloniaFact]
    public void DockControl_Mixed_DockGroups_Should_Work_Independently()
    {
        var factory = CreateFactory();
        
        // Create mixed content with different groups
        var toolA = CreateTool("ToolA", "Tool A", "ToolGroup");
        var toolB = CreateTool("ToolB", "Tool B", "ToolGroup");
        var docA = CreateDocument("DocA", "Document A", "DocumentGroup");
        var docB = CreateDocument("DocB", "Document B", "DocumentGroup");
        
        // Create separate docks for different groups
        var toolDock = CreateToolDock("ToolDock", "Tools", "ToolGroup");
        toolDock.VisibleDockables.Add(toolA);
        
        var documentDock = new DocumentDock
        {
            Id = "DocumentDock",
            Title = "Documents",
            DockGroup = "DocumentGroup"
        };
        documentDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { docA };
        
        // Create proportional dock containing both
        var proportionalDock = new ProportionalDock
        {
            Id = "MainLayout",
            Title = "Main Layout",
            Orientation = Orientation.Horizontal
        };
        proportionalDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { toolDock, documentDock };
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { proportionalDock };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            // Test tool group restrictions
            var canDropToolSame = dockManager.ValidateTool(toolB, toolDock, DragAction.Copy, DockOperation.Fill, false);
            var canDropToolDifferent = dockManager.ValidateDocument(docB, toolDock, DragAction.Copy, DockOperation.Fill, false);
            
            // Test document group restrictions
            var canDropDocSame = dockManager.ValidateDocument(docB, documentDock, DragAction.Copy, DockOperation.Fill, false);
            var canDropDocDifferent = dockManager.ValidateTool(toolB, documentDock, DragAction.Copy, DockOperation.Fill, false);
            
            Assert.True(canDropToolSame);
            Assert.False(canDropToolDifferent);
            Assert.True(canDropDocSame);
            Assert.False(canDropDocDifferent);
        }
    }

    [AvaloniaFact]
    public void DockControl_Hierarchy_Should_Inherit_DockGroup()
    {
        var factory = CreateFactory();
        
        // Create tools without explicit groups
        var tool1 = CreateTool("Tool1", "Tool 1", null);
        var tool2 = CreateTool("Tool2", "Tool 2", null);
        var tool3 = CreateTool("Tool3", "Tool 3", null);
        
        // Create parent dock with group that should be inherited
        var parentDock = CreateToolDock("ParentDock", "Parent Tools", "InheritedGroup");
        parentDock.VisibleDockables.Add(tool1);
        
        // Set up ownership hierarchy
        tool1.Owner = parentDock;
        tool2.Owner = parentDock;
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { parentDock };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            // Tool2 should inherit the group from parentDock and be allowed
            var canDropInherited = dockManager.ValidateTool(tool2, parentDock, DragAction.Copy, DockOperation.Fill, false);
            
            // Tool3 with no ownership should still be allowed (no group restriction)
            var canDropUnrelated = dockManager.ValidateTool(tool3, parentDock, DragAction.Copy, DockOperation.Fill, false);
            
            Assert.True(canDropInherited);
            Assert.True(canDropUnrelated);
        }
    }

    [AvaloniaFact]
    public void DockControl_EmptyDock_Should_Accept_Any_Group()
    {
        var factory = CreateFactory();
        
        // Create tools with different groups
        var toolA = CreateTool("ToolA", "Tool A", "GroupA");
        var toolB = CreateTool("ToolB", "Tool B", "GroupB");
        var toolNull = CreateTool("ToolNull", "Tool Null", null);
        
        // Create empty dock with specific group
        var emptyDock = CreateToolDock("EmptyDock", "Empty Dock", "GroupA");
        
        var rootDock = factory.CreateRootDock();
        rootDock.VisibleDockables = new Avalonia.Collections.AvaloniaList<IDockable> { emptyDock };
        
        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = rootDock
        };
        
        var dockManager = factory.DockManager;
        if (dockManager != null)
        {
            // Empty dock should accept any tool initially
            var canDropSame = dockManager.ValidateTool(toolA, emptyDock, DragAction.Copy, DockOperation.Fill, false);
            var canDropDifferent = dockManager.ValidateTool(toolB, emptyDock, DragAction.Copy, DockOperation.Fill, false);
            var canDropNull = dockManager.ValidateTool(toolNull, emptyDock, DragAction.Copy, DockOperation.Fill, false);
            
            Assert.True(canDropSame);
            Assert.True(canDropDifferent);
            Assert.True(canDropNull);
        }
    }
}
