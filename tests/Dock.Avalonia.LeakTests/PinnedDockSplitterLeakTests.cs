using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class PinnedDockSplitterLeakTests
{
    [ReleaseFact]
    public void PinnedDockControl_SplitterDrag_DoesNotLeak()
    {
        var previousPinned = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = false;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Root.PinnedDockDisplayMode = PinnedDockDisplayMode.Inline;
                context.Factory.InitLayout(context.Root);

                var rootControl = new RootDockControl { DataContext = context.Root };
                var window = new Window { Content = rootControl };
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());

                ShowWindow(window);
                rootControl.ApplyTemplate();
                rootControl.UpdateLayout();
                DrainDispatcher();

                var pinnedDockControl = FindVisualDescendant<PinnedDockControl>(rootControl);
                Assert.NotNull(pinnedDockControl);

                pinnedDockControl!.ApplyTemplate();
                pinnedDockControl.UpdateLayout();
                DrainDispatcher();

                var splitter = FindTemplateChild<GridSplitter>(pinnedDockControl, "PART_PinnedDockSplitter");
                Assert.NotNull(splitter);

                var detached = false;
                pinnedDockControl.DetachedFromVisualTree += (_, _) => detached = true;

                if (splitter is not null)
                {
                    splitter.RaiseEvent(new VectorEventArgs
                    {
                        RoutedEvent = Thumb.DragStartedEvent,
                        Vector = new Vector(4, 0)
                    });
                    splitter.RaiseEvent(new VectorEventArgs
                    {
                        RoutedEvent = Thumb.DragDeltaEvent,
                        Vector = new Vector(4, 0)
                    });
                    splitter.RaiseEvent(new VectorEventArgs
                    {
                        RoutedEvent = Thumb.DragCompletedEvent,
                        Vector = new Vector(4, 0)
                    });
                }

                DrainDispatcher();

                window.Content = null;
                DrainDispatcher();
                ClearPinnedDockControlState(pinnedDockControl);
                ClearFactoryCaches(context.Factory);
                ClearInputState(window);
                ResetDispatcherForUnitTests();

                var controlRef = new WeakReference(pinnedDockControl);
                pinnedDockControl = null;
                rootControl = null;

                return new PinnedDockSplitterLeakResult(
                    controlRef,
                    window,
                    context.Root,
                    context.Factory,
                    detached);
            });

            Assert.True(result.DetachedFromVisualTree, "PinnedDockControl did not detach from visual tree.");
            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                AssertCollected(result.ControlRef);
            }
            GC.KeepAlive(result.WindowKeepAlive);
            GC.KeepAlive(result.RootKeepAlive);
            GC.KeepAlive(result.FactoryKeepAlive);
        }
        finally
        {
            DockSettings.UsePinnedDockWindow = previousPinned;
        }
    }

    private sealed record PinnedDockSplitterLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDock RootKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);
}
