using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Dock.Model.CommandBars;
using Dock.Avalonia.CommandBars;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
#if AVALONIA_11
using Dock.Model.Mvvm;
#else
using Dock.Model.Avalonia;
#endif
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
#if AVALONIA_11
using Xunit.Abstractions;
#endif

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewGeometryTests(ITestOutputHelper output)
{
#if AVALONIA_11
    private sealed class ScaledWindow : Window, global::Avalonia.Layout.ILayoutRoot
    {
        public double LayoutScaling { get; set; } = 1;
    }
#else
    private sealed class ScaledWindow : Window
    {
        public double LayoutScaling
        {
            get => RenderScaling;
            set => this.SetRenderScaling(value);
        }
    }
#endif

    public static System.Collections.Generic.IEnumerable<object[]> ScalingCases()
    {
        foreach (var scale in new[] { 1.25, 1.5, 2.0 })
        foreach (var global in new[] { false, true })
        foreach (var operation in new[] { DockOperation.Left, DockOperation.Right, DockOperation.Top, DockOperation.Bottom })
            yield return new object[] { scale, global, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(ScalingCases))]
    public void Nested_projection_matches_release_at_display_scaling(double scale, bool global, DockOperation operation)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.41, 0.59);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        IDock targetPane = panes[1];
        for (var depth = 0; depth < 4; depth++)
        {
            var parent = Assert.IsAssignableFrom<IDock>(targetPane.Owner);
            var group = factory.CreateProportionalDock();
            group.Orientation = depth % 2 == 0 ? Orientation.Vertical : Orientation.Horizontal;
            group.Proportion = group.CollapsedProportion = targetPane.Proportion;
            group.VisibleDockables = factory.CreateList<IDockable>();
            parent.VisibleDockables![parent.VisibleDockables.IndexOf(targetPane)] = group;
            factory.AddDockable(group, targetPane);
            targetPane.Proportion = targetPane.CollapsedProportion = 0.71;
            factory.AddDockable(group, factory.CreateProportionalDockSplitter());
            var sibling = factory.CreateToolDock();
            sibling.Proportion = sibling.CollapsedProportion = 0.29;
            sibling.VisibleDockables = factory.CreateList<IDockable>(factory.CreateTool());
            sibling.ActiveDockable = sibling.VisibleDockables[0];
            factory.AddDockable(group, sibling);
        }
        factory.InitLayout(root);
        var control = new DockControl { Layout = root };
        var window = new ScaledWindow { LayoutScaling = scale, Width = 1003, Height = 607, Content = control };
        try
        {
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Equal(scale, global::Avalonia.Layout.LayoutHelper.GetLayoutScale(control));
            var target = global ? root.VisibleDockables![0] : targetPane;
            var proportion = global ? 0.33 : double.NaN;
            var visualChildren = control.GetVisualChildren().ToArray();
            var preview = new DockDropPreviewService().GetBounds(source, target, operation, control, proportion)!.Value;
            Assert.Equal(visualChildren, control.GetVisualChildren().ToArray());
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
            if (global) Assert.True(DockSplitProportion.Apply(Assert.IsAssignableFrom<IDock>(source.Owner), proportion));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Scaling={scale}; preview={preview}; actual={actual}");
            var pixel = 1 / scale + 0.00001;
            Assert.InRange(preview.X - actual.X, -pixel, pixel);
            Assert.InRange(preview.Y - actual.Y, -pixel, pixel);
            Assert.InRange(preview.Right - actual.Right, -pixel, pixel);
            Assert.InRange(preview.Bottom - actual.Bottom, -pixel, pixel);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Preview_cache_invalidates_when_display_scaling_changes()
    {
        var (_, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.6);
        var control = new DockControl { Layout = root };
        var window = new ScaledWindow { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var service = new DockDropPreviewService();
            var source = panes[0].ActiveDockable!;
            Assert.NotNull(service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN));
            Assert.True(service.IsCurrent(source, panes[1], DockOperation.Right, control, double.NaN));
            window.LayoutScaling = 1.5;
            Assert.False(service.IsCurrent(source, panes[1], DockOperation.Right, control, double.NaN));
        }
        finally { window.Close(); }
    }

    private sealed class WidePaneFactory : Factory
    {
        public override IToolDock CreateToolDock()
        {
            var pane = base.CreateToolDock();
            pane.MinWidth = 300;
            return pane;
        }
        public override IFactory CreatePreviewFactory() => new WidePaneFactory();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cross_factory_projection_preserves_each_factory(bool customSource)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(customSource ? new Factory() : new WidePaneFactory(), 0.5, 0.5);
        var (sourceFactory, sourceRoot, sources) = DockDropPreviewTests.CreateLayout(customSource ? new WidePaneFactory() : new Factory(), 1.0);
        var source = sources[0].ActiveDockable!;
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var projection = Assert.IsType<DockSplitPreview>(DockSplitPreview.Create(source, panes[1], DockOperation.Right));
            Assert.NotSame(factory, projection.Copies[root].Factory);
            Assert.NotSame(sourceFactory, projection.Copies[sourceRoot].Factory);
            Assert.NotSame(projection.Copies[root].Factory, projection.Copies[sourceRoot].Factory);
            Assert.Same(sources[0], source.Owner);
            Assert.Same(sourceFactory, source.Factory);
            Assert.Same(source, sources[0].ActiveDockable);
            Assert.Single(sources[0].VisibleDockables!);
            Assert.Equal(0.5, panes[1].Proportion);
            var preview = new DockDropPreviewService().GetBounds(source, panes[1], DockOperation.Right, control, double.NaN)!.Value;
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, true));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Projected={preview}; actual={actual}");
            DockDropPreviewTests.AssertBounds(preview, actual);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Preview_cache_invalidates_when_splitter_thickness_changes()
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var service = new DockDropPreviewService();
            var source = panes[0].ActiveDockable!;
            var before = service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN)!.Value;
            window.Styles.Add(new Style(x => x.OfType<ProportionalStackPanelSplitter>())
            {
                Setters = { new Setter(ProportionalStackPanelSplitter.ThicknessProperty, 20d) }
            });
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.False(service.IsCurrent(source, panes[1], DockOperation.Right, control, double.NaN));
            var cached = service.GetBounds(source, panes[1], DockOperation.Right, control, double.NaN)!.Value;
            var fresh = new DockDropPreviewService().GetBounds(source, panes[1], DockOperation.Right, control, double.NaN)!.Value;
            Assert.NotEqual(before, cached);
            Assert.Equal(fresh, cached);
            Assert.True(service.IsCurrent(source, panes[1], DockOperation.Right, control, double.NaN));
            output.WriteLine($"Before={before}; cached={cached}; fresh={fresh}; accepted as current={service.IsCurrent(source, panes[1], DockOperation.Right, control, double.NaN)}");
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, true));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Actual after drop={actual}");
            DockDropPreviewTests.AssertBounds(actual, cached);
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
    public void Command_bar_space_is_excluded_from_projected_geometry(bool global, DockOperation operation)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        var prior = Dock.Settings.DockSettings.CommandBarMergingEnabled;
        DockCommandBarManager? bars = null;
        try
        {
            Dock.Settings.DockSettings.CommandBarMergingEnabled = true;
            window.Show();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var target = global ? panes[1].Owner! : panes[1];
            var proportion = global ? 0.33 : double.NaN;
            var service = new DockDropPreviewService();
            var before = service.GetBounds(source, target, operation, control, proportion);
            Assert.NotNull(before);
            var host = control.GetVisualDescendants().OfType<DockCommandBarHost>().Single();
            host.BaseCommandBars = new[] { new DockCommandBarDefinition("review", DockCommandBarKind.ToolBar) { Content = new Border { Height = 40 } } };
            bars = new DockCommandBarManager(host);
            bars.Attach(root);
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using var frame = window.CaptureRenderedFrame();
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.True(host.IsVisible);
            Assert.True(host.Bounds.Height >= 40);
            Assert.False(service.IsCurrent(source, target, operation, control, proportion));
            var preview = service.GetBounds(source, target, operation, control, proportion)!.Value;
            Assert.NotEqual(before, preview);
            Assert.True(service.IsCurrent(source, target, operation, control, proportion));
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
            if (global) Assert.True(DockSplitProportion.Apply(Assert.IsAssignableFrom<IDock>(source.Owner), 0.33));
            global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Command bar height={host.Bounds.Height}; preview={preview}; actual={actual}");
            DockDropPreviewTests.AssertBounds(preview, actual);
        }
        finally { bars?.Detach(); window.Close(); Dock.Settings.DockSettings.CommandBarMergingEnabled = prior; }
    }
}
