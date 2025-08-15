// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Controls;
using Xunit;

namespace Dock.Model.CaliburMicro.UnitTests.Controls;

public class ToolDockTests
{
    [Fact]
    public void ToolDock_Constructor_Sets_Default_Values()
    {
        var toolDock = new ToolDock();

        Assert.Equal(Alignment.Unset, toolDock.Alignment);
        Assert.True(toolDock.IsExpanded);
        Assert.True(toolDock.AutoHide);
        Assert.Equal(GripMode.Visible, toolDock.GripMode);
    }

    [Fact]
    public void ToolDock_Alignment_Property_Can_Be_Set()
    {
        var toolDock = new ToolDock();
        
        toolDock.Alignment = Alignment.Left;
        
        Assert.Equal(Alignment.Left, toolDock.Alignment);
    }

    [Fact]
    public void ToolDock_IsExpanded_Property_Can_Be_Set()
    {
        var toolDock = new ToolDock();
        
        toolDock.IsExpanded = false;
        
        Assert.False(toolDock.IsExpanded);
    }

    [Fact]
    public void ToolDock_AutoHide_Property_Can_Be_Set()
    {
        var toolDock = new ToolDock();
        
        toolDock.AutoHide = false;
        
        Assert.False(toolDock.AutoHide);
    }

    [Fact]
    public void ToolDock_GripMode_Property_Can_Be_Set()
    {
        var toolDock = new ToolDock();
        
        toolDock.GripMode = GripMode.Hidden;
        
        Assert.Equal(GripMode.Hidden, toolDock.GripMode);
    }

    [Fact]
    public void AddTool_With_Null_Factory_Does_Not_Throw()
    {
        var toolDock = new ToolDock();
        var tool = new Tool { Id = "Tool1" };

        // Should not throw when Factory is null
        var exception = Record.Exception(() => toolDock.AddTool(tool));
        
        Assert.Null(exception);
    }

    [Fact]
    public void AddTool_Can_Be_Overridden()
    {
        var customToolDock = new CustomToolDock();
        var tool = new Tool { Id = "Tool1" };

        customToolDock.AddTool(tool);

        Assert.True(customToolDock.AddToolCalled);
        Assert.Equal(tool, customToolDock.LastToolAdded);
    }

    [Fact]
    public void AddTool_Is_Virtual_Method()
    {
        // Verify that AddTool is virtual by checking if it can be overridden
        var method = typeof(ToolDock).GetMethod(nameof(ToolDock.AddTool));
        
        Assert.NotNull(method);
        Assert.True(method.IsVirtual);
    }

    private class CustomToolDock : ToolDock
    {
        public bool AddToolCalled { get; private set; }
        public IDockable? LastToolAdded { get; private set; }

        public override void AddTool(IDockable tool)
        {
            AddToolCalled = true;
            LastToolAdded = tool;
            base.AddTool(tool);
        }
    }
}
