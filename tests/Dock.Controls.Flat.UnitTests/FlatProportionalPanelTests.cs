using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
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

    [AvaloniaFact]
    public void Measure_Uses_LiveProportion_When_CollapsedProportion_IsStale()
    {
        var left = new ObservableTestItem("Left", 0.25) { CollapsedProportion = 0.25 };
        var right = new ObservableTestItem("Right", 0.75) { CollapsedProportion = 0.75 };
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { left, new TestSplitter("Splitter"), right });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        left.UpdateProportion(0.4);
        right.UpdateProportion(0.6);
        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        Assert.Equal(0.4, left.Proportion, 2);
        Assert.Equal(0.6, right.Proportion, 2);
        Assert.Equal(left.Proportion, left.CollapsedProportion);
        Assert.Equal(right.Proportion, right.CollapsedProportion);
    }

    [AvaloniaFact]
    public void Measure_Returns_ChildDesiredSize_For_UnboundedAxis()
    {
        var left = new TestItem("Left", 1.0)
        {
            Content = new Border
            {
                Width = 123,
                Height = 45
            },
            MinWidthValue = 123,
            MinHeightValue = 45
        };
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { left });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(double.PositiveInfinity, 100));

        Assert.True(panel.DesiredSize.Width >= 123);
        Assert.Equal(100, panel.DesiredSize.Height);
    }

    [AvaloniaFact]
    public void Measure_With_UnboundedStackingAxis_Does_NotClamp_MaxConstraint_ToZero()
    {
        var left = new TestItem("Left", 0.4) { MaxWidthValue = 250 };
        var right = new TestItem("Right", 0.6);
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { left, new TestSplitter("Splitter"), right });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(double.PositiveInfinity, 100));

        Assert.Equal(0.4, left.Proportion, 2);
        Assert.Equal(0.6, right.Proportion, 2);
    }

    [AvaloniaFact]
    public void ContentChange_Refreshes_ExistingPresenter()
    {
        var item = new ObservableTestItem("Item", 1.0);
        var root = new TestDock(
            "Root",
            Orientation.Horizontal,
            1.0,
            new IFlatProportionalItem[] { item });
        var panel = new FlatProportionalPanel { Root = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var nextContent = new TextBlock { Text = "Updated" };
        item.UpdateContent(nextContent);

        var presenter = panel.Children.OfType<ContentPresenter>().Single();

        Assert.Same(nextContent, presenter.Content);
        Assert.Same(nextContent, presenter.DataContext);
    }

    [AvaloniaFact]
    public void Reattach_Resubscribes_VisibleItemsChanges()
    {
        var left = new TestItem("Left", 0.5);
        var right = new TestItem("Right", 0.5);
        var items = new ObservableCollection<IFlatProportionalItem> { left, new TestSplitter("Splitter"), right };
        var root = new TestDock("Root", Orientation.Horizontal, 1.0, items);
        var panel = new FlatProportionalPanel
        {
            Root = root,
            UseLayoutTransitions = false
        };
        var window = new Window
        {
            Content = panel,
            Width = 1000,
            Height = 600
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Content = panel;
            Dispatcher.UIThread.RunJobs();

            items.Remove(right);
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var presenter = panel.Children.OfType<ContentPresenter>().Single();

            Assert.Same(left.Content, presenter.Content);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
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

        public object? Content { get; set; }

        public double Proportion { get; set; }

        public double CollapsedProportion { get; set; }

        public double MinWidthValue { get; init; }

        public double MinHeightValue { get; init; }

        public double MinWidth => MinWidthValue;

        public double MinHeight => MinHeightValue;

        public double MaxWidthValue { get; init; } = double.PositiveInfinity;

        public double MaxHeightValue { get; init; } = double.PositiveInfinity;

        public double MaxWidth => MaxWidthValue;

        public double MaxHeight => MaxHeightValue;

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

    private sealed class ObservableTestItem : TestItem, INotifyPropertyChanged
    {
        public ObservableTestItem(string id, double proportion)
            : base(id, proportion)
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void UpdateContent(object? content)
        {
            Content = content;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
        }

        public void UpdateProportion(double proportion)
        {
            Proportion = proportion;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Proportion)));
        }
    }
}
