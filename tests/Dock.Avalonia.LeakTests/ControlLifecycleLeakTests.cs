using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ControlLifecycleLeakTests
{
    [ReleaseFact]
    public void DockControl_AttachDetach_DoesNotLeak()
    {
        var factory = new Factory();

        var (controlRef, layoutRef) = RunInSession(() =>
        {
            var layout = factory.CreateLayout();
            layout.Factory = factory;

            var dockControl = new DockControl
            {
                Factory = factory,
                Layout = layout
            };

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var result = (new WeakReference(dockControl), new WeakReference(layout));
            CleanupWindow(window);
            return result;
        });

        AssertCollected(controlRef, layoutRef);
        System.GC.KeepAlive(factory);
    }

    [ReleaseFact]
    public void DockableControl_Detach_DoesNotLeak_Dockable()
    {
        var factory = new Factory();

        var (controlRef, dockableRef) = RunInSession(() =>
        {
            var dockable = factory.CreateDocument();
            dockable.Factory = factory;

            var control = new DockableControl
            {
                TrackingMode = TrackingMode.Visible,
                DataContext = dockable
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var result = (new WeakReference(control), new WeakReference(dockable));
            CleanupWindow(window);
            return result;
        });

        AssertCollected(controlRef, dockableRef);
        System.GC.KeepAlive(factory);
    }

    [ReleaseFact]
    public void OverlayHost_Detach_DoesNotLeak_StaticRegistry()
    {
        var hostRef = RunInSession(() =>
        {
            var host = new OverlayHost
            {
                Content = new Border()
            };

            var window = new Window { Content = host };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            var reference = new WeakReference(host);
            CleanupWindow(window);
            return reference;
        });

        AssertCollected(hostRef);
    }

    [ReleaseFact]
    public void AllControls_AttachDetach_DoesNotLeak()
    {
        foreach (var testCase in LeakTestCases.ControlCases)
        {
            var result = LeakTestCaseRunner.RunControlCase(testCase, exerciseInput: false);
            LeakTestCaseRunner.AssertCollectedForCase(result, keepAlive: true);
        }
    }
}
