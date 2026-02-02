// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class PinnedDockPreviewTests
{
    [Fact]
    public void HidePreviewingDockables_UpdatesActiveDockable_WhenRemovingHiddenDockable()
    {
        var factory = new Factory();
        var root = new RootDock();
        var pinnedDock = new ToolDock
        {
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var hiddenDockable = new Tool
        {
            Id = "Hidden",
            Title = "Hidden",
            KeepPinnedDockableVisible = false
        };
        var keptDockable = new Tool
        {
            Id = "Kept",
            Title = "Kept",
            KeepPinnedDockableVisible = true
        };

        pinnedDock.VisibleDockables.Add(hiddenDockable);
        pinnedDock.VisibleDockables.Add(keptDockable);
        pinnedDock.ActiveDockable = hiddenDockable;
        root.PinnedDock = pinnedDock;

        factory.HidePreviewingDockables(root);

        Assert.Same(pinnedDock, root.PinnedDock);
        Assert.Single(pinnedDock.VisibleDockables);
        Assert.Same(keptDockable, pinnedDock.VisibleDockables[0]);
        Assert.Same(keptDockable, pinnedDock.ActiveDockable);
    }

    [Fact]
    public void PreviewPinnedDockable_SetsActiveDockable()
    {
        var factory = new Factory();
        var root = new RootDock();
        var dockable = new Tool
        {
            Id = "Tool",
            Title = "Tool"
        };

        root.LeftPinnedDockables = factory.CreateList<IDockable>();
        root.LeftPinnedDockables.Add(dockable);

        factory.InitDockable(root, null);

        factory.PreviewPinnedDockable(dockable);

        Assert.NotNull(root.PinnedDock);
        Assert.Same(dockable, root.PinnedDock!.ActiveDockable);
    }
}
