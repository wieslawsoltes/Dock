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

    private sealed record ToolTabStripLeakResult(
        WeakReference TabStripRef,
        ToolTabStripItem ItemKeepAlive,
        object DockableKeepAlive);
}
