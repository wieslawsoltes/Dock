using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockableControlLeakTests
{
    [ReleaseFact]
    public void DockableControl_TrackingModeVisible_DoesNotLeak_WhenDockableAlive()
    {
        var result = RunTrackingModeLeak(TrackingMode.Visible);

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.DockableKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DockableControl_TrackingModePinned_DoesNotLeak_WhenDockableAlive()
    {
        var result = RunTrackingModeLeak(TrackingMode.Pinned);

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.DockableKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DockableControl_TrackingModeTab_DoesNotLeak_WhenDockableAlive()
    {
        var result = RunTrackingModeLeak(TrackingMode.Tab);

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.DockableKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DockableControl_SwapDockable_DoesNotLeak_PreviousDockable()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dockableA = context.Document;
            var dockableB = context.Factory.CreateDocument();
            dockableB.Factory = context.Factory;

            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = dockableA
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            control.DataContext = dockableB;
            DrainDispatcher();

            var result = new DockableControlSwapLeakResult(
                new WeakReference(control),
                dockableA,
                dockableB,
                context.Factory);

            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.DockableAKeepAlive);
        GC.KeepAlive(result.DockableBKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DockableControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dockable = context.Document;

            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = dockable
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockableControlDetachLeakResult(controlRef, window, dockable, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockableControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockableKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private static DockableControlLeakResult RunTrackingModeLeak(TrackingMode trackingMode)
    {
        return RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dockable = context.Document;

            var control = new DockableControl
            {
                TrackingMode = trackingMode,
                DataContext = dockable
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var result = new DockableControlLeakResult(
                new WeakReference(control),
                dockable,
                context.Factory);

            CleanupWindow(window);
            return result;
        });
    }

    private sealed record DockableControlLeakResult(
        WeakReference ControlRef,
        IDockable DockableKeepAlive,
        IFactory FactoryKeepAlive);

    private sealed record DockableControlSwapLeakResult(
        WeakReference ControlRef,
        IDockable DockableAKeepAlive,
        IDockable DockableBKeepAlive,
        IFactory FactoryKeepAlive);

    private sealed record DockableControlDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDockable DockableKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);
}
