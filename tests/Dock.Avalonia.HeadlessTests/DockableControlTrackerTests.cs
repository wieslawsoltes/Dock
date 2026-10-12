using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
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

    [AvaloniaFact]
    public void Tracker_Requires_The_Native_Control_Contract()
    {
        Assert.Throws<ArgumentNullException>(() => new DockableControlTracker(null!, TrackingMode.Visible));
        Assert.Throws<ArgumentException>(() => new DockableControlTracker(new Grid(), TrackingMode.Visible));
    }

    private static void Update(Window window) { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    private sealed class TrackingGrid : Grid, IDockableControl { }
}
