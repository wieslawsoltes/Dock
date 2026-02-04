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
public class ToolTabStripLeakTests
{
    [ReleaseFact]
    public void ToolTabStrip_ItemContainer_DoesNotLeak_TabStrip_WhenItemAlive()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var tabStrip = new ToolTabStrip
            {
                ItemsSource = null
            };

            var tabItem = new ToolTabStripItem { DataContext = context.Tool };
            tabStrip.Items.Add(tabItem);

            var window = new Window { Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            tabStrip.Items.Remove(tabItem);
            DrainDispatcher();

            var result = new ToolTabStripLeakResult(
                new WeakReference(tabStrip),
                tabItem,
                context.Tool);

            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.TabStripRef);
        GC.KeepAlive(result.ItemKeepAlive);
        GC.KeepAlive(result.DockableKeepAlive);
    }

    [ReleaseFact]
    public void ToolTabStripItem_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var tabStrip = new ToolTabStrip
            {
                DataContext = context.ToolDock
            };

            var tabItem = new ToolTabStripItem
            {
                DataContext = context.Tool
            };
            tabStrip.Items.Add(tabItem);

            var window = new Window { Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var startPoint = new Point(tabItem.Bounds.Width / 2, tabItem.Bounds.Height / 2);
            var dragPoint = new Point(
                startPoint.X + DockSettings.MinimumHorizontalDragDistance + 20,
                startPoint.Y + DockSettings.MinimumVerticalDragDistance + 20);

            RaisePointerPressed(tabItem, startPoint, MouseButton.Left);
            RaisePointerMoved(tabItem, dragPoint, leftPressed: true);
            DrainDispatcher();

            tabStrip.Items.Remove(tabItem);
            DrainDispatcher();
            ClearInputState(window);

            var itemRef = new WeakReference(tabItem);
            tabItem = null;

            return new ToolTabStripItemLeakResult(itemRef, window, tabStrip, context.Tool, context.ToolDock);
        });

        AssertCollected(result.ItemRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.TabStripKeepAlive);
        GC.KeepAlive(result.ToolKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
    }

    [ReleaseFact]
    public void ToolTabStrip_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var tabStrip = new ToolTabStrip
            {
                ItemsSource = context.ToolDock.VisibleDockables
            };

            var window = new Window { Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();

            var tabStripRef = new WeakReference(tabStrip);
            tabStrip = null;

            return new ToolTabStripDetachLeakResult(tabStripRef, window, context.ToolDock);
        });

        AssertCollected(result.TabStripRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
    }

    private sealed record ToolTabStripLeakResult(
        WeakReference TabStripRef,
        ToolTabStripItem ItemKeepAlive,
        object DockableKeepAlive);

    private sealed record ToolTabStripItemLeakResult(
        WeakReference ItemRef,
        Window WindowKeepAlive,
        ToolTabStrip TabStripKeepAlive,
        Dock.Model.Core.IDockable ToolKeepAlive,
        Dock.Model.Core.IDock DockKeepAlive);

    private sealed record ToolTabStripDetachLeakResult(
        WeakReference TabStripRef,
        Window WindowKeepAlive,
        Dock.Model.Core.IDock DockKeepAlive);
}
