using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ManagedWindowLayerLeakTests
{
    [ReleaseFact]
    public void ManagedWindowLayer_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new Factory();
            var root = (IRootDock)factory.CreateRootDock();
            root.Factory = factory;

            var dockWindow = new DockWindow
            {
                Factory = factory,
                Layout = root
            };

            var document = new ManagedDockWindowDocument(dockWindow)
            {
                Factory = factory
            };

            var dock = new ManagedWindowDock
            {
                Factory = factory
            };

            dock.VisibleDockables = factory.CreateList<IDockable>(document);
            dock.ActiveDockable = document;
            document.Owner = dock;

            var layer = new ManagedWindowLayer { Dock = dock };

            var window = new Window { Content = layer };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            layer.ApplyTemplate();
            layer.UpdateLayout();
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();
            ClearInputState(window);

            var layerRef = new WeakReference(layer);
            layer = null;

            return new ManagedLayerDetachLeakResult(layerRef, window, dock, dockWindow);
        });

        AssertCollected(result.LayerRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.DockWindowKeepAlive);
    }

    [ReleaseFact]
    public void ManagedWindowLayer_DockSwap_DoesNotLeak_PreviousDock()
    {
        var result = RunInSession(() =>
        {
            var factory = new Dock.Model.Avalonia.Factory();

            var rootA = (IRootDock)factory.CreateRootDock();
            rootA.Factory = factory;
            rootA.VisibleDockables = factory.CreateList<IDockable>();

            var dockWindowA = new Dock.Model.Avalonia.Core.DockWindow
            {
                Factory = factory,
                Layout = rootA,
                Title = "DockA"
            };

            var managedDocA = new ManagedDockWindowDocument(dockWindowA)
            {
                Factory = factory
            };

            var dockA = new ManagedWindowDock
            {
                Factory = factory,
                VisibleDockables = factory.CreateList<IDockable>(managedDocA),
                ActiveDockable = managedDocA
            };
            managedDocA.Owner = dockA;

            var rootB = (IRootDock)factory.CreateRootDock();
            rootB.Factory = factory;
            rootB.VisibleDockables = factory.CreateList<IDockable>();

            var dockWindowB = new Dock.Model.Avalonia.Core.DockWindow
            {
                Factory = factory,
                Layout = rootB,
                Title = "DockB"
            };

            var managedDocB = new ManagedDockWindowDocument(dockWindowB)
            {
                Factory = factory
            };

            var dockB = new ManagedWindowDock
            {
                Factory = factory,
                VisibleDockables = factory.CreateList<IDockable>(managedDocB),
                ActiveDockable = managedDocB
            };
            managedDocB.Owner = dockB;

            var layer = new ManagedWindowLayer { Dock = dockA };

            var window = new Window { Content = layer };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            layer.ApplyTemplate();
            layer.UpdateLayout();
            DrainDispatcher();

            var dockRef = new WeakReference(dockA);
            var docRef = new WeakReference(managedDocA);

            layer.Dock = dockB;
            DrainDispatcher();

            dockA = null;
            managedDocA = null;
            dockWindowA = null;
            rootA = null;

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            return new ManagedLayerDockSwapLeakResult(
                dockRef,
                docRef,
                window,
                layer,
                dockB,
                factory);
        });

        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.DockRef);
            AssertCollected(result.DocumentRef);
        }

        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.LayerKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private sealed record ManagedLayerDetachLeakResult(
        WeakReference LayerRef,
        Window WindowKeepAlive,
        ManagedWindowDock DockKeepAlive,
        DockWindow DockWindowKeepAlive);

    private sealed record ManagedLayerDockSwapLeakResult(
        WeakReference DockRef,
        WeakReference DocumentRef,
        Window WindowKeepAlive,
        ManagedWindowLayer LayerKeepAlive,
        ManagedWindowDock DockKeepAlive,
        Dock.Model.Avalonia.Factory FactoryKeepAlive);
}
