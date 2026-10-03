using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [AvaloniaTheory]
    [InlineData(false, DockOperation.Left)]
    [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Top)]
    [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Left)]
    [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Top)]
    [InlineData(true, DockOperation.Bottom)]
    public void Local_drop_matches_preview_when_source_pane_collapses(bool keepSourcePane, DockOperation operation)
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        var source = panes[0].ActiveDockable!;
        if (keepSourcePane) factory.AddDockable(panes[0], factory.CreateTool());
        var before = Measure(root);
        var liveEvents = 0;
        factory.DockableRemoved += (_, _) => liveEvents++;
        factory.DockableAdded += (_, _) => liveEvents++;
        var projection = Assert.IsType<DockSplitPreview>(DockSplitPreview.Create(source, panes[1], operation));
        Assert.Equal(0, liveEvents);
        var preview = DockPreviewLayout.Measure(projection, new Size(1000, 600), 0)!.Value;
        // Previewing must not remove the live source or adjust any live proportions.
        Assert.Same(panes[0], source.Owner);
        Assert.Equal(0.4, panes[0].Proportion, 3);
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, panes[1], DragAction.Move, operation, false));
        Assert.True(manager.ValidateDockable(source, panes[1], DragAction.Move, operation, true));
        var actual = Measure(root)[source.Owner!];
        AssertBounds(preview, actual);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Global_share_is_independent_of_intermediate_arrange(bool arrangeBeforeProportion)
    {
        var (_, targetRoot, targets) = CreateLayout(0.5, 0.5);
        var (_, sourceRoot, sources) = CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        Measure(targetRoot);
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, targets[0].Owner!, DragAction.Move, DockOperation.Left, true));
        if (arrangeBeforeProportion) Measure(targetRoot);
        Assert.True(GlobalDockingService.Instance.TryApplyGlobalDockingProportion(source, sourceRoot, targetRoot, 0.33));
        Assert.Equal(330, Measure(targetRoot)[source.Owner!].Width, 0);
    }

    private sealed class DropState : DockControlState
    {
        internal DropState(DockManager manager, DockDragContext context, Control dropControl, DockDropPreviewService? preview = null)
            : base(manager, new DefaultDragOffsetCalculator(), context: context, dropPreview: preview) => DropControl = dropControl;
    }

    [AvaloniaFact]
    public void Global_container_drag_sizes_the_inserted_pane()
    {
        var (_, targetRoot, targets) = CreateLayout(0.5, 0.5);
        var (_, sourceRoot, sources) = CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        Measure(targetRoot);
        var dockControl = new Dock.Avalonia.Controls.DockControl { Layout = targetRoot };
        var window = new Window { Width = 1000, Height = 600, Content = dockControl };
        var previousProportion = Dock.Settings.DockSettings.GlobalDockingProportion;
        try
        {
            Dock.Settings.DockSettings.GlobalDockingProportion = 0.33;
            window.Show();
            window.UpdateLayout();
            var dragControl = new Border { DataContext = sources[0] };
            var dropControl = new Border { DataContext = targets[0] };
            dropControl.SetValue(Dock.Settings.DockProperties.IsDropEnabledProperty, true);
            var context = new DockDragContext
            {
                DragControl = dragControl, DoDragDrop = true,
                TargetPoint = new Point(10, 10), TargetDockControl = dockControl,
                ResolvedOperation = DockOperation.Left, UseGlobalOperation = true, HasResolvedOperation = true
            };
            var state = new DropState(new DockManager(new DockService()), context, dropControl);
            state.Process(default, default, EventType.Released, DragAction.Move, dockControl, new List<IDockControl> { dockControl });
            Assert.Equal(330, Measure(targetRoot)[source.Owner!].Width, 0);
            Assert.False(dockControl.IsDraggingDock);
        }
        finally { window.Close(); Dock.Settings.DockSettings.GlobalDockingProportion = previousProportion; }
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Bottom)]
    public void Nested_resized_layout_matches_projection_with_real_splitters(DockOperation operation)
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        var nested = factory.CreateProportionalDock();
        nested.Orientation = Orientation.Vertical;
        nested.Proportion = nested.CollapsedProportion = 0.3;
        nested.VisibleDockables = factory.CreateList<IDockable>();
        var owner = (IDock)panes[1].Owner!;
        owner.VisibleDockables![2] = nested;
        factory.AddDockable(nested, panes[1]);
        factory.AddDockable(nested, factory.CreateProportionalDockSplitter());
        var bottom = factory.CreateToolDock();
        bottom.VisibleDockables = factory.CreateList<IDockable>(factory.CreateTool());
        factory.AddDockable(nested, bottom);
        panes[1].Proportion = panes[1].CollapsedProportion = 0.72;
        bottom.Proportion = bottom.CollapsedProportion = 0.28;
        factory.InitLayout(root);
        Measure(root, 6);
        var source = panes[0].ActiveDockable!;
        var projection = Assert.IsType<DockSplitPreview>(DockSplitPreview.Create(source, bottom, operation));
        var preview = DockPreviewLayout.Measure(projection, new Size(1000, 600), 6)!.Value;
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, bottom, DragAction.Move, operation, true));
        AssertBounds(preview, Measure(root, 6)[source.Owner!]);
    }

    [AvaloniaFact]
    public void Minimum_sizes_are_measured_by_the_same_layout_engine()
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        factory.AddDockable(panes[0], factory.CreateTool());
        panes[1].MinWidth = 200;
        Measure(root, 4);
        var source = panes[0].ActiveDockable!;
        var projection = Assert.IsType<DockSplitPreview>(DockSplitPreview.Create(source, panes[1], DockOperation.Right));
        var preview = DockPreviewLayout.Measure(projection, new Size(1000, 600))!.Value;
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, true));
        AssertBounds(preview, Measure(root, 4)[source.Owner!]);
    }

    private sealed class CountingFactory : Factory
    {
        internal int PreviewCount { get; private set; }
        public override IFactory CreatePreviewFactory() { PreviewCount++; return base.CreatePreviewFactory(); }
    }

    [AvaloniaFact]
    public void Preview_cache_invalidates_when_live_proportions_change()
    {
        var countingFactory = new CountingFactory();
        var (factory, root, panes) = CreateLayout(countingFactory, 0.4, 0.3, 0.3);
        factory.AddDockable(panes[0], factory.CreateTool());
        var control = new Dock.Avalonia.Controls.DockControl { Layout = root };
        control.Measure(new Size(1000, 600));
        control.Arrange(new Rect(0, 0, 1000, 600));
        var service = new DockDropPreviewService();
        var source = panes[0].ActiveDockable!;
        var first = service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN);
        Assert.NotNull(first);
        Assert.Equal(first, service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 1000; index++)
            service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        stopwatch.Stop();
        output.WriteLine($"1000 unchanged hovers: {stopwatch.Elapsed.TotalMilliseconds:F3} ms, {allocated} bytes, {countingFactory.PreviewCount} projection(s).");
        Assert.Equal(1, countingFactory.PreviewCount);
        panes[1].Proportion = panes[1].CollapsedProportion = 0.2;
        panes[2].Proportion = panes[2].CollapsedProportion = 0.4;
        var changed = service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN);
        Assert.NotEqual(first, changed);
        Assert.Equal(2, countingFactory.PreviewCount);
        Assert.Equal(0.2, panes[1].Proportion, 3);
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Bottom)]
    public void Projected_bounds_match_real_DockControl_after_drop(DockOperation operation)
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        var source = panes[0].ActiveDockable!;
        var control = new Dock.Avalonia.Controls.DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            window.UpdateLayout();
            var service = new DockDropPreviewService();
            var preview = service.GetBounds(source, panes[1], operation, control, double.NaN)!.Value;
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[1], DragAction.Move, operation, true));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var position = realized.TranslatePoint(default, control)!.Value;
            AssertBounds(preview, new Rect(position, realized.Bounds.Size));
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Release_cancels_a_preview_if_the_layout_changed_since_hover()
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        var source = panes[0].ActiveDockable!;
        var control = new Dock.Avalonia.Controls.DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var preview = new DockDropPreviewService();
            Assert.NotNull(preview.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN));
            var dropControl = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[panes[1]]);
            dropControl.SetValue(Dock.Settings.DockProperties.IsDropEnabledProperty, true);
            var context = new DockDragContext
            {
                DragControl = new Border { DataContext = source }, DoDragDrop = true,
                TargetPoint = new Point(10, 10), TargetDockControl = control,
                ResolvedOperation = DockOperation.Right, HasResolvedOperation = true
            };
            panes[1].Proportion = panes[1].CollapsedProportion = 0.2;
            panes[2].Proportion = panes[2].CollapsedProportion = 0.4;
            var state = new DropState(new DockManager(new DockService()), context, dropControl, preview);
            state.Process(default, default, EventType.Released, DragAction.Move, control, new List<IDockControl> { control });
            Assert.Same(panes[0], source.Owner);
            Assert.Single(panes[0].VisibleDockables!);
            Assert.False(preview.HasProjection);
        }
        finally { window.Close(); }
    }

    internal static (Factory factory, IRootDock root, IToolDock[] panes) CreateLayout(params double[] shares) => CreateLayout(new Factory(), shares);

    internal static (Factory factory, IRootDock root, IToolDock[] panes) CreateLayout(Factory factory, params double[] shares)
    {
        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = factory.CreateList<IDockable>();
        var row = factory.CreateProportionalDock();
        row.Orientation = Orientation.Horizontal;
        row.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, row);
        var panes = new IToolDock[shares.Length];
        for (var i = 0; i < shares.Length; i++)
        {
            if (i > 0) factory.AddDockable(row, factory.CreateProportionalDockSplitter());
            var pane = factory.CreateToolDock();
            pane.Id = "pane-" + i;
            pane.Proportion = pane.CollapsedProportion = shares[i];
            pane.VisibleDockables = factory.CreateList<IDockable>();
            factory.AddDockable(row, pane);
            var tool = factory.CreateTool();
            tool.Id = "tool-" + i;
            factory.AddDockable(pane, tool);
            pane.ActiveDockable = tool;
            panes[i] = pane;
        }
        factory.InitLayout(root);
        return (factory, root, panes);
    }

    internal static Dictionary<IDockable, Rect> Measure(IDock root, double splitterThickness = 0)
    {
        var controls = new Dictionary<IDockable, Control>();
        Control Build(IDockable model)
        {
            Control control;
            if (model is IProportionalDock row)
            {
                var panel = new ProportionalStackPanel
                {
                    Orientation = row.Orientation == Orientation.Horizontal
                        ? global::Avalonia.Layout.Orientation.Horizontal : global::Avalonia.Layout.Orientation.Vertical
                };
                foreach (var child in row.VisibleDockables!)
                    panel.Children.Add(Build(child));
                control = panel;
            }
            else if (model is IRootDock dock) return Build(dock.VisibleDockables![0]);
            else if (model is IProportionalDockSplitter) control = new ProportionalStackPanelSplitter { Thickness = splitterThickness };
            else control = new Border();
            controls[model] = control;
            control.MinWidth = double.IsNaN(model.MinWidth) ? 0 : model.MinWidth;
            control.MinHeight = double.IsNaN(model.MinHeight) ? 0 : model.MinHeight;
            control.MaxWidth = double.IsNaN(model.MaxWidth) ? double.PositiveInfinity : model.MaxWidth;
            control.MaxHeight = double.IsNaN(model.MaxHeight) ? double.PositiveInfinity : model.MaxHeight;
            ProportionalStackPanel.SetIsCollapsed(control, model.IsCollapsable && model.IsEmpty);
            ProportionalStackPanel.SetProportion(control, model.Proportion);
            ProportionalStackPanel.SetCollapsedProportion(control, model.CollapsedProportion);
            return control;
        }
        var visual = Build(root);
        visual.Measure(new Size(1000, 600));
        visual.Arrange(new Rect(0, 0, 1000, 600));
        var result = new Dictionary<IDockable, Rect>();
        foreach (var pair in controls)
        {
            var position = pair.Value.TranslatePoint(default, visual)!.Value;
            result[pair.Key] = new Rect(position, pair.Value.Bounds.Size);
            // Mirror the production panel's two-way proportion binding without reflection bindings.
            pair.Key.Proportion = ProportionalStackPanel.GetProportion(pair.Value);
            pair.Key.CollapsedProportion = ProportionalStackPanel.GetCollapsedProportion(pair.Value);
        }
        return result;
    }

    internal static void AssertBounds(Rect expected, Rect actual)
    {
        Assert.InRange(Math.Abs(expected.X - actual.X), 0, 1);
        Assert.InRange(Math.Abs(expected.Y - actual.Y), 0, 1);
        Assert.InRange(Math.Abs(expected.Width - actual.Width), 0, 1);
        Assert.InRange(Math.Abs(expected.Height - actual.Height), 0, 1);
    }
}
