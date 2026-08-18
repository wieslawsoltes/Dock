using System;
using Avalonia.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlActivationOrderLeakTests
{
    [ReleaseFact]
    public void DockControl_ActivationOrder_DoesNotRetainClosedOrRemovedDockables()
    {
        var result = RunInSession(CreateClosedDockables);

        AssertCollected(result.ClosedDockableRef, result.RemovedDockableRef);
        GC.KeepAlive(result.DockControlKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static ActivationOrderLeakResult CreateClosedDockables()
    {
        var factory = new Factory();
        var firstDocument = new Document { Id = "closed" };
        var secondDocument = new Document { Id = "removed" };
        var documentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>(firstDocument, secondDocument),
            ActiveDockable = firstDocument
        };
        var root = new RootDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>(documentDock),
            ActiveDockable = documentDock,
            DefaultDockable = documentDock
        };

        factory.InitLayout(root);

        var dockControl = new DockControl
        {
            Factory = factory,
            Layout = root,
            InitializeFactory = false,
            InitializeLayout = false
        };

        factory.SetActiveDockable(firstDocument);
        factory.SetActiveDockable(secondDocument);

        var closedDockableRef = new WeakReference(firstDocument);
        var removedDockableRef = new WeakReference(secondDocument);

        factory.CloseDockable(firstDocument);
        factory.RemoveDockable(secondDocument, true);

        return new ActivationOrderLeakResult(
            closedDockableRef,
            removedDockableRef,
            dockControl,
            factory,
            root);
    }

    private sealed record ActivationOrderLeakResult(
        WeakReference ClosedDockableRef,
        WeakReference RemovedDockableRef,
        DockControl DockControlKeepAlive,
        Factory FactoryKeepAlive,
        RootDock LayoutKeepAlive);
}
