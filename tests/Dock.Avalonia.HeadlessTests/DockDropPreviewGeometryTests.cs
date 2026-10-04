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
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewGeometryTests(Xunit.Abstractions.ITestOutputHelper output)
{
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
