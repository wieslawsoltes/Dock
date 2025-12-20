// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using SplitViewDisplayMode = Dock.Model.Core.SplitViewDisplayMode;
using SplitViewPanePlacement = Dock.Model.Core.SplitViewPanePlacement;

namespace Dock.Avalonia.HeadlessTests;

public class SplitViewDockControlTests
{
    [AvaloniaFact]
    public void SplitViewDockControl_Can_Be_Created()
    {
        var control = new SplitViewDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void SplitViewDockControl_With_DataContext()
    {
        var splitViewDock = new SplitViewDock
        {
            Id = "Test",
            Title = "Test SplitViewDock",
            DisplayMode = SplitViewDisplayMode.CompactOverlay,
            IsPaneOpen = false
        };

        var control = new SplitViewDockControl
        {
            DataContext = splitViewDock
        };

        Assert.NotNull(control);
        Assert.Same(splitViewDock, control.DataContext);
    }

    [AvaloniaFact]
    public void SplitViewDock_Factory_CreateSplitViewDock()
    {
        var factory = new Factory();
        var splitViewDock = factory.CreateSplitViewDock();

        Assert.NotNull(splitViewDock);
        Assert.IsType<SplitViewDock>(splitViewDock);
        Assert.Equal(48.0, splitViewDock.CompactPaneLength);
        Assert.Equal(320.0, splitViewDock.OpenPaneLength);
        Assert.Equal(SplitViewDisplayMode.Overlay, splitViewDock.DisplayMode);
        Assert.False(splitViewDock.IsPaneOpen);
        Assert.Equal(SplitViewPanePlacement.Left, splitViewDock.PanePlacement);
    }

    [AvaloniaFact]
    public void SplitViewDock_With_Pane_And_Content()
    {
        var factory = new Factory();
        var tool = factory.CreateTool();
        tool.Id = "NavTool";
        tool.Title = "Navigation";

        var documentDock = factory.CreateDocumentDock();
        documentDock.Id = "DocDock";
        documentDock.Title = "Documents";

        var splitViewDock = factory.CreateSplitViewDock();
        splitViewDock.Id = "SplitView";
        splitViewDock.Title = "Main";
        splitViewDock.PaneDockable = tool;
        splitViewDock.ContentDockable = documentDock;
        splitViewDock.DisplayMode = SplitViewDisplayMode.CompactInline;
        splitViewDock.IsPaneOpen = true;

        Assert.Same(tool, splitViewDock.PaneDockable);
        Assert.Same(documentDock, splitViewDock.ContentDockable);
        Assert.Equal(SplitViewDisplayMode.CompactInline, splitViewDock.DisplayMode);
        Assert.True(splitViewDock.IsPaneOpen);
    }

    [AvaloniaFact]
    public void SplitViewDock_Toggle_IsPaneOpen()
    {
        var splitViewDock = new SplitViewDock
        {
            IsPaneOpen = false
        };

        Assert.False(splitViewDock.IsPaneOpen);

        splitViewDock.IsPaneOpen = true;
        Assert.True(splitViewDock.IsPaneOpen);

        splitViewDock.IsPaneOpen = false;
        Assert.False(splitViewDock.IsPaneOpen);
    }

    [AvaloniaFact]
    public void SplitViewDock_Change_DisplayMode()
    {
        var splitViewDock = new SplitViewDock();

        splitViewDock.DisplayMode = SplitViewDisplayMode.Inline;
        Assert.Equal(SplitViewDisplayMode.Inline, splitViewDock.DisplayMode);

        splitViewDock.DisplayMode = SplitViewDisplayMode.CompactInline;
        Assert.Equal(SplitViewDisplayMode.CompactInline, splitViewDock.DisplayMode);

        splitViewDock.DisplayMode = SplitViewDisplayMode.Overlay;
        Assert.Equal(SplitViewDisplayMode.Overlay, splitViewDock.DisplayMode);

        splitViewDock.DisplayMode = SplitViewDisplayMode.CompactOverlay;
        Assert.Equal(SplitViewDisplayMode.CompactOverlay, splitViewDock.DisplayMode);
    }

    [AvaloniaFact]
    public void SplitViewDock_Change_PanePlacement()
    {
        var splitViewDock = new SplitViewDock();

        splitViewDock.PanePlacement = SplitViewPanePlacement.Left;
        Assert.Equal(SplitViewPanePlacement.Left, splitViewDock.PanePlacement);

        splitViewDock.PanePlacement = SplitViewPanePlacement.Right;
        Assert.Equal(SplitViewPanePlacement.Right, splitViewDock.PanePlacement);

        splitViewDock.PanePlacement = SplitViewPanePlacement.Top;
        Assert.Equal(SplitViewPanePlacement.Top, splitViewDock.PanePlacement);

        splitViewDock.PanePlacement = SplitViewPanePlacement.Bottom;
        Assert.Equal(SplitViewPanePlacement.Bottom, splitViewDock.PanePlacement);
    }

    [AvaloniaFact]
    public void SplitViewDock_Custom_Lengths()
    {
        var splitViewDock = new SplitViewDock
        {
            CompactPaneLength = 64,
            OpenPaneLength = 256
        };

        Assert.Equal(64.0, splitViewDock.CompactPaneLength);
        Assert.Equal(256.0, splitViewDock.OpenPaneLength);
    }

    [AvaloniaFact]
    public void SplitViewDock_UseLightDismissOverlayMode()
    {
        var splitViewDock = new SplitViewDock();
        Assert.False(splitViewDock.UseLightDismissOverlayMode);

        splitViewDock.UseLightDismissOverlayMode = true;
        Assert.True(splitViewDock.UseLightDismissOverlayMode);
    }

    [AvaloniaFact]
    public void SplitViewDock_Implements_ISplitViewDock()
    {
        var splitViewDock = new SplitViewDock();
        Assert.IsAssignableFrom<ISplitViewDock>(splitViewDock);
    }

    [AvaloniaFact]
    public void SplitViewDock_Implements_IDock()
    {
        var splitViewDock = new SplitViewDock();
        Assert.IsAssignableFrom<IDock>(splitViewDock);
    }
}
