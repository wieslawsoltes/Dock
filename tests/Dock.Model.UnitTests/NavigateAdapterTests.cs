// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Adapters;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class NavigateAdapterTests
{
    [Fact]
    public void Close_ClosesActiveDockable_ForNonRootDock()
    {
        var factory = new Factory
        {
            HideToolsOnClose = false
        };
        var dock = factory.CreateToolDock();
        var activeDockable = factory.CreateTool();
        activeDockable.CanClose = true;

        factory.InitDockable(dock, null);
        factory.AddDockable(dock, activeDockable);
        dock.ActiveDockable = activeDockable;

        var adapter = new NavigateAdapter(dock);

        adapter.Close();

        Assert.DoesNotContain(activeDockable, dock.VisibleDockables!);
        Assert.Null(dock.ActiveDockable);
    }

    [Fact]
    public void Close_DoesNotCloseActiveDockable_ForRootDock()
    {
        var factory = new TrackingFactory();
        var rootDock = factory.CreateRootDock();
        var activeDockable = factory.CreateTool();
        rootDock.Factory = factory;
        rootDock.ActiveDockable = activeDockable;

        var adapter = new NavigateAdapter(rootDock);

        adapter.Close();

        Assert.Null(factory.ClosedDockable);
    }

    private sealed class TrackingFactory : Factory
    {
        public IDockable? ClosedDockable { get; private set; }

        public override void CloseDockable(IDockable dockable)
        {
            ClosedDockable = dockable;
        }
    }
}
