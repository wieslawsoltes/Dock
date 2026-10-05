using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public sealed class DockEqualSizingTests
{
    [AvaloniaTheory]
    [InlineData(false, DockOperation.Left)] [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Top)] [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Left)] [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Top)] [InlineData(true, DockOperation.Bottom)]
    public void Equal_sizing_preview_matches_release_for_local_and_outer_edges(bool global, DockOperation operation)
    {
        var factory = new EqualFactory();
        var root = factory.CreateRootDock(); root.IsCollapsable = false;
        var row = factory.CreateProportionalDock(); row.Orientation = Orientation.Horizontal;
        var panes = Enumerable.Range(0, 3).Select(i =>
        {
            var pane = factory.CreateToolDock();
            var tool = factory.CreateTool(); pane.VisibleDockables = factory.CreateList<IDockable>(tool); pane.ActiveDockable = tool;
            pane.Proportion = pane.CollapsedProportion = 1D / 3;
            return pane;
        }).ToArray();
        row.VisibleDockables = factory.CreateList<IDockable>(panes[0], factory.CreateProportionalDockSplitter(), panes[1], factory.CreateProportionalDockSplitter(), panes[2]);
        root.VisibleDockables = factory.CreateList<IDockable>(row); factory.InitLayout(root);
        var sourceRoot = factory.CreateRootDock(); var sourcePane = factory.CreateToolDock(); var source = factory.CreateTool();
        sourcePane.VisibleDockables = factory.CreateList<IDockable>(source); sourcePane.ActiveDockable = source;
        sourceRoot.VisibleDockables = factory.CreateList<IDockable>(sourcePane); factory.InitLayout(sourceRoot);
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1003, Height = 607, Content = control };
        try
        {
            window.Show(); global::Avalonia.Threading.Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            IDockable target = global ? row : panes[1];
            var share = global ? .33 : double.NaN;
            var preview = new DockDropPreviewService().GetBounds(source, target, operation, control, share)!.Value;
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
            if (global) Assert.True(DockSplitProportion.Apply((IDock)source.Owner!, share));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            Assert.InRange(preview.X - actual.X, -1, 1); Assert.InRange(preview.Y - actual.Y, -1, 1);
            Assert.InRange(preview.Right - actual.Right, -1, 1); Assert.InRange(preview.Bottom - actual.Bottom, -1, 1);
            Assert.All(((IDock)source.Owner!.Owner!).VisibleDockables!.Where(c => c is not ISplitter), c => Assert.True(c.Proportion > 0));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Policy_change_invalidates_cached_preview_without_requiring_a_resize()
    {
        var factory = new EqualFactory(); var root = factory.CreateRootDock();
        var row = factory.CreateProportionalDock(); var target = factory.CreateToolDock(); var sourcePane = factory.CreateToolDock();
        var targetTool = factory.CreateTool(); target.VisibleDockables = factory.CreateList<IDockable>(targetTool); target.ActiveDockable = targetTool;
        var source = factory.CreateTool(); sourcePane.VisibleDockables = factory.CreateList<IDockable>(source, factory.CreateTool()); sourcePane.ActiveDockable = source;
        row.VisibleDockables = factory.CreateList<IDockable>(target, factory.CreateProportionalDockSplitter(), sourcePane);
        root.VisibleDockables = factory.CreateList<IDockable>(row); factory.InitLayout(root);
        var control = new DockControl { Layout = root }; var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show(); window.UpdateLayout();
            var service = new DockDropPreviewService();
            Assert.NotNull(service.GetBounds(source, target, DockOperation.Right, control, double.NaN));
            Assert.True(service.IsCurrent(source, target, DockOperation.Right, control, double.NaN));
            ((IEqualProportionalDock)row).KeepProportionsEqual = false;
            Assert.False(service.IsCurrent(source, target, DockOperation.Right, control, double.NaN));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Insert_remove_and_global_share_respect_the_policy_and_ignore_splitters()
    {
        var factory = new EqualFactory(); var row = factory.CreateProportionalDock(); row.IsCollapsable = false;
        var first = factory.CreateToolDock(); var second = factory.CreateToolDock();
        factory.AddDockable(row, first); factory.AddDockable(row, factory.CreateProportionalDockSplitter()); factory.AddDockable(row, second);
        Assert.Equal(.5, first.Proportion); Assert.Equal(.5, second.Proportion);
        var third = factory.CreateToolDock(); factory.SplitToDock(second, third, DockOperation.Right);
        Assert.Equal(1D / 3, first.Proportion, 8); Assert.Equal(1D / 3, second.Proportion, 8); Assert.Equal(1D / 3, third.Proportion, 8);
        Assert.True(DockSplitProportion.Apply(third, .33)); Assert.Equal(1D / 3, third.Proportion, 8);
        factory.RemoveDockable(third, true);
        Assert.Equal(.5, first.Proportion); Assert.Equal(.5, second.Proportion);
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)] [InlineData(Orientation.Vertical)]
    public void Nested_same_axis_insertions_and_removals_rebalance_ancestors(Orientation orientation)
    {
        var factory = new EqualFactory();
        var outer = factory.CreateProportionalDock(); outer.Orientation = orientation; outer.IsCollapsable = false;
        var inner = factory.CreateProportionalDock(); inner.Orientation = orientation; inner.IsCollapsable = false;
        var first = factory.CreateToolDock(); var second = factory.CreateToolDock(); var third = factory.CreateToolDock();
        factory.AddDockable(inner, first);
        factory.AddDockable(outer, inner);
        factory.AddDockable(outer, factory.CreateProportionalDockSplitter());
        factory.AddDockable(outer, third);
        factory.AddDockable(inner, factory.CreateProportionalDockSplitter());
        factory.AddDockable(inner, second);
        Assert.Equal(2D / 3, inner.Proportion, 8);
        Assert.Equal(1D / 3, third.Proportion, 8);
        Assert.Equal(.5, first.Proportion); Assert.Equal(.5, second.Proportion);
        factory.RemoveDockable(second, true);
        Assert.Equal(.5, inner.Proportion); Assert.Equal(.5, third.Proportion);
        Assert.Equal(1, first.Proportion);
    }

    private sealed class EqualDock : Dock.Model.Avalonia.Controls.ProportionalDock, IEqualProportionalDock
    {
        public bool KeepProportionsEqual { get; set; } = true;
    }
    private sealed class EqualFactory : Dock.Model.Avalonia.Factory
    {
        public override IProportionalDock CreateProportionalDock() => new EqualDock();
        public override IFactory CreatePreviewFactory() => new EqualFactory();
    }
}
