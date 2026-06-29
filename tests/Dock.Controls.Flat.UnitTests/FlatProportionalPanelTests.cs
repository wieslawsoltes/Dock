using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Xunit;

namespace Dock.Controls.Flat.UnitTests;

public class FlatProportionalPanelTests
{
    [AvaloniaFact]
    public void FlatProportionalPanel_Flattens_InterfaceTree()
    {
        var left = new TestItem("Left", 0.25);
        var top = new TestItem("Top", 0.6);
        var bottom = new TestItem("Bottom", 0.4);
        var rootSplitter = new TestSplitter("RootSplitter");
        var innerSplitter = new TestSplitter("InnerSplitter");
        var inner = new TestDock(
            "Inner",
            Orientation.Vertical,
            0.75,
            new IFlatProportionalItem[] { top, innerSplitter, bottom });
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { left, rootSplitter, inner });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var presenters = panel.Children.OfType<ContentPresenter>().ToList();
        var splitters = panel.Children.OfType<FlatProportionalSplitter>().ToList();
        var surfaces = panel.Children.OfType<Border>().ToList();

        Assert.Equal(3, presenters.Count);
        Assert.Equal(2, splitters.Count);
        Assert.Equal(2, surfaces.Count);
        Assert.Contains(presenters, presenter => ReferenceEquals(presenter.Content, left.Content));
        Assert.Contains(presenters, presenter => ReferenceEquals(presenter.Content, top.Content));
        Assert.Contains(presenters, presenter => ReferenceEquals(presenter.Content, bottom.Content));
    }

    [AvaloniaFact]
    public void ResizeSplitter_Updates_Adjacent_ItemProportions()
    {
        var left = new TestItem("Left", 0.25);
        var right = new TestItem("Right", 0.75);
        var splitter = new TestSplitter("Splitter");
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { left, splitter, right });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var splitterControl = panel.Children
            .OfType<FlatProportionalSplitter>()
            .Single(control => ReferenceEquals(control.Splitter, splitter));

        panel.ResizeSplitter(splitterControl, 100);

        Assert.Equal(0.35, left.Proportion, 2);
        Assert.Equal(0.65, right.Proportion, 2);
        Assert.Equal(left.Proportion, left.CollapsedProportion);
        Assert.Equal(right.Proportion, right.CollapsedProportion);
    }

    [AvaloniaFact]
    public void CollectionChange_Rebuilds_FlatChildren()
    {
        var left = new TestItem("Left", 0.5);
        var right = new TestItem("Right", 0.5);
        var splitter = new TestSplitter("Splitter");
        var items = new ObservableCollection<IFlatProportionalItem> { left, splitter, right };
        var root = new TestDock("Root", Orientation.Horizontal, 1.0, items);
        var panel = new FlatProportionalPanel
        {
            Root = root,
            UseLayoutTransitions = false
        };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        items.Remove(right);
        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var presenters = panel.Children.OfType<ContentPresenter>().ToList();

        Assert.Single(presenters);
        Assert.Same(left.Content, presenters[0].Content);
    }

    [AvaloniaFact]
    public void Rebuild_Reuses_Visuals_By_ItemKey()
    {
        var firstLeft = new TestItem("Left", 0.5);
        var firstRight = new TestItem("Right", 0.5);
        var firstRoot = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { firstLeft, new TestSplitter("Splitter"), firstRight });
        var panel = new FlatProportionalPanel
        {
            Root = firstRoot,
            UseLayoutTransitions = false
        };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var firstPresenter = panel.Children
            .OfType<ContentPresenter>()
            .Single(presenter => ReferenceEquals(presenter.Content, firstLeft.Content));
        var secondLeft = new TestItem("Left", 0.4);
        var secondRight = new TestItem("Right", 0.6);
        var secondRoot = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { secondLeft, new TestSplitter("Splitter"), secondRight });

        panel.Root = secondRoot;
        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var reusedPresenter = panel.Children
            .OfType<ContentPresenter>()
            .Single(presenter => ReferenceEquals(presenter.Content, secondLeft.Content));

        Assert.Same(firstPresenter, reusedPresenter);
    }

    private class TestItem : IFlatProportionalItem
    {
        public TestItem(string id, double proportion)
        {
            Key = id;
            Content = new TextBlock { Text = id };
            Proportion = proportion;
            CollapsedProportion = proportion;
        }

        public object Key { get; }

        public object? Content { get; }

        public double Proportion { get; set; }

        public double CollapsedProportion { get; set; }

        public double MinWidth => 0;

        public double MinHeight => 0;

        public double MaxWidth => double.PositiveInfinity;

        public double MaxHeight => double.PositiveInfinity;

        public bool IsCollapsable => false;

        public bool IsEmpty => false;
    }

    private sealed class TestDock : TestItem, IFlatProportionalDock
    {
        private readonly IList<IFlatProportionalItem> _visibleItems;

        public TestDock(
            string id,
            Orientation orientation,
            double proportion,
            IList<IFlatProportionalItem> visibleItems)
            : base(id, proportion)
        {
            Orientation = orientation;
            _visibleItems = visibleItems;
        }

        public Orientation Orientation { get; }

        public IList<IFlatProportionalItem>? VisibleItems => _visibleItems;
    }

    private sealed class TestSplitter : TestItem, IFlatProportionalSplitter
    {
        public TestSplitter(string id)
            : base(id, 0)
        {
        }

        public bool CanResize => true;

        public bool ResizePreview => false;
    }
}
