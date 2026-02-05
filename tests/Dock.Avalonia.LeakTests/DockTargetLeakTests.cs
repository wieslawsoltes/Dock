using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockTargetLeakTests
{
    [ReleaseFact]
    public void DockTarget_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var control = new DockTarget();
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            control.ShowHorizontalTargets = false;
            control.ShowVerticalTargets = false;
            control.IsGlobalDockAvailable = true;
            control.IsGlobalDockActive = true;
            control.ShowIndicatorsOnly = true;
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockTargetDetachLeakResult(controlRef, window, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockTarget did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    [ReleaseFact]
    public void GlobalDockTarget_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var control = new GlobalDockTarget();
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            control.ShowHorizontalTargets = false;
            control.ShowVerticalTargets = false;
            control.IsGlobalDockAvailable = true;
            control.IsGlobalDockActive = true;
            control.ShowIndicatorsOnly = true;
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockTargetDetachLeakResult(controlRef, window, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "GlobalDockTarget did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    private sealed record DockTargetDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        bool DetachedFromVisualTree);
}
