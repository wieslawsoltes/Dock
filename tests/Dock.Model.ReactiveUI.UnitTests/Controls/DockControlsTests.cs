using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls;

public class DockControlsTests
{
    [Fact]
    public void DockDock_Default_LastChildFill_True()
    {
        var dock = new DockDock();
        Assert.True(dock.LastChildFill);
    }

    [Fact]
    public void StackDock_Defaults()
    {
        var dock = new StackDock();
        Assert.Equal(Orientation.Horizontal, dock.Orientation);
        Assert.Equal(0, dock.Spacing);
    }

    [Fact]
    public void WrapDock_Defaults()
    {
        var dock = new WrapDock();
        Assert.Equal(Orientation.Horizontal, dock.Orientation);
    }

    [Fact]
    public void UniformGridDock_Defaults()
    {
        var dock = new UniformGridDock();
        Assert.Equal(0, dock.Rows);
        Assert.Equal(0, dock.Columns);
    }

    [Fact]
    public void GridDock_Defaults()
    {
        var dock = new GridDock();
        Assert.Null(dock.ColumnDefinitions);
        Assert.Null(dock.RowDefinitions);
    }

    [Fact]
    public void ProportionalDock_Default_Orientation_Horizontal()
    {
        var dock = new ProportionalDock();
        Assert.Equal(Orientation.Horizontal, dock.Orientation);
    }

    [Fact]
    public void ProportionalDockSplitter_CanResize_Default_True()
    {
        var splitter = new ProportionalDockSplitter();
        Assert.True(splitter.CanResize);
    }

    [Fact]
    public void GridDockSplitter_Defaults()
    {
        var splitter = new GridDockSplitter();
        Assert.Equal(0, splitter.Column);
        Assert.Equal(0, splitter.Row);
        Assert.Equal(GridResizeDirection.Columns, splitter.ResizeDirection);
    }

    [Fact]
    public void ToolDock_Defaults()
    {
        var dock = new ToolDock();
        Assert.Equal(Alignment.Unset, dock.Alignment);
        Assert.False(dock.IsExpanded);
        Assert.True(dock.AutoHide);
        Assert.Equal(GripMode.Visible, dock.GripMode);
    }

    [Fact]
    public void DocumentDock_Defaults()
    {
        var dock = new DocumentDock();
        Assert.False(dock.CanCreateDocument);
        Assert.Null(dock.DocumentFactory);
        Assert.NotNull(dock.CreateDocument);
        Assert.False(dock.EnableWindowDrag);
        Assert.Equal(DocumentTabLayout.Top, dock.TabsLayout);
        Assert.Equal("No documents open", dock.EmptyContent);
    }
}
