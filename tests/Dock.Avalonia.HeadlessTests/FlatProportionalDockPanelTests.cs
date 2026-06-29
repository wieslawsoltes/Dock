using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FlatProportionalDockPanelTests
{
    [AvaloniaFact]
    public void FlatProportionalDockPanel_Flattens_Nested_ProportionalDocks()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.25, CollapsedProportion = 0.25 };
        var top = new DocumentDock { Id = "Top", Proportion = 0.6, CollapsedProportion = 0.6 };
        var bottom = new ToolDock { Id = "Bottom", Proportion = 0.4, CollapsedProportion = 0.4 };
        var rootSplitter = new ProportionalDockSplitter { Id = "RootSplitter" };
        var innerSplitter = new ProportionalDockSplitter { Id = "InnerSplitter" };
        var inner = new ProportionalDock
        {
            Id = "Inner",
            Orientation = Orientation.Vertical,
            Proportion = 0.75,
            CollapsedProportion = 0.75,
            VisibleDockables = new List<IDockable> { top, innerSplitter, bottom }
        };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, rootSplitter, inner }
        };
        var panel = new FlatProportionalDockPanel { Dock = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var presenters = panel.Children.OfType<ContentPresenter>().ToList();
        var splitters = panel.Children.OfType<FlatProportionalDockSplitter>().ToList();
        var surfaces = panel.Children.OfType<DockableControl>().ToList();

        Assert.Equal(3, presenters.Count);
        Assert.Equal(2, splitters.Count);
        Assert.Equal(2, surfaces.Count);
        Assert.DoesNotContain(panel.Children, child => child is ProportionalDockControl);
        Assert.Contains(surfaces, surface => ReferenceEquals(surface.DataContext, root));
        Assert.Contains(surfaces, surface => ReferenceEquals(surface.DataContext, inner));

        var leftPresenter = presenters.Single(presenter => ReferenceEquals(presenter.Content, left));
        var innerSurface = surfaces.Single(surface => ReferenceEquals(surface.DataContext, inner));

        Assert.Equal(249, leftPresenter.Bounds.Width, 0);
        Assert.Equal(253, innerSurface.Bounds.X, 0);
        Assert.Equal(747, innerSurface.Bounds.Width, 0);
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_ResizeSplitter_Updates_ModelProportions()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.25, CollapsedProportion = 0.25 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.75, CollapsedProportion = 0.75 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel { Dock = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var splitterControl = panel.Children
            .OfType<FlatProportionalDockSplitter>()
            .Single(control => ReferenceEquals(control.Splitter, splitter));

        panel.ResizeSplitter(splitterControl, 100);

        Assert.Equal(0.35, left.Proportion, 2);
        Assert.Equal(0.65, right.Proportion, 2);
    }
}
