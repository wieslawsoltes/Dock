using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockSizingTests
{
    [AvaloniaTheory]
    [InlineData(Alignment.Left, PinnedDockDisplayMode.Overlay)]
    [InlineData(Alignment.Right, PinnedDockDisplayMode.Overlay)]
    [InlineData(Alignment.Top, PinnedDockDisplayMode.Overlay)]
    [InlineData(Alignment.Bottom, PinnedDockDisplayMode.Overlay)]
    [InlineData(Alignment.Left, PinnedDockDisplayMode.Inline)]
    [InlineData(Alignment.Right, PinnedDockDisplayMode.Inline)]
    [InlineData(Alignment.Top, PinnedDockDisplayMode.Inline)]
    [InlineData(Alignment.Bottom, PinnedDockDisplayMode.Inline)]
    public void Configured_Minima_Apply_To_Layout_And_Inline_Resizing(Alignment alignment, PinnedDockDisplayMode mode)
    {
        bool previous = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = false;
        Window? window = null;
        try
        {
            (PinnedDockControl control, Tool tool) = Create(alignment, mode);
            Assert.Equal(50, control.MinimumPinnedWidth);
            Assert.Equal(50, control.MinimumPinnedHeight);
            control.MinimumPinnedWidth = 240;
            control.MinimumPinnedHeight = 180;
            window = CreateWindow(control, alignment, mode);
            window.Show(); window.UpdateLayout();
            ContentControl content = control.GetVisualDescendants().OfType<ContentControl>().Single(child => child.Name == "PART_PinnedDock");
            bool horizontal = alignment is Alignment.Left or Alignment.Right;
            Assert.Equal(horizontal ? 240 : 180, horizontal ? content.Bounds.Width : content.Bounds.Height, 1);
            control.MinimumPinnedWidth = 310;
            control.MinimumPinnedHeight = 270;
            window.UpdateLayout();
            Assert.Equal(horizontal ? 310 : 270, horizontal ? content.Bounds.Width : content.Bounds.Height, 1);
            if (mode == PinnedDockDisplayMode.Inline)
            {
                double delta = alignment is Alignment.Right or Alignment.Bottom ? 1000 : -1000;
                Assert.True(control.TryResizeInlinePinnedDock(horizontal ? new Vector(delta, 0) : new Vector(0, delta)));
                window.UpdateLayout();
                tool.GetPinnedBounds(out _, out _, out double width, out double height);
                Assert.Equal(horizontal ? 310 : 270, horizontal ? width : height, 1);
            }
        }
        finally { window?.Close(); DockSettings.UsePinnedDockWindow = previous; }
    }

    [AvaloniaTheory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-1)]
    public void Minima_Reject_Invalid_Values(double value)
    {
        PinnedDockControl control = new();
        Assert.Throws<ArgumentException>(() => control.MinimumPinnedWidth = value);
        Assert.Throws<ArgumentException>(() => control.MinimumPinnedHeight = value);
        control.MinimumPinnedWidth = control.MinimumPinnedHeight = 0;
        Assert.Equal(0, control.MinimumPinnedWidth);
        Assert.Equal(0, control.MinimumPinnedHeight);
    }

    [AvaloniaFact]
    public void Custom_Thumb_Template_Part_Is_Positioned_Without_The_GridSplitter_Resize_Handlers()
    {
        bool previous = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = false;
        Window? window = null;
        try
        {
            (PinnedDockControl control, Tool tool) = Create(Alignment.Right, PinnedDockDisplayMode.Inline);
            Thumb thumb = new() { Name = "PART_PinnedDockSplitter", Width = 6 };
            control.Template = new FuncControlTemplate<PinnedDockControl>((_, scope) =>
            {
                Grid grid = new() { Name = "PART_PinnedDockGrid" };
                ContentControl content = new() { Name = "PART_PinnedDock", Content = "Preview" };
                scope.Register(grid.Name, grid); scope.Register(content.Name, content); scope.Register(thumb.Name!, thumb);
                grid.Children.Add(content); grid.Children.Add(thumb);
                return grid;
            });
            window = CreateWindow(control, Alignment.Right, PinnedDockDisplayMode.Inline);
            window.Show(); window.UpdateLayout();
            Assert.Equal(0, Grid.GetColumn(thumb));
            Assert.Equal(6, thumb.Bounds.Width);
            thumb.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragStartedEvent });
            thumb.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragDeltaEvent, Vector = new Vector(35, 0) });
            thumb.RaiseEvent(new VectorEventArgs { RoutedEvent = Thumb.DragCompletedEvent });
            tool.GetPinnedBounds(out _, out _, out double width, out _);
            Assert.Equal(100, width);
            tool.SetPinnedBounds(0, 0, 260, 100);
            control.InvalidateMeasure(); window.UpdateLayout();
            ContentControl content = control.GetVisualDescendants().OfType<ContentControl>().Single(child => child.Name == "PART_PinnedDock");
            Assert.Equal(260, content.Bounds.Width, 1);
        }
        finally { window?.Close(); DockSettings.UsePinnedDockWindow = previous; }
    }

    private static (PinnedDockControl control, Tool tool) Create(Alignment alignment, PinnedDockDisplayMode mode)
    {
        Factory factory = new();
        Tool tool = new();
        tool.SetPinnedBounds(0, 0, 100, 100);
        RootDock root = new()
        {
            Factory = factory, PinnedDockDisplayMode = mode,
            PinnedDock = new ToolDock
            {
                Alignment = alignment, IsEmpty = false,
                VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool
            }
        };
        return (new PinnedDockControl { DataContext = root }, tool);
    }

    private static Window CreateWindow(PinnedDockControl control, Alignment alignment, PinnedDockDisplayMode mode) => new()
    {
        Width = 800, Height = 600,
        Content = new PinnedDockHostPanel
        {
            DataContext = control.DataContext, PinnedDockAlignment = alignment, PinnedDockDisplayMode = mode,
            Children = { new Border(), control }
        }
    };
}
