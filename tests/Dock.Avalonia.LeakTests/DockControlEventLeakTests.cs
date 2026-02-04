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
public class DockControlEventLeakTests
{
    private sealed record FactoryEventLeakResult(WeakReference ControlRef, object FactoryKeepAlive);

    [ReleaseFact]
    public void DockControl_FactoryEventSubscription_DoesNotLeak_WhenFactoryAlive()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
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
            context.Factory.SetActiveDockable(context.Document);
            DrainDispatcher();

            var result = new FactoryEventLeakResult(
                new WeakReference(dockControl),
                context.Factory);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            return result;
        });

        AssertCollected(result.ControlRef);
        System.GC.KeepAlive(result.FactoryKeepAlive);
    }
}
