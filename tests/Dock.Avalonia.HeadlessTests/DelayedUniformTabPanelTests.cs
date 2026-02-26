using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DelayedUniformTabPanelTests
{
    [AvaloniaFact]
    public void UsesUniformMaxWidthWhenSpaceIsAvailable()
    {
        var panel = CreatePanel();

        Layout(panel, 1000, 32);

        AssertTabWidth(panel, 220d);
    }

    [AvaloniaFact]
    public void ShrinksUniformWidthImmediatelyWhenConstrained()
    {
        var panel = CreatePanel();
        Layout(panel, 1000, 32);

        Layout(panel, 500, 32);

        AssertTabWidth(panel, 164d);
    }

    [AvaloniaFact]
    public async Task ExpandsAfterConfiguredDelayWhenSpaceReturns()
    {
        var panel = CreatePanel(4);
        panel.ExpansionDelay = TimeSpan.FromMilliseconds(40);

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        await Task.Delay(90);
        Dispatcher.UIThread.RunJobs();

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 164d);
    }

    [AvaloniaFact]
    public async Task ClosingMultipleTabsRestartsDelay_EvenWhenTargetWidthIsUnchanged()
    {
        var panel = CreatePanel(6);
        panel.ExpansionDelay = TimeSpan.FromMilliseconds(150);

        Layout(panel, 1000, 32);
        var compactWidth = panel.Children[0].Bounds.Width;
        AssertTabWidth(panel, compactWidth);

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 1000, 32);
        AssertTabWidth(panel, compactWidth);

        // Block the UI thread briefly so the pending expansion timer cannot tick before
        // the second close is applied. This keeps the restart-delay assertion deterministic
        // under CI scheduling jitter.
        Thread.Sleep(TimeSpan.FromMilliseconds(45));

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 1000, 32);
        AssertTabWidth(panel, compactWidth);

        var elapsedSinceSecondClose = await WaitForTabWidthAsync(panel, 220d, TimeSpan.FromMilliseconds(800));
        Assert.True(
            elapsedSinceSecondClose >= panel.ExpansionDelay - TimeSpan.FromMilliseconds(20),
            $"Expansion happened too early after second close. Elapsed={elapsedSinceSecondClose.TotalMilliseconds:F0}ms Delay={panel.ExpansionDelay.TotalMilliseconds:F0}ms");
    }

    private static async Task<TimeSpan> WaitForTabWidthAsync(DelayedUniformTabPanel panel, double expectedWidth, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            Dispatcher.UIThread.RunJobs();
            Layout(panel, 1000, 32);

            if (panel.Children.Count > 0 &&
                Math.Abs(panel.Children[0].Bounds.Width - expectedWidth) <= 0.1)
            {
                return stopwatch.Elapsed;
            }

            await Task.Delay(5);
        }

        AssertTabWidth(panel, expectedWidth);
        return stopwatch.Elapsed;
    }

    [AvaloniaFact]
    public async Task CloseBurstDefersExpansionFromRelayoutUntilDelay()
    {
        var panel = CreatePanel(4);
        panel.ExpansionDelay = TimeSpan.FromMilliseconds(60);

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        // Simulate immediate relayout width increase while close-burst debounce is active.
        Layout(panel, 600, 32);
        AssertTabWidth(panel, 122d);

        await Task.Delay(80);
        Dispatcher.UIThread.RunJobs();
        Layout(panel, 600, 32);
        var expandedWidth = panel.Children[0].Bounds.Width;
        AssertTabWidth(panel, expandedWidth);
        Assert.InRange(expandedWidth, 197d, 199d);
    }

    [AvaloniaFact]
    public void ResizesImmediatelyWhenOnlyViewportChanges()
    {
        var panel = CreatePanel();
        panel.ExpansionDelay = TimeSpan.FromSeconds(10);

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 164d);

        Layout(panel, 1000, 32);
        AssertTabWidth(panel, 220d);
    }

    [AvaloniaFact]
    public async Task DefersViewportExpansionWhileCloseDebounceIsPending()
    {
        var panel = CreatePanel(4);
        panel.ExpansionDelay = TimeSpan.FromMilliseconds(60);

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 500, 32);
        AssertTabWidth(panel, 122d);

        Layout(panel, 1000, 32);
        AssertTabWidth(panel, 122d);

        var elapsedSinceClose = await WaitForTabWidthAsync(panel, 220d, TimeSpan.FromMilliseconds(800));
        Assert.True(
            elapsedSinceClose >= panel.ExpansionDelay - TimeSpan.FromMilliseconds(20),
            $"Expansion happened too early. Elapsed={elapsedSinceClose.TotalMilliseconds:F0}ms Delay={panel.ExpansionDelay.TotalMilliseconds:F0}ms");
    }

    private static DelayedUniformTabPanel CreatePanel(int count = 3)
    {
        var panel = new DelayedUniformTabPanel
        {
            MaxTabWidth = 220d,
            MinTabWidth = 80d,
            ItemSpacing = 4d,
            ExpansionDelay = TimeSpan.FromSeconds(1)
        };

        for (var i = 0; i < count; i++)
        {
            panel.Children.Add(new Border());
        }
        return panel;
    }

    private static void Layout(DelayedUniformTabPanel panel, double width, double height)
    {
        panel.Measure(new Size(width, height));
        panel.Arrange(new Rect(0, 0, width, height));
    }

    private static void AssertTabWidth(DelayedUniformTabPanel panel, double expectedWidth)
    {
        foreach (var child in panel.Children)
        {
            Assert.InRange(child.Bounds.Width, expectedWidth - 0.1, expectedWidth + 0.1);
        }
    }
}
