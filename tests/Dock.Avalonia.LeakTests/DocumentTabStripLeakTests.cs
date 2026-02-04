using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DocumentTabStripLeakTests
{
    [ReleaseFact]
    public void DocumentTabStrip_DragHelper_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.DocumentDock.CanCloseLastDockable = false;

            var tabStrip = new DocumentTabStrip
            {
                DataContext = context.DocumentDock,
                ItemsSource = context.DocumentDock.VisibleDockables,
                EnableWindowDrag = true
            };

            var window = new Window { Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var startPoint = new Point(tabStrip.Bounds.Width / 2, tabStrip.Bounds.Height / 2);
            var dragPoint = new Point(
                startPoint.X + DockSettings.MinimumHorizontalDragDistance + 20,
                startPoint.Y + DockSettings.MinimumVerticalDragDistance + 20);

            RaisePointerPressed(tabStrip, startPoint, MouseButton.Left);
            RaisePointerMoved(tabStrip, dragPoint, leftPressed: true);
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();
            ClearInputState(window);

            tabStrip.ItemsSource = null;
            tabStrip.DataContext = null;

            var tabStripRef = new WeakReference(tabStrip);
            tabStrip = null;

            return new DocumentTabStripLeakResult(tabStripRef, window);
        });

        AssertCollected(result.TabStripRef);
        GC.KeepAlive(result.WindowKeepAlive);
    }

    private sealed record DocumentTabStripLeakResult(
        WeakReference TabStripRef,
        Window WindowKeepAlive);
}
