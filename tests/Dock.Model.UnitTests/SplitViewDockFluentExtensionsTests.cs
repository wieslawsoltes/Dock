// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class SplitViewDockFluentExtensionsTests
{
    [Fact]
    public void SplitViewDock_Fluent_Creates_Instance()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock();

        Assert.NotNull(splitViewDock);
        Assert.IsAssignableFrom<ISplitViewDock>(splitViewDock);
    }

    [Fact]
    public void SplitViewDock_Fluent_With_Configure()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock(d =>
        {
            d.Id = "TestSplitView";
            d.Title = "Test";
        });

        Assert.Equal("TestSplitView", splitViewDock.Id);
        Assert.Equal("Test", splitViewDock.Title);
    }

    [Fact]
    public void SplitViewDock_Fluent_Out_Parameter()
    {
        var factory = new Factory();
        var returnedFactory = factory.SplitViewDock(out var splitViewDock);

        Assert.Same(factory, returnedFactory);
        Assert.NotNull(splitViewDock);
        Assert.IsAssignableFrom<ISplitViewDock>(splitViewDock);
    }

    [Fact]
    public void SplitViewDock_Fluent_Out_With_Configure()
    {
        var factory = new Factory();
        factory.SplitViewDock(out var splitViewDock, d =>
        {
            d.Id = "MySplitView";
            d.DisplayMode = SplitViewDisplayMode.CompactOverlay;
        });

        Assert.Equal("MySplitView", splitViewDock.Id);
        Assert.Equal(SplitViewDisplayMode.CompactOverlay, splitViewDock.DisplayMode);
    }

    [Fact]
    public void WithCompactPaneLength_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithCompactPaneLength(64);

        Assert.Equal(64.0, splitViewDock.CompactPaneLength);
    }

    [Fact]
    public void WithDisplayMode_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithDisplayMode(SplitViewDisplayMode.Inline);

        Assert.Equal(SplitViewDisplayMode.Inline, splitViewDock.DisplayMode);
    }

    [Fact]
    public void WithIsPaneOpen_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithIsPaneOpen(true);

        Assert.True(splitViewDock.IsPaneOpen);
    }

    [Fact]
    public void WithOpenPaneLength_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithOpenPaneLength(256);

        Assert.Equal(256.0, splitViewDock.OpenPaneLength);
    }

    [Fact]
    public void WithPanePlacement_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithPanePlacement(SplitViewPanePlacement.Right);

        Assert.Equal(SplitViewPanePlacement.Right, splitViewDock.PanePlacement);
    }

    [Fact]
    public void WithUseLightDismissOverlayMode_Sets_Value()
    {
        var factory = new Factory();
        var splitViewDock = factory.SplitViewDock()
            .WithUseLightDismissOverlayMode(true);

        Assert.True(splitViewDock.UseLightDismissOverlayMode);
    }

    [Fact]
    public void WithPaneDockable_Sets_Value()
    {
        var factory = new Factory();
        var tool = factory.CreateTool();
        var splitViewDock = factory.SplitViewDock()
            .WithPaneDockable(tool);

        Assert.Same(tool, splitViewDock.PaneDockable);
    }

    [Fact]
    public void WithContentDockable_Sets_Value()
    {
        var factory = new Factory();
        var documentDock = factory.CreateDocumentDock();
        var splitViewDock = factory.SplitViewDock()
            .WithContentDockable(documentDock);

        Assert.Same(documentDock, splitViewDock.ContentDockable);
    }

    [Fact]
    public void Fluent_Chaining_All_Properties()
    {
        var factory = new Factory();
        var tool = factory.CreateTool();
        var documentDock = factory.CreateDocumentDock();

        var splitViewDock = factory.SplitViewDock()
            .WithId("MainSplitView")
            .WithTitle("Main")
            .WithCompactPaneLength(48)
            .WithOpenPaneLength(320)
            .WithDisplayMode(SplitViewDisplayMode.CompactOverlay)
            .WithIsPaneOpen(true)
            .WithPanePlacement(SplitViewPanePlacement.Left)
            .WithUseLightDismissOverlayMode(false)
            .WithPaneDockable(tool)
            .WithContentDockable(documentDock);

        Assert.Equal("MainSplitView", splitViewDock.Id);
        Assert.Equal("Main", splitViewDock.Title);
        Assert.Equal(48.0, splitViewDock.CompactPaneLength);
        Assert.Equal(320.0, splitViewDock.OpenPaneLength);
        Assert.Equal(SplitViewDisplayMode.CompactOverlay, splitViewDock.DisplayMode);
        Assert.True(splitViewDock.IsPaneOpen);
        Assert.Equal(SplitViewPanePlacement.Left, splitViewDock.PanePlacement);
        Assert.False(splitViewDock.UseLightDismissOverlayMode);
        Assert.Same(tool, splitViewDock.PaneDockable);
        Assert.Same(documentDock, splitViewDock.ContentDockable);
    }
}
