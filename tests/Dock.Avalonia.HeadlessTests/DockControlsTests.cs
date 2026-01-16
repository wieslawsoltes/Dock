using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using System;
using Dock.Model.Avalonia.Core;
using Avalonia.Layout;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

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
    public void SplitViewDock_Defaults()
    {
        var dock = new SplitViewDock();
        Assert.Equal(48.0, dock.CompactPaneLength);
        Assert.Equal(320.0, dock.OpenPaneLength);
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.Overlay, dock.DisplayMode);
        Assert.False(dock.IsPaneOpen);
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Left, dock.PanePlacement);
        Assert.False(dock.UseLightDismissOverlayMode);
        Assert.Null(dock.PaneDockable);
        Assert.Null(dock.ContentDockable);
    }

    [AvaloniaFact]
    public void SplitViewDock_Set_Properties()
    {
        var dock = new SplitViewDock
        {
            CompactPaneLength = 64,
            OpenPaneLength = 256,
            DisplayMode = Dock.Model.Core.SplitViewDisplayMode.CompactOverlay,
            IsPaneOpen = true,
            PanePlacement = Dock.Model.Core.SplitViewPanePlacement.Right,
            UseLightDismissOverlayMode = true
        };
        
        Assert.Equal(64.0, dock.CompactPaneLength);
        Assert.Equal(256.0, dock.OpenPaneLength);
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.CompactOverlay, dock.DisplayMode);
        Assert.True(dock.IsPaneOpen);
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Right, dock.PanePlacement);
        Assert.True(dock.UseLightDismissOverlayMode);
    }

    [AvaloniaFact]
    public void SplitViewDock_Set_PaneDockable_And_ContentDockable()
    {
        var factory = new Factory();
        var tool = factory.CreateTool();
        var documentDock = factory.CreateDocumentDock();
        
        var splitViewDock = new SplitViewDock
        {
            PaneDockable = tool,
            ContentDockable = documentDock
        };
        
        Assert.Same(tool, splitViewDock.PaneDockable);
        Assert.Same(documentDock, splitViewDock.ContentDockable);
    }

    [AvaloniaFact]
    public void SplitViewDock_All_DisplayModes()
    {
        var dock1 = new SplitViewDock { DisplayMode = Dock.Model.Core.SplitViewDisplayMode.Inline };
        var dock2 = new SplitViewDock { DisplayMode = Dock.Model.Core.SplitViewDisplayMode.CompactInline };
        var dock3 = new SplitViewDock { DisplayMode = Dock.Model.Core.SplitViewDisplayMode.Overlay };
        var dock4 = new SplitViewDock { DisplayMode = Dock.Model.Core.SplitViewDisplayMode.CompactOverlay };
        
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.Inline, dock1.DisplayMode);
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.CompactInline, dock2.DisplayMode);
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.Overlay, dock3.DisplayMode);
        Assert.Equal(Dock.Model.Core.SplitViewDisplayMode.CompactOverlay, dock4.DisplayMode);
    }

    [AvaloniaFact]
    public void SplitViewDock_All_PanePlacements()
    {
        var dock1 = new SplitViewDock { PanePlacement = Dock.Model.Core.SplitViewPanePlacement.Left };
        var dock2 = new SplitViewDock { PanePlacement = Dock.Model.Core.SplitViewPanePlacement.Right };
        var dock3 = new SplitViewDock { PanePlacement = Dock.Model.Core.SplitViewPanePlacement.Top };
        var dock4 = new SplitViewDock { PanePlacement = Dock.Model.Core.SplitViewPanePlacement.Bottom };
        
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Left, dock1.PanePlacement);
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Right, dock2.PanePlacement);
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Top, dock3.PanePlacement);
        Assert.Equal(Dock.Model.Core.SplitViewPanePlacement.Bottom, dock4.PanePlacement);
    }
}
