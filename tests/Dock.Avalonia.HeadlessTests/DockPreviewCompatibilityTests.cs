using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
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

public class DockPreviewCompatibilityTests(ITestOutputHelper output)
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Projected_indicator_preserves_existing_theme_opacity(bool global)
    {
        DockTargetBase target = global ? new GlobalDockTarget() : new DockTarget();
        target.ShowIndicatorsOnly = true;
        var window = new Window { Width = 1000, Height = 600, Content = target };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var legacy = target.GetVisualDescendants().OfType<Panel>().Single(x => x.Name == "PART_LeftIndicator");
            target.GetDockOperation(new Point(10, 300), legacy, target, DragAction.Move, (_, _, _, _) => true);
            Assert.Equal(0.5, legacy.Opacity);
            target.SetPreviewBounds(new Rect(0, 0, 500, 600));
            window.UpdateLayout();
            var projected = target.GetVisualDescendants().OfType<Border>().Single(x => x.Name == "PART_PreviewIndicator");
            Assert.True(projected.IsVisible);
            output.WriteLine($"Legacy opacity={legacy.Opacity}; projected opacity={projected.Opacity}; brush={projected.Background}");
            Assert.Equal(legacy.Opacity, projected.Opacity);
        }
        finally { window.Close(); }
    }

    // Existing consumer: this factory predates the new preview API.
    private sealed class ExistingWidePaneFactory : Factory
    {
        public override IToolDock CreateToolDock()
        {
            var pane = base.CreateToolDock();
            pane.MinWidth = 300;
            return pane;
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Existing_derived_factory_keeps_legacy_preview_and_can_still_dock(bool customSource)
    {
        var (_, root, panes) = DockDropPreviewTests.CreateLayout(customSource ? new Factory() : new ExistingWidePaneFactory(), 0.5, 0.5);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(customSource ? new ExistingWidePaneFactory() : new Factory(), 1.0);
        var source = sources[0].ActiveDockable!;
        Assert.Null(DockSplitPreview.Create(source, panes[1], DockOperation.Right));
        Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, true));
        Assert.NotSame(sources[0], source.Owner);
        if (!customSource) Assert.Equal(300, source.Owner!.MinWidth);
    }

    private sealed class LivePreviewFactory : Factory
    {
        public override IFactory CreatePreviewFactory() => this;
    }

    [AvaloniaFact]
    public void A_provider_cannot_use_the_live_factory_for_projection()
    {
        var (_, _, panes) = DockDropPreviewTests.CreateLayout(new LivePreviewFactory(), 0.5, 0.5);
        var source = panes[0].ActiveDockable!;
        Assert.Null(DockSplitPreview.Create(source, panes[1], DockOperation.Right));
        Assert.Same(panes[0], source.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Visual_constraints_fall_back_and_invalidate_cached_projections(bool documents, bool vertical, bool maximum)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.5, 0.5);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        IDockable source = sources[0].ActiveDockable!;
        IDockable target = panes[1];
        if (documents)
        {
            var dock = factory.CreateDocumentDock();
            dock.VisibleDockables = factory.CreateList<IDockable>(factory.CreateDocument());
            dock.ActiveDockable = dock.VisibleDockables[0];
            root.VisibleDockables = factory.CreateList<IDockable>(dock);
            root.ActiveDockable = dock;
            factory.InitLayout(root);
            target = dock;
            var sourceDock = factory.CreateDocumentDock();
            source = factory.CreateDocument();
            sourceDock.VisibleDockables = factory.CreateList<IDockable>(source);
            sourceDock.ActiveDockable = source;
            var sourceRoot = factory.CreateRootDock();
            sourceRoot.VisibleDockables = factory.CreateList<IDockable>(sourceDock);
            sourceRoot.ActiveDockable = sourceDock;
            factory.InitLayout(sourceRoot);
        }
        var property = vertical
            ? maximum ? Control.MaxHeightProperty : Control.MinHeightProperty
            : maximum ? Control.MaxWidthProperty : Control.MinWidthProperty;
        var style = new Style(x => documents
            ? x.OfType<Window>().Class("constrained-preview").Descendant().OfType<DocumentDockControl>()
            : x.OfType<Window>().Class("constrained-preview").Descendant().OfType<ToolDockControl>())
        {
            Setters = { new Setter(property, maximum ? 450d : 300d) }
        };
        var operation = vertical ? DockOperation.Bottom : DockOperation.Right;
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        window.Styles.Add(style);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var service = new DockDropPreviewService();
            Assert.NotNull(service.GetBounds(source, target, operation, control, double.NaN));
            window.Classes.Add("constrained-preview");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.False(service.IsCurrent(source, target, operation, control, double.NaN));
            Assert.Null(service.GetBounds(source, target, operation, control, double.NaN));
            Assert.False(service.HasProjection);
            window.Classes.Remove("constrained-preview");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.NotNull(service.GetBounds(source, target, operation, control, double.NaN));
            window.Classes.Add("constrained-preview");
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Null(service.GetBounds(source, target, operation, control, double.NaN));
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
        }
        finally { window.Close(); }
    }
}
