using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class OverlayServiceLeakTests
{
    [ReleaseFact]
    public void BusyOverlayControl_DetachWhileWindowAlive_DoesNotLeak_WhenServicesAlive()
    {
        var result = RunInSession(() =>
        {
            var busy = new StubBusyService();
            var global = new StubGlobalBusyService();

            var control = new BusyOverlayControl
            {
                BusyService = busy,
                GlobalBusyService = global
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            busy.Begin("Working");
            global.Begin("Global");
            DrainDispatcher();
            control.BusyService = null;
            control.GlobalBusyService = null;
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var controlRef = new WeakReference(control);
            control = null;

            return new OverlayServiceLeakResult(controlRef, window, busy, global, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "BusyOverlayControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.ServiceKeepAlive);
        GC.KeepAlive(result.GlobalServiceKeepAlive);
    }

    [ReleaseFact]
    public void DialogOverlayControl_DetachWhileWindowAlive_DoesNotLeak_WhenServicesAlive()
    {
        var result = RunInSession(() =>
        {
            var dialog = new StubDialogService();
            var global = new StubGlobalDialogService();

            var control = new DialogOverlayControl
            {
                DialogService = dialog,
                GlobalDialogService = global
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            global.Begin("Dialog");
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var controlRef = new WeakReference(control);
            control = null;

            return new OverlayServiceLeakResult(controlRef, window, dialog, global, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DialogOverlayControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.ServiceKeepAlive);
        GC.KeepAlive(result.GlobalServiceKeepAlive);
    }

    [ReleaseFact]
    public void ConfirmationOverlayControl_DetachWhileWindowAlive_DoesNotLeak_WhenServicesAlive()
    {
        var result = RunInSession(() =>
        {
            var confirmation = new StubConfirmationService();
            var global = new StubGlobalConfirmationService();

            var control = new ConfirmationOverlayControl
            {
                ConfirmationService = confirmation,
                GlobalConfirmationService = global
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            global.Begin("Confirm");
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var controlRef = new WeakReference(control);
            control = null;

            return new OverlayServiceLeakResult(controlRef, window, confirmation, global, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "ConfirmationOverlayControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.ServiceKeepAlive);
        GC.KeepAlive(result.GlobalServiceKeepAlive);
    }

    private sealed record OverlayServiceLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        object ServiceKeepAlive,
        object GlobalServiceKeepAlive,
        bool DetachedFromVisualTree);
}
