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

    private sealed record ManagedLayerDetachLeakResult(
        WeakReference LayerRef,
        Window WindowKeepAlive,
        ManagedWindowDock DockKeepAlive,
        DockWindow DockWindowKeepAlive);
}
