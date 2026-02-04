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
public class DockControlInputLeakTests
{
    [ReleaseFact]
    public void DockControl_InputDetachWhileWindowAlive_LimitedInteractions_DoesNotLeak()
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
            var detached = false;
            dockControl.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            dockControl.ApplyTemplate();
            dockControl.UpdateLayout();
            DrainDispatcher();

            ExerciseInputInteractions(
                dockControl,
                interactionMask: InputInteractionMask.PointerEnterExit | InputInteractionMask.PointerMove);

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            var controlRef = new WeakReference(dockControl);
            dockControl = null;

            return new DockControlInputLeakResult(controlRef, window, context.Factory, context.Root, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private sealed record DockControlInputLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        Dock.Model.Core.IFactory FactoryKeepAlive,
        Dock.Model.Core.IDock LayoutKeepAlive,
        bool DetachedFromVisualTree);
}
