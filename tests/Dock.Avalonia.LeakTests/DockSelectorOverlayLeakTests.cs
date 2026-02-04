using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockSelectorOverlayLeakTests
{
    [ReleaseFact]
    public void DockSelectorOverlay_DetachWhileWindowAlive_DoesNotLeak_WhenDockableAlive()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var item = new DockSelectorItem(context.Document, 1, true, false, false, false, false);

            var control = new DockSelectorOverlay
            {
                Items = new[] { item },
                SelectedItem = item,
                Mode = DockSelectorMode.Documents,
                IsOpen = true
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

            control.IsOpen = false;
            control.IsOpen = true;
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockSelectorOverlayLeakResult(controlRef, window, context.Document, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockSelectorOverlay did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockableKeepAlive);
    }

    private sealed record DockSelectorOverlayLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDockable DockableKeepAlive,
        bool DetachedFromVisualTree);
}
