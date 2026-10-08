using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockSourcePreviewTests
{
    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(2, false)]
    [InlineData(0, true)]
    [InlineData(2, true)]
    public void Drag_vacates_source_without_mutating_models_and_cancel_restores(int sourceIndex, bool wholePane)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        if (wholePane) factory.AddDockable(panes[sourceIndex], factory.CreateTool());
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var originals = panes.Select(p => Bounds(factory, p, host)).ToArray();
            var shares = panes.Select(p => (p.Proportion, p.CollapsedProportion)).ToArray();
            var row = (IDock)panes[0].Owner!;
            var children = row.VisibleDockables!.ToArray();
            var lifecycleEvents = 0;
            factory.DockableRemoved += (_, _) => lifecycleEvents++;
            factory.DockableAdded += (_, _) => lifecycleEvents++;
            factory.DockableClosed += (_, _) => lifecycleEvents++;
            factory.ActiveDockableChanged += (_, _) => lifecycleEvents++;
            var source = wholePane ? (IDockable)panes[sourceIndex] : panes[sourceIndex].ActiveDockable!;
            var drag = host.GetVisualDescendants().OfType<Control>().First(c => ReferenceEquals(c.DataContext, source)
               );
            var state = (DockControlState)host.DockControlState;
            state.StartDrag(drag, default, new Point(500, 300), host);
            window.UpdateLayout();
            var remaining = (sourceIndex + 1) % 3;
            Assert.True(Bounds(factory, panes[remaining], host).Width > originals[remaining].Width + 50,
                "The remaining panes must reflow while dragging, not only after release.");
            Assert.Equal(0, lifecycleEvents);
            Assert.Equal(children, row.VisibleDockables);
            for (var i = 0; i < panes.Length; i++)
            {
                Assert.Equal(shares[i], (panes[i].Proportion, panes[i].CollapsedProportion));
                Assert.Same(row, panes[i].Owner);
            }
            if (wholePane)
                host.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape });
            else
                state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
            window.UpdateLayout();
            for (var i = 0; i < panes.Length; i++) DockDropPreviewTests.AssertBounds(originals[i], Bounds(factory, panes[i], host));
            Assert.False(state.HasActiveDrag);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, DockOperation.Left)]
    [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Top)]
    [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Left)]
    [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Top)]
    [InlineData(true, DockOperation.Bottom)]
    public void Reflowed_target_and_drop_share_the_same_geometry(bool global, DockOperation operation)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var source = panes[2].ActiveDockable!;
            using var scope = DockDragPreviewLayout.TryCreate(source, host);
            Assert.NotNull(scope);
            var beforeSplit = Bounds(factory, panes[0], host);
            Assert.True(beforeSplit.Width > 500);
            var target = global ? panes[0].Owner! : panes[0];
            var proportion = global ? 0.33 : double.NaN;
            var predicted = new DockDropPreviewService().GetBounds(source, target, operation, host, proportion)!.Value;
            if (!global) Assert.True(beforeSplit.Contains(predicted.Center), "The highlight must lie within the target the user sees.");
            scope!.Dispose();
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
            if (global) DockSplitProportion.Apply((IDock)source.Owner!, proportion);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            DockDropPreviewTests.AssertBounds(predicted, Bounds(factory, source.Owner!, host));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Nested_source_removal_reflows_like_real_removal_and_preserves_constraints(bool vertical, bool constrained)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var row = (IProportionalDock)panes[0].Owner!;
        row.Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal;
        var group = factory.CreateProportionalDock();
        group.Orientation = vertical ? Orientation.Horizontal : Orientation.Vertical;
        group.Proportion = group.CollapsedProportion = 0.3;
        group.VisibleDockables = factory.CreateList<IDockable>();
        row.VisibleDockables![4] = group;
        factory.AddDockable(group, panes[2]);
        factory.AddDockable(group, factory.CreateProportionalDockSplitter());
        var sibling = factory.CreateToolDock();
        sibling.VisibleDockables = factory.CreateList<IDockable>(factory.CreateTool());
        sibling.ActiveDockable = sibling.VisibleDockables[0];
        sibling.Proportion = sibling.CollapsedProportion = 0.28;
        factory.AddDockable(group, sibling);
        panes[2].Proportion = panes[2].CollapsedProportion = 0.72;
        if (constrained)
        {
            panes[0].MinWidth = 200;
            panes[0].MinHeight = 180;
            sibling.MaxWidth = 350;
            sibling.MaxHeight = 400;
        }
        factory.InitLayout(root);
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var source = panes[2].ActiveDockable!;
            using var scope = DockDragPreviewLayout.TryCreate(source, host);
            Assert.NotNull(scope);
            var models = new IDockable[] { panes[0], panes[1], sibling };
            var preview = models.Select(m => Bounds(factory, m, host)).ToArray();
            scope!.Dispose();
            factory.RemoveDockable(source, true);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            for (var i = 0; i < models.Length; i++)
                DockDropPreviewTests.AssertBounds(preview[i], Bounds(factory, models[i], host));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(DragAction.Copy)]
    [InlineData(DragAction.Link)]
    public void Non_move_action_restores_the_source_layout(DragAction action)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var original = Bounds(factory, panes[0], host);
            var drag = host.GetVisualDescendants().OfType<Control>().First(c => ReferenceEquals(c.DataContext, panes[2]));
            var state = (DockControlState)host.DockControlState;
            state.StartDrag(drag, default, new Point(500, 300), host);
            Assert.True(Bounds(factory, panes[0], host).Width > original.Width);
            state.Process(new Point(500, 300), default, EventType.Moved, action, host, new List<IDockControl> { host });
            window.UpdateLayout();
            DockDropPreviewTests.AssertBounds(original, Bounds(factory, panes[0], host));
            state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Abandoned_drag_restores_source_layout(int ending)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        panes[2].CanFloat = false;
        panes[2].ActiveDockable!.CanFloat = false;
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var original = Bounds(factory, panes[0], host);
            var remainingControl = (Control)factory.VisibleDockableControls[panes[0]];
            var sourceControl = (Control)factory.VisibleDockableControls[panes[2]];
            var sourcePresenter = sourceControl.GetVisualAncestors().OfType<Control>()
                .First(c => c.GetVisualParent() is ProportionalStackPanel);
            var drag = host.GetVisualDescendants().OfType<Control>().First(c => ReferenceEquals(c.DataContext, panes[2]));
            var state = (DockControlState)host.DockControlState;
            state.StartDrag(drag, default, new Point(500, 300), host);
            Assert.True(Bounds(factory, panes[0], host).Width > original.Width);
            if (ending == 0)
            {
                host.IsDockingEnabled = false;
                state.Process(default, default, EventType.Moved, DragAction.Move, host, new List<IDockControl> { host });
            }
            else if (ending == 1)
                state.Process(default, default, EventType.Released, DragAction.Move, host, new List<IDockControl> { host });
            else
            {
                Assert.False(sourcePresenter.IsVisible);
                window.Content = null;
                Assert.True(sourcePresenter.IsVisible);
                state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
                return;
            }
            window.UpdateLayout();
            DockDropPreviewTests.AssertBounds(original, new Rect(remainingControl.TranslatePoint(default, host)!.Value, remainingControl.Bounds.Size));
            state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
        }
        finally { window.Close(); }
    }

    private static Rect Bounds(IFactory factory, IDockable model, Control relativeTo)
    {
        var control = (Control)factory.VisibleDockableControls[model];
        return new Rect(control.TranslatePoint(default, relativeTo)!.Value, control.Bounds.Size);
    }
}
