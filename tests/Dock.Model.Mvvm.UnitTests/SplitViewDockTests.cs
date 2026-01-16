// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Xunit;

namespace Dock.Model.Mvvm.UnitTests;

public class SplitViewDockTests
{
    [Fact]
    public void SplitViewDock_Constructor_Creates_Instance()
    {
        var splitViewDock = new SplitViewDock();
        Assert.NotNull(splitViewDock);
    }

    [Fact]
    public void SplitViewDock_Default_CompactPaneLength_Is_48()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Equal(48.0, splitViewDock.CompactPaneLength);
    }

    [Fact]
    public void SplitViewDock_Default_OpenPaneLength_Is_320()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Equal(320.0, splitViewDock.OpenPaneLength);
    }

    [Fact]
    public void SplitViewDock_Default_DisplayMode_Is_Overlay()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Equal(SplitViewDisplayMode.Overlay, splitViewDock.DisplayMode);
    }

    [Fact]
    public void SplitViewDock_Default_IsPaneOpen_Is_False()
    {
        var splitViewDock = new SplitViewDock();
        Assert.False(splitViewDock.IsPaneOpen);
    }

    [Fact]
    public void SplitViewDock_Default_PanePlacement_Is_Left()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Equal(SplitViewPanePlacement.Left, splitViewDock.PanePlacement);
    }

    [Fact]
    public void SplitViewDock_Default_UseLightDismissOverlayMode_Is_False()
    {
        var splitViewDock = new SplitViewDock();
        Assert.False(splitViewDock.UseLightDismissOverlayMode);
    }

    [Fact]
    public void SplitViewDock_Default_PaneDockable_Is_Null()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Null(splitViewDock.PaneDockable);
    }

    [Fact]
    public void SplitViewDock_Default_ContentDockable_Is_Null()
    {
        var splitViewDock = new SplitViewDock();
        Assert.Null(splitViewDock.ContentDockable);
    }

    [Fact]
    public void SplitViewDock_Set_CompactPaneLength()
    {
        var splitViewDock = new SplitViewDock { CompactPaneLength = 64.0 };
        Assert.Equal(64.0, splitViewDock.CompactPaneLength);
    }

    [Fact]
    public void SplitViewDock_Set_OpenPaneLength()
    {
        var splitViewDock = new SplitViewDock { OpenPaneLength = 256.0 };
        Assert.Equal(256.0, splitViewDock.OpenPaneLength);
    }

    [Fact]
    public void SplitViewDock_Set_DisplayMode_Inline()
    {
        var splitViewDock = new SplitViewDock { DisplayMode = SplitViewDisplayMode.Inline };
        Assert.Equal(SplitViewDisplayMode.Inline, splitViewDock.DisplayMode);
    }

    [Fact]
    public void SplitViewDock_Set_DisplayMode_CompactInline()
    {
        var splitViewDock = new SplitViewDock { DisplayMode = SplitViewDisplayMode.CompactInline };
        Assert.Equal(SplitViewDisplayMode.CompactInline, splitViewDock.DisplayMode);
    }

    [Fact]
    public void SplitViewDock_Set_DisplayMode_CompactOverlay()
    {
        var splitViewDock = new SplitViewDock { DisplayMode = SplitViewDisplayMode.CompactOverlay };
        Assert.Equal(SplitViewDisplayMode.CompactOverlay, splitViewDock.DisplayMode);
    }

    [Fact]
    public void SplitViewDock_Set_IsPaneOpen_True()
    {
        var splitViewDock = new SplitViewDock { IsPaneOpen = true };
        Assert.True(splitViewDock.IsPaneOpen);
    }

    [Fact]
    public void SplitViewDock_Set_PanePlacement_Right()
    {
        var splitViewDock = new SplitViewDock { PanePlacement = SplitViewPanePlacement.Right };
        Assert.Equal(SplitViewPanePlacement.Right, splitViewDock.PanePlacement);
    }

    [Fact]
    public void SplitViewDock_Set_PanePlacement_Top()
    {
        var splitViewDock = new SplitViewDock { PanePlacement = SplitViewPanePlacement.Top };
        Assert.Equal(SplitViewPanePlacement.Top, splitViewDock.PanePlacement);
    }

    [Fact]
    public void SplitViewDock_Set_PanePlacement_Bottom()
    {
        var splitViewDock = new SplitViewDock { PanePlacement = SplitViewPanePlacement.Bottom };
        Assert.Equal(SplitViewPanePlacement.Bottom, splitViewDock.PanePlacement);
    }

    [Fact]
    public void SplitViewDock_Set_UseLightDismissOverlayMode_True()
    {
        var splitViewDock = new SplitViewDock { UseLightDismissOverlayMode = true };
        Assert.True(splitViewDock.UseLightDismissOverlayMode);
    }

    [Fact]
    public void SplitViewDock_Set_PaneDockable()
    {
        var factory = new Factory();
        var tool = factory.CreateTool();
        var splitViewDock = new SplitViewDock { PaneDockable = tool };
        Assert.Same(tool, splitViewDock.PaneDockable);
    }

    [Fact]
    public void SplitViewDock_Set_ContentDockable()
    {
        var factory = new Factory();
        var documentDock = factory.CreateDocumentDock();
        var splitViewDock = new SplitViewDock { ContentDockable = documentDock };
        Assert.Same(documentDock, splitViewDock.ContentDockable);
    }

    [Fact]
    public void SplitViewDock_Implements_ISplitViewDock()
    {
        var splitViewDock = new SplitViewDock();
        Assert.IsAssignableFrom<ISplitViewDock>(splitViewDock);
    }

    [Fact]
    public void SplitViewDock_Implements_IDock()
    {
        var splitViewDock = new SplitViewDock();
        Assert.IsAssignableFrom<IDock>(splitViewDock);
    }

    [Fact]
    public void SplitViewDock_PropertyChanged_CompactPaneLength()
    {
        var splitViewDock = new SplitViewDock();
        var propertyChanged = false;

        splitViewDock.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SplitViewDock.CompactPaneLength))
            {
                propertyChanged = true;
            }
        };

        splitViewDock.CompactPaneLength = 100.0;
        Assert.True(propertyChanged);
    }

    [Fact]
    public void SplitViewDock_PropertyChanged_IsPaneOpen()
    {
        var splitViewDock = new SplitViewDock();
        var propertyChanged = false;

        splitViewDock.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SplitViewDock.IsPaneOpen))
            {
                propertyChanged = true;
            }
        };

        splitViewDock.IsPaneOpen = true;
        Assert.True(propertyChanged);
    }

    [Fact]
    public void SplitViewDock_InitLayout_Sets_Pane_Content_Owner()
    {
        var factory = new Factory();
        var paneTool = factory.CreateTool();
        var contentDock = factory.CreateDocumentDock();
        var splitViewDock = new SplitViewDock
        {
            Id = "SplitView",
            PaneDockable = paneTool,
            ContentDockable = contentDock
        };
        var rootDock = new RootDock
        {
            Id = "Root",
            VisibleDockables = factory.CreateList<IDockable>(splitViewDock),
            ActiveDockable = splitViewDock,
            DefaultDockable = splitViewDock
        };

        factory.InitLayout(rootDock);

        Assert.Same(splitViewDock, paneTool.Owner);
        Assert.Same(splitViewDock, contentDock.Owner);
        Assert.Same(factory, contentDock.Factory);
    }
}
