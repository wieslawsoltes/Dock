using System;
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
        var panel = CreatePanel();
        panel.ExpansionDelay = TimeSpan.FromMilliseconds(40);

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 164d);

        panel.Children.RemoveAt(panel.Children.Count - 1);
        Layout(panel, 500, 32);
        AssertTabWidth(panel, 164d);

        await Task.Delay(90);
        Dispatcher.UIThread.RunJobs();

        Layout(panel, 500, 32);
        AssertTabWidth(panel, 220d);
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

    private static DelayedUniformTabPanel CreatePanel()
    {
        var panel = new DelayedUniformTabPanel
        {
            MaxTabWidth = 220d,
            MinTabWidth = 80d,
            ItemSpacing = 4d,
            ExpansionDelay = TimeSpan.FromSeconds(1)
        };

        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
        panel.Children.Add(new Border());
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
