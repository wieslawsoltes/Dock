using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockScrollableSourcePreviewTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Drag_with_unbounded_cross_axis_supports_reflow_cancellation_and_drop(bool vertical, bool scrollViewer)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var row = (IProportionalDock)panes[0].Owner!;
        row.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;
        var host = new DockControl { Layout = root, MinWidth = 300, MinHeight = 300 };
        Control container;
        if (scrollViewer)
        {
            container = new ScrollViewer
            {
                Content = host,
                HorizontalScrollBarVisibility = vertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = vertical ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto
            };
        }
        else
        {
            var stack = new StackPanel
            {
                Orientation = vertical ? global::Avalonia.Layout.Orientation.Horizontal : global::Avalonia.Layout.Orientation.Vertical
            };
            stack.Children.Add(host);
            container = stack;
        }
        var window = new Window { Width = 1000, Height = 600, Content = container };
        var state = (DockControlState)host.DockControlState;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var originalBounds = panes.Select(p => Bounds(factory, p, host)).ToArray();
            var originalShares = panes.Select(p => (p.Proportion, p.CollapsedProportion)).ToArray();
            var originalChildren = row.VisibleDockables!.ToArray();
            var originalHostBounds = host.Bounds;
            var source = panes[2].ActiveDockable!;
            var drag = host.GetVisualDescendants().OfType<Control>().First(c => ReferenceEquals(c.DataContext, source));
            for (var attempt = 0; attempt < 2; attempt++)
            {
                state.StartDrag(drag, default, new Point(150, 150), host);
                window.UpdateLayout();
                var reflowed = Bounds(factory, panes[0], host);
                Assert.True(vertical ? reflowed.Height > originalBounds[0].Height + 50
                    : reflowed.Width > originalBounds[0].Width + 50);
                DockDropPreviewTests.AssertBounds(originalHostBounds, host.Bounds);
                Assert.Equal(originalChildren, row.VisibleDockables);
                Assert.Equal(originalShares, panes.Select(p => (p.Proportion, p.CollapsedProportion)));

                state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
                window.UpdateLayout();
                Assert.False(state.HasActiveDrag);
                for (var i = 0; i < panes.Length; i++)
                {
                    DockDropPreviewTests.AssertBounds(originalBounds[i], Bounds(factory, panes[i], host));
                    Assert.Same(row, panes[i].Owner);
                }
                Assert.Equal(originalShares, panes.Select(p => (p.Proportion, p.CollapsedProportion)));
            }

            using var preview = DockDragPreviewLayout.TryCreate(source, host);
            Assert.NotNull(preview);
            var operation = vertical ? DockOperation.Bottom : DockOperation.Right;
            var predicted = new DockDropPreviewService().GetBounds(source, panes[0], operation, host, double.NaN);
            Assert.NotNull(predicted);
            preview!.Dispose();
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[0], DragAction.Move, operation, true));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            DockDropPreviewTests.AssertBounds(predicted!.Value, Bounds(factory, source.Owner!, host));
        }
        finally
        {
            state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
            window.Close();
        }
    }

    private static Rect Bounds(IFactory factory, IDockable model, Control relativeTo)
    {
        var control = (Control)factory.VisibleDockableControls[model];
        return new Rect(control.TranslatePoint(default, relativeTo)!.Value, control.Bounds.Size);
    }
}
