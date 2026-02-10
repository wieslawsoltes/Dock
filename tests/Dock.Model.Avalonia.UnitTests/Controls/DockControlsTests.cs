using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using System;
using Dock.Model.Avalonia.Core;
using Avalonia.Layout;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DockControlsTests
{
    [AvaloniaFact]
    public void DockDock_Default_LastChildFill_True()
    {
        var dock = new DockDock();
        Assert.True(dock.LastChildFill);
    }

    [AvaloniaFact]
    public void StackDock_Defaults()
    {
        var dock = new StackDock();
        Assert.Equal(Dock.Model.Core.Orientation.Horizontal, dock.Orientation);
        Assert.Equal(0, dock.Spacing);
    }

    [AvaloniaFact]
    public void WrapDock_Defaults()
    {
        var dock = new WrapDock();
        Assert.Equal(Dock.Model.Core.Orientation.Horizontal, dock.Orientation);
    }

    [AvaloniaFact]
    public void UniformGridDock_Defaults()
    {
        var dock = new UniformGridDock();
        Assert.Equal(0, dock.Rows);
        Assert.Equal(0, dock.Columns);
    }

    [AvaloniaFact]
    public void GridDock_Defaults()
    {
        var dock = new GridDock();
        Assert.Null(dock.ColumnDefinitions);
        Assert.Null(dock.RowDefinitions);
    }

    [AvaloniaFact]
    public void ProportionalDock_Default_Orientation_Horizontal()
    {
        var dock = new ProportionalDock();
        Assert.Equal(Dock.Model.Core.Orientation.Horizontal, dock.Orientation);
    }

    [AvaloniaFact]
    public void ProportionalDockSplitter_CanResize_Default_True()
    {
        var splitter = new ProportionalDockSplitter();
        Assert.True(splitter.CanResize);
        Assert.False(splitter.ResizePreview);
    }

    [AvaloniaFact]
    public void GridDockSplitter_Defaults()
    {
        var splitter = new GridDockSplitter();
        Assert.Equal(0, splitter.Column);
        Assert.Equal(0, splitter.Row);
        Assert.Equal(Dock.Model.Core.GridResizeDirection.Columns, splitter.ResizeDirection);
    }

    [AvaloniaFact]
    public void RootDock_Collections_NotNull()
    {
        var dock = new RootDock();
        Assert.NotNull(dock.HiddenDockables);
        Assert.NotNull(dock.LeftPinnedDockables);
        Assert.NotNull(dock.RightPinnedDockables);
        Assert.NotNull(dock.TopPinnedDockables);
        Assert.NotNull(dock.BottomPinnedDockables);
        Assert.NotNull(dock.Windows);
    }

    [AvaloniaFact]
    public void ToolDock_Defaults()
    {
        var dock = new ToolDock();
        Assert.Equal(Alignment.Unset, dock.Alignment);
        Assert.False(dock.IsExpanded);
        Assert.True(dock.AutoHide);
        Assert.Equal(GripMode.Visible, dock.GripMode);
    }

    [AvaloniaFact]
    public void DocumentDock_CreateDocumentFromTemplate_Creates_Document()
    {
        var factory = new Factory();
        var dock = new DocumentDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(),
            CanCreateDocument = true,
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };

        var document = dock.CreateDocumentFromTemplate();

        Assert.NotNull(document);
        Assert.IsType<Document>(document);
        Assert.Single(dock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void Document_Match_Uses_DataType()
    {
        var doc = new Document { DataType = typeof(string) };
        Assert.True(doc.Match("text"));
        Assert.False(doc.Match(123));
    }

    [AvaloniaFact]
    public void Tool_Match_Uses_DataType()
    {
        var tool = new Tool { DataType = typeof(int) };
        Assert.True(tool.Match(42));
        Assert.False(tool.Match("text"));
    }

    [AvaloniaFact]
    public void DocumentTemplate_Match_Uses_DataType()
    {
        var template = new DocumentTemplate { DataType = typeof(TextBlock) };
        Assert.True(template.Match(new TextBlock()));
        Assert.False(template.Match("text"));
    }

    [AvaloniaFact]
    public void ToolTemplate_Match_Uses_DataType()
    {
        var template = new ToolTemplate { DataType = typeof(TextBlock) };
        Assert.True(template.Match(new TextBlock()));
        Assert.False(template.Match("text"));
    }

    [AvaloniaFact]
    public void DocumentTemplate_Build_Supports_Func_Returning_Control()
    {
        var template = new DocumentTemplate
        {
            Content = new Func<IServiceProvider, object>(_ => new StackPanel())
        };

        var control = template.Build(null, null);

        Assert.IsType<StackPanel>(control);
    }

    [AvaloniaFact]
    public void ToolTemplate_Build_Supports_Func_Returning_Control()
    {
        var template = new ToolTemplate
        {
            Content = new Func<IServiceProvider, object>(_ => new StackPanel())
        };

        var control = template.Build(null, null);

        Assert.IsType<StackPanel>(control);
    }
}
