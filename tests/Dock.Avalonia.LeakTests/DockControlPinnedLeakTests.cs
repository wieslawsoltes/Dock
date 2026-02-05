using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
public class DockControlPinnedLeakTests
{
    [ReleaseFact]
    public void PinnedDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var previousPinned = DockSettings.UsePinnedDockWindow;
        var previousMode = DockSettings.FloatingWindowHostMode;
        DockSettings.UsePinnedDockWindow = true;
        DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Factory.InitLayout(context.Root);
                context.Root.PinnedDockDisplayMode = PinnedDockDisplayMode.Overlay;

                var rootControl = new RootDockControl { DataContext = context.Root };
                var window = new Window { Content = rootControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);
                DrainDispatcher();
                rootControl.ApplyTemplate();
                rootControl.UpdateLayout();
                DrainDispatcher();

                var pinnedDockControl = FindVisualDescendant<PinnedDockControl>(rootControl);
                Assert.NotNull(pinnedDockControl);

                window.Content = new Border();
                DrainDispatcher();
                ClearFactoryCaches(context.Factory);

                var pinnedRef = new WeakReference(pinnedDockControl);
                pinnedDockControl = null;
                rootControl = null;

                return new PinnedDetachLeakResult(pinnedRef, window, context.Factory);
            });

            AssertCollected(result.ControlRef);
            GC.KeepAlive(result.WindowKeepAlive);
            GC.KeepAlive(result.FactoryKeepAlive);
        }
        finally
        {
            DockSettings.UsePinnedDockWindow = previousPinned;
            DockSettings.FloatingWindowHostMode = previousMode;
        }
    }

    [ReleaseFact]
    public void PinnedDockControl_OverlayWindow_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var previousPinned = DockSettings.UsePinnedDockWindow;
            var previousMode = DockSettings.FloatingWindowHostMode;
            DockSettings.UsePinnedDockWindow = true;
            DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

            try
            {
                var context = LeakContext.Create();
                context.Root.PinnedDockDisplayMode = PinnedDockDisplayMode.Overlay;
                context.Factory.InitLayout(context.Root);

                var rootControl = new RootDockControl { DataContext = context.Root };
                var window = new Window { Content = rootControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);
                DrainDispatcher();
                rootControl.ApplyTemplate();
                rootControl.UpdateLayout();
                DrainDispatcher();

                var pinnedDockControl = FindVisualDescendant<PinnedDockControl>(rootControl);
                if (pinnedDockControl is null)
                {
                    TraceVisualTree(rootControl, "PinnedDockControl/visuals");
                }
                Assert.NotNull(pinnedDockControl);

                var pinnedContent = pinnedDockControl is not null
                    ? FindVisualDescendant<ContentControl>(pinnedDockControl, control => control.Name == "PART_PinnedDock")
                    : null;

                Assert.NotNull(pinnedContent);
                var overlayActive = pinnedContent!.Opacity == 0;

                var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                var pinnedWindow = lifetime?.Windows
                    .OfType<PinnedDockWindow>()
                    .FirstOrDefault();

                var result = new PinnedWindowLeakResult(
                    new WeakReference(rootControl),
                    new WeakReference(context.Root),
                    pinnedWindow is null ? null : new WeakReference(pinnedWindow),
                    overlayActive);

                context.Root.PinnedDockDisplayMode = PinnedDockDisplayMode.Inline;
                DrainDispatcher();
                context.Root.PinnedDock = null;
                context.Root.LeftPinnedDockables?.Clear();
                DrainDispatcher();
                if (context.Root.Close.CanExecute(null))
                {
                    context.Root.Close.Execute(null);
                    DrainDispatcher();
                }
                context.Factory.VisibleDockableControls.Clear();
                context.Factory.VisibleRootControls.Clear();
                context.Factory.PinnedDockableControls.Clear();
                context.Factory.PinnedRootControls.Clear();
                context.Factory.TabDockableControls.Clear();
                context.Factory.TabRootControls.Clear();
                context.Factory.ToolControls.Clear();
                context.Factory.DocumentControls.Clear();
                context.Factory.DockControls.Clear();
                context.Factory.HostWindows.Clear();
                rootControl.DataContext = null;
                DrainDispatcher();

                CleanupWindow(window);
                return result;
            }
            finally
            {
                DockSettings.UsePinnedDockWindow = previousPinned;
                DockSettings.FloatingWindowHostMode = previousMode;
            }
        });

        Assert.True(result.OverlayActive, "Pinned dock overlay did not activate.");
        if (result.PinnedWindowRef is not null)
        {
            AssertCollected(result.PinnedWindowRef);
        }
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
            AssertCollected(result.LayoutRef);
        }
    }

    private sealed record PinnedDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        Dock.Model.Core.IFactory FactoryKeepAlive);
}
