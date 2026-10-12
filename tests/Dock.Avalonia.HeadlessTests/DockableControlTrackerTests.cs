using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockableControlTrackerTests
{
    [AvaloniaTheory]
    [InlineData(TrackingMode.Visible)]
    [InlineData(TrackingMode.Pinned)]
    [InlineData(TrackingMode.Tab)]
    public void Custom_Grid_Tracks_Model_Changes_Attachment_And_Pointers(TrackingMode mode)
    {
        Factory factory = new();
        Tool first = new() { Factory = factory }, second = new() { Factory = factory };
        TrackingGrid grid = new() { DataContext = first, Background = Brushes.White };
        using DockableControlTracker tracker = new(grid, mode);
        Window window = new() { Content = grid, Width = 300, Height = 200 };
        IDictionary<IDockable, IDockableControl> registry = mode switch
        {
            TrackingMode.Pinned => factory.PinnedDockableControls,
            TrackingMode.Tab => factory.TabDockableControls,
            _ => factory.VisibleDockableControls
        };
        try
        {
            Assert.Empty(registry);
            window.Show(); Update(window);
            Assert.Same(grid, registry[first]);
            grid.DataContext = second;
            Assert.False(registry.ContainsKey(first));
            Assert.Same(grid, registry[second]);
            window.MouseMove(new Point(40, 50));
            second.GetPointerPosition(out double x, out double y);
            Assert.Equal(40, x); Assert.Equal(50, y);
            window.Content = null;
            Assert.Empty(registry);
            window.Content = grid; Update(window);
            Assert.Same(grid, registry[second]);
            tracker.Dispose(); tracker.Dispose();
            Assert.Empty(registry);
            grid.DataContext = first;
            Assert.Empty(registry);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(TrackingMode.Visible)]
    [InlineData(TrackingMode.Pinned)]
    [InlineData(TrackingMode.Tab)]
    public void Detaching_A_Previous_Presentation_Preserves_The_Current_Registration(TrackingMode mode)
    {
        Factory factory = new();
        Tool model = new() { Factory = factory };
        TrackingGrid previous = new() { DataContext = model }, current = new() { DataContext = model };
        using DockableControlTracker oldTracker = new(previous, mode), newTracker = new(current, mode);
        Panel host = new() { Children = { previous, current } };
        Window window = new() { Content = host, Width = 300, Height = 200 };
        IDictionary<IDockable, IDockableControl> registry = mode switch
        {
            TrackingMode.Pinned => factory.PinnedDockableControls,
            TrackingMode.Tab => factory.TabDockableControls,
            _ => factory.VisibleDockableControls
        };
        try
        {
            window.Show(); Update(window);
            Assert.Same(current, registry[model]);
            host.Children.Remove(previous);
            Assert.Same(current, registry[model]);
            host.Children.Remove(current);
            Assert.Empty(registry);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Replacing_A_Model_Publishes_The_Current_Tracked_Bounds_Immediately()
    {
        Tool first = new(), second = new();
        TrackingGrid grid = new() { DataContext = first, Width = 120, Height = 80 };
        using DockableControlTracker tracker = new(grid, TrackingMode.Visible);
        Window window = new() { Content = grid, Width = 300, Height = 200 };
        try
        {
            window.Show(); Update(window);
            grid.DataContext = second;
            first.GetVisibleBounds(out double x, out double y, out double width, out double height);
            second.GetVisibleBounds(out double nextX, out double nextY, out double nextWidth, out double nextHeight);
            Assert.Equal((x, y, width, height), (nextX, nextY, nextWidth, nextHeight));
            Assert.Equal(120, nextWidth);
            Assert.Equal(80, nextHeight);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(TrackingMode.Visible)]
    [InlineData(TrackingMode.Pinned)]
    [InlineData(TrackingMode.Tab)]
    public void Child_Offsets_Are_Applied_Once_In_Native_Bounds(TrackingMode mode)
    {
        Tool model = new();
        TrackingGrid grid = new() { DataContext = model, Width = 120, Height = 80 };
        Canvas.SetLeft(grid, 40); Canvas.SetTop(grid, 50);
        Canvas parent = new() { Children = { grid } };
        using DockableControlTracker tracker = new(grid, mode);
        Window window = new() { Content = parent, Width = 400, Height = 300 };
        try
        {
            window.Show(); Update(window);
            AssertBounds();
            Canvas.SetLeft(grid, 65); Canvas.SetTop(grid, 75);
            Update(window);
            AssertBounds();
        }
        finally { window.Close(); }

        void AssertBounds()
        {
            double x, y, width, height;
            if (mode == TrackingMode.Pinned) model.GetPinnedBounds(out x, out y, out width, out height);
            else if (mode == TrackingMode.Tab) model.GetTabBounds(out x, out y, out width, out height);
            else model.GetVisibleBounds(out x, out y, out width, out height);
            Point origin = grid.TranslatePoint(default, (Visual)grid.GetVisualRoot()!)!.Value;
            Assert.Equal(origin.X, x); Assert.Equal(origin.Y, y);
            Assert.Equal(120, width); Assert.Equal(80, height);
        }
    }

    [AvaloniaFact]
    public void Tracker_Requires_The_Native_Control_Contract()
    {
        Assert.Throws<ArgumentNullException>(() => new DockableControlTracker(null!, TrackingMode.Visible));
        Assert.Throws<ArgumentException>(() => new DockableControlTracker(new Grid(), TrackingMode.Visible));
    }

    private static void Update(Window window) { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    private sealed class TrackingGrid : Grid, IDockableControl { }
}
