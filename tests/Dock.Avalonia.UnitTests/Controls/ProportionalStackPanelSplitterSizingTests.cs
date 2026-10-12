using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Dock.Controls.ProportionalStackPanel;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalStackPanelSplitterSizingTests
{
    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Thickness_Changes_Remeasure_The_Splitter_And_Adjacent_Panes(Orientation orientation)
    {
        ProportionalStackPanel panel = new() { Orientation = orientation };
        Border first = new();
        Border second = new();
        ProportionalStackPanelSplitter splitter = new();
        panel.Children.Add(first);
        panel.Children.Add(splitter);
        panel.Children.Add(second);
        Window window = new() { Content = panel, Width = 600, Height = 400 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(4, orientation == Orientation.Horizontal ? splitter.Bounds.Width : splitter.Bounds.Height);
            double before = orientation == Orientation.Horizontal ? first.Bounds.Width : first.Bounds.Height;
            splitter.Thickness = 24;
            window.UpdateLayout();
            Assert.Equal(24, orientation == Orientation.Horizontal ? splitter.Bounds.Width : splitter.Bounds.Height);
            Assert.Equal(before - 10, orientation == Orientation.Horizontal ? first.Bounds.Width : first.Bounds.Height);
            splitter.Thickness = 0;
            window.UpdateLayout();
            Assert.Equal(0, orientation == Orientation.Horizontal ? splitter.Bounds.Width : splitter.Bounds.Height);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Orientation_Changes_Update_The_Same_Splitter_Without_A_Thickness_Change()
    {
        ProportionalStackPanel panel = new() { Orientation = Orientation.Horizontal };
        ProportionalStackPanelSplitter splitter = new() { Thickness = 10 };
        panel.Children.Add(new Border());
        panel.Children.Add(splitter);
        panel.Children.Add(new Border());
        Window window = new() { Content = panel, Width = 600, Height = 400 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(10, splitter.Bounds.Width);
            Assert.Equal(panel.Bounds.Height, splitter.Bounds.Height);
            panel.Orientation = Orientation.Vertical;
            window.UpdateLayout();
            Assert.Equal(10, splitter.Bounds.Height);
            Assert.Equal(panel.Bounds.Width, splitter.Bounds.Width);
            Assert.True(double.IsNaN(splitter.Width));
            panel.Orientation = Orientation.Horizontal;
            window.UpdateLayout();
            Assert.Equal(10, splitter.Bounds.Width);
            Assert.Equal(panel.Bounds.Height, splitter.Bounds.Height);
            Assert.True(double.IsNaN(splitter.Height));
        }
        finally { window.Close(); }
    }
}
