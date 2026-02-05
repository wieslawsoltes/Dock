using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlFloatingLeakTests
{
    [ReleaseFact]
    public void DockControl_FloatDockable_NativeHost_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var previousMode = DockSettings.FloatingWindowHostMode;
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

            try
            {
                var context = LeakContext.Create();
                context.Document.CanFloat = true;
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;

                var dockControl = new DockControl
                {
                    Factory = context.Factory,
                    Layout = context.Root,
                    InitializeFactory = true,
                    InitializeLayout = true
                };
                var window = new Window { Content = dockControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);

                context.Factory.FloatDockable(context.Document);
                DrainDispatcher();

                var dockWindow = context.Root.Windows?.FirstOrDefault();
                Assert.NotNull(dockWindow);

                var host = dockWindow!.Host;
                var hostWindow = host as Window;

                var result = new FloatLeakResult(
                    new WeakReference(dockControl),
                    new WeakReference(context.Root),
                    dockWindow is null ? null : new WeakReference(dockWindow),
                    host is null ? null : new WeakReference(host),
                    hostWindow is null ? null : new WeakReference(hostWindow));

                context.Factory.RemoveWindow(dockWindow!);
                DrainDispatcher();

                if (hostWindow is not null)
                {
                    CleanupWindow(hostWindow);
                }

                CleanupWindow(window);
                return result;
            }
            finally
            {
                DockSettings.FloatingWindowHostMode = previousMode;
            }
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
        if (result.DockWindowRef is not null)
        {
            AssertCollected(result.DockWindowRef);
        }
        if (result.HostRef is not null)
        {
            AssertCollected(result.HostRef);
        }
        if (result.HostWindowRef is not null)
        {
            AssertCollected(result.HostWindowRef);
        }
    }

    [ReleaseFact]
    public void DockControl_FloatDockable_ManagedHost_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var previousMode = DockSettings.FloatingWindowHostMode;
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;

            try
            {
                var context = LeakContext.Create();
                context.Document.CanFloat = true;
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;

                var dockControl = new DockControl
                {
                    Factory = context.Factory,
                    Layout = context.Root,
                    InitializeFactory = true,
                    InitializeLayout = true
                };
                var window = new Window { Content = dockControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);

                context.Factory.FloatDockable(context.Document);
                DrainDispatcher();

                var dockWindow = context.Root.Windows?.FirstOrDefault();
                Assert.NotNull(dockWindow);

                var host = dockWindow!.Host;
                Assert.IsType<ManagedHostWindow>(host);

                var managedLayer = FindVisualDescendant<ManagedWindowLayer>(dockControl);
                var managedDock = managedLayer?.Dock;
                var managedDocument = managedDock?.VisibleDockables?.OfType<ManagedDockWindowDocument>().FirstOrDefault();

                var result = new ManagedFloatLeakResult(
                    new WeakReference(dockControl),
                    new WeakReference(context.Root),
                    dockWindow is null ? null : new WeakReference(dockWindow),
                    host is null ? null : new WeakReference(host),
                    managedDocument is null ? null : new WeakReference(managedDocument));

                context.Factory.RemoveWindow(dockWindow!);
                DrainDispatcher();

                CleanupWindow(window);
                return result;
            }
            finally
            {
                DockSettings.FloatingWindowHostMode = previousMode;
            }
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
        if (result.DockWindowRef is not null)
        {
            AssertCollected(result.DockWindowRef);
        }
        if (result.HostRef is not null)
        {
            AssertCollected(result.HostRef);
        }
        if (result.ManagedDocumentRef is not null)
        {
            AssertCollected(result.ManagedDocumentRef);
        }
    }
}
