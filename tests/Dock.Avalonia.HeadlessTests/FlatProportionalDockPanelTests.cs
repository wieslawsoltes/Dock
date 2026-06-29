using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
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
        Assert.All(surfaces, surface => Assert.True(DockProperties.GetIsDockTarget(surface)));
        Assert.All(surfaces, surface => Assert.Same(surface, DockProperties.GetDockAdornerHost(surface)));
        Assert.All(surfaces, surface => Assert.False(DockProperties.GetShowDockIndicatorOnly(surface)));

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

    [AvaloniaFact]
    public void FlatProportionalDockPanel_LayoutTransitions_Default_And_CanSet()
    {
        var panel = new FlatProportionalDockPanel();

        Assert.True(panel.UseLayoutTransitions);
        Assert.Equal(TimeSpan.FromMilliseconds(240), panel.LayoutTransitionDuration);

        panel.UseLayoutTransitions = false;
        panel.LayoutTransitionDuration = TimeSpan.FromMilliseconds(80);

        Assert.False(panel.UseLayoutTransitions);
        Assert.Equal(TimeSpan.FromMilliseconds(80), panel.LayoutTransitionDuration);
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Reuses_Existing_Visuals_When_Layout_Rebuilds()
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

        var leftPresenter = panel.Children
            .OfType<ContentPresenter>()
            .Single(presenter => ReferenceEquals(presenter.Content, left));
        var leftBounds = leftPresenter.Bounds;

        root.VisibleDockables = new List<IDockable> { right, splitter, left };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var movedLeftPresenter = panel.Children
            .OfType<ContentPresenter>()
            .Single(presenter => ReferenceEquals(presenter.Content, left));
        Dispatcher.UIThread.RunJobs();

        Assert.Same(leftPresenter, movedLeftPresenter);
        Assert.NotEqual(leftBounds.X, movedLeftPresenter.Bounds.X);
        Assert.Equal(1.0, movedLeftPresenter.Opacity);
        Assert.True(movedLeftPresenter.IsHitTestVisible);
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Rebuild_Keeps_Reused_Presenter_Attached()
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
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var detached = 0;
            var attached = 0;
            leftPresenter.DetachedFromVisualTree += (_, _) => detached++;
            leftPresenter.AttachedToVisualTree += (_, _) => attached++;

            root.VisibleDockables = new List<IDockable> { right, splitter, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.Same(leftPresenter, GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left)));
            Assert.Equal(0, detached);
            Assert.Equal(0, attached);
            Assert.Equal(1.0, leftPresenter.Opacity);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Rebuild_Reuses_And_Moves_Splitter_Visual()
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
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var splitterControl = panel.Children
                .OfType<FlatProportionalDockSplitter>()
                .Single(control => ReferenceEquals(control.Splitter, splitter));
            var previousBounds = splitterControl.Bounds;

            root.VisibleDockables = new List<IDockable> { right, splitter, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            var movedSplitterControl = panel.Children
                .OfType<FlatProportionalDockSplitter>()
                .Single(control => ReferenceEquals(control.Splitter, splitter));

            Assert.Same(splitterControl, movedSplitterControl);
            Assert.NotEqual(previousBounds.X, movedSplitterControl.Bounds.X);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Moved_Dockable_Animates_Live_Presenter()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var visibleDockables = new ObservableCollection<IDockable> { left, right };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = visibleDockables
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(20)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var previousBounds = leftPresenter.Bounds;

            visibleDockables.Insert(2, left);
            visibleDockables.RemoveAt(0);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.NotEqual(previousBounds.X, leftPresenter.Bounds.X);
            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, rightPresenter.Opacity);

            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.True(leftPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Collection_Move_Animates_Moved_Presenter_Only()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var visibleDockables = new ObservableCollection<IDockable> { left, right };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = visibleDockables
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(40)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var previousLeftBounds = leftPresenter.Bounds;
            var previousRightBounds = rightPresenter.Bounds;

            visibleDockables.Move(0, 1);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.NotEqual(previousLeftBounds.X, leftPresenter.Bounds.X);
            Assert.NotEqual(previousRightBounds.X, rightPresenter.Bounds.X);
            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
            Assert.Equal(1.0, rightPresenter.Opacity);

            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Direct_Move_Animates_Moved_Presenter_Only()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0 / 3.0, CollapsedProportion = 1.0 / 3.0 };
        var middle = new DocumentDock { Id = "Middle", Proportion = 1.0 / 3.0, CollapsedProportion = 1.0 / 3.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 1.0 / 3.0, CollapsedProportion = 1.0 / 3.0 };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, middle, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(40)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(990, 600));
            panel.Arrange(new Rect(0, 0, 990, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var middlePresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, middle));
            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var previousLeftBounds = leftPresenter.Bounds;
            var previousMiddleBounds = middlePresenter.Bounds;
            var previousRightBounds = rightPresenter.Bounds;
            var middleVisual = ElementComposition.GetElementVisual(middlePresenter);
            var rightVisual = ElementComposition.GetElementVisual(rightPresenter);

            Assert.NotNull(middleVisual);
            Assert.NotNull(rightVisual);

            root.VisibleDockables = new List<IDockable> { middle, right, left };

            panel.Measure(new Size(990, 600));
            panel.Arrange(new Rect(0, 0, 990, 600));

            Assert.NotEqual(previousLeftBounds.X, leftPresenter.Bounds.X);
            Assert.NotEqual(previousMiddleBounds.X, middlePresenter.Bounds.X);
            Assert.NotEqual(previousRightBounds.X, rightPresenter.Bounds.X);
            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.True(middlePresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
            Assert.Equal(middlePresenter.Bounds.X, middleVisual.Offset.X, 3);
            Assert.Equal(rightPresenter.Bounds.X, rightVisual.Offset.X, 3);

            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.True(middlePresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.True(middlePresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Retargeted_Move_Keeps_Presenter_Disabled_Until_Latest_Transition_Completes()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(80)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));

            root.VisibleDockables = new List<IDockable> { right, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            await Task.Delay(50);
            Dispatcher.UIThread.RunJobs();

            root.VisibleDockables = new List<IDockable> { left, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            await Task.Delay(60);
            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            await Task.Delay(70);
            Dispatcher.UIThread.RunJobs();

            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(leftPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(leftPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(leftPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(leftPresenter.Bounds.Height, visual.Size.Y, 3);
            Assert.Equal(1.0, visual.Scale.X, 3);
            Assert.Equal(1.0, visual.Scale.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Coalesced_Move_Back_To_Original_Bounds_Cancels_Pending_Animation()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var originalBounds = leftPresenter.Bounds;

            root.VisibleDockables = new List<IDockable> { right, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.False(leftPresenter.IsHitTestVisible);

            root.VisibleDockables = new List<IDockable> { left, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.Equal(originalBounds, leftPresenter.Bounds);
            Assert.True(leftPresenter.IsHitTestVisible);

            Dispatcher.UIThread.RunJobs();

            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(leftPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(leftPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(leftPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(leftPresenter.Bounds.Height, visual.Size.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Queued_Move_Targets_Arranged_Bounds_Not_Stale_Composition_Offset()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(1000)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);

            root.VisibleDockables = new List<IDockable> { right, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            visual.Offset = new Vector3D(123.0, 45.0, 0.0);

            for (var attempt = 0; attempt < 10 && AreClose(visual.Offset.X, 123.0); attempt++)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(20);
            }

            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);
            Assert.NotEqual(123.0, visual.Offset.X, 3);
            Assert.NotEqual(45.0, visual.Offset.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Moved_Dockable_Between_Parents_Animates_Live_Presenter_With_Size_Delta()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 1.0, CollapsedProportion = 1.0 };
        var leftItems = new ObservableCollection<IDockable> { left };
        var rightItems = new ObservableCollection<IDockable> { right };
        var leftDock = new ProportionalDock
        {
            Id = "LeftDock",
            Orientation = Orientation.Horizontal,
            Proportion = 0.3,
            CollapsedProportion = 0.3,
            VisibleDockables = leftItems
        };
        var rightDock = new ProportionalDock
        {
            Id = "RightDock",
            Orientation = Orientation.Horizontal,
            Proportion = 0.7,
            CollapsedProportion = 0.7,
            VisibleDockables = rightItems
        };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { leftDock, splitter, rightDock }
        };
        var panel = new FlatProportionalDockPanel { Dock = root };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var leftPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, left));
        var previousBounds = leftPresenter.Bounds;

        rightItems.Add(left);
        leftItems.Remove(left);

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        Assert.NotEqual(previousBounds.X, leftPresenter.Bounds.X);
        Assert.NotEqual(previousBounds.Width, leftPresenter.Bounds.Width);
        Assert.Equal(1.0, leftPresenter.Opacity);
        Assert.False(leftPresenter.IsHitTestVisible);
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Unchanged_Rebuild_Does_Not_Create_Exit_Presenter()
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

        var leftPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, left));

        root.VisibleDockables = new List<IDockable> { left, splitter, right };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        Assert.Same(leftPresenter, GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, left)));
        Assert.Equal(1.0, leftPresenter.Opacity);
        Assert.True(leftPresenter.IsHitTestVisible);
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Removed_Dockable_Keeps_Exit_Presenter()
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

        var rightPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, right));
        var previousBounds = rightPresenter.Bounds;

        root.VisibleDockables = new List<IDockable> { left };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var exitingPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, right));

        Assert.Same(rightPresenter, exitingPresenter);
        Assert.Same(right, exitingPresenter.Content);
        Assert.Equal(previousBounds, exitingPresenter.Bounds);
        Assert.False(exitingPresenter.IsHitTestVisible);
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Removed_Dockable_Keeps_Remaining_Presenter_Interactive()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(20)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var previousBounds = leftPresenter.Bounds;
            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);

            root.VisibleDockables = new List<IDockable> { left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.NotEqual(previousBounds.Width, leftPresenter.Bounds.Width);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.Equal(leftPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(leftPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(leftPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(leftPresenter.Bounds.Height, visual.Size.Y, 3);
            Assert.Same(right, GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right)).Content);

            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.DoesNotContain(GetLivePresenters(panel), presenter => ReferenceEquals(presenter.Content, right));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Reinserted_Dockable_Cancels_Exit_And_Reuses_Presenter()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));

            left.Proportion = 0.7;
            left.CollapsedProportion = 0.7;
            right.Proportion = 0.3;
            right.CollapsedProportion = 0.3;
            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.False(rightPresenter.IsHitTestVisible);
            Assert.Same(rightPresenter, GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right)));

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            var reinsertedRightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));

            Assert.Same(rightPresenter, reinsertedRightPresenter);
            Assert.Equal(1.0, reinsertedRightPresenter.Opacity);
            Assert.False(reinsertedRightPresenter.IsHitTestVisible);

            await Task.Delay(220);
            Dispatcher.UIThread.RunJobs();

            Assert.Same(rightPresenter, GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right)));
            Assert.True(reinsertedRightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Inserted_Dockable_Animates_Live_Presenter()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(20)
        };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        root.VisibleDockables = new List<IDockable> { left, splitter, right };

        panel.Measure(new Size(1000, 600));
        panel.Arrange(new Rect(0, 0, 1000, 600));

        var rightPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, right));
        var leftPresenter = GetLivePresenters(panel)
            .Single(presenter => ReferenceEquals(presenter.Content, left));
        Assert.Equal(1.0, leftPresenter.Opacity);
        Assert.Equal(0.0, rightPresenter.Opacity);
        Assert.False(rightPresenter.IsHitTestVisible);

        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1.0, rightPresenter.Opacity);
        Assert.True(rightPresenter.IsHitTestVisible);

        await Task.Delay(60);
        Dispatcher.UIThread.RunJobs();

        Assert.True(rightPresenter.IsHitTestVisible);
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Collection_Insert_Does_Not_Animate_Existing_Sibling()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var visibleDockables = new ObservableCollection<IDockable> { left };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = visibleDockables
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(40)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var previousBounds = leftPresenter.Bounds;

            visibleDockables.Add(splitter);
            visibleDockables.Add(right);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));

            Assert.NotEqual(previousBounds.Width, leftPresenter.Bounds.Width);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Scoped_Insert_Normalizes_Previous_Excluded_Active_Animation()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var center = new DocumentDock { Id = "Center", Proportion = 0.33, CollapsedProportion = 0.33 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var visibleDockables = new ObservableCollection<IDockable> { left, right };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = visibleDockables
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(1000)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));

            visibleDockables.Move(0, 1);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            visibleDockables.Insert(1, splitter);
            visibleDockables.Insert(2, center);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var centerPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, center));
            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.Equal(leftPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(leftPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(leftPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(leftPresenter.Bounds.Height, visual.Size.Y, 3);
            Assert.Equal(0.0, centerPresenter.Opacity);
            Assert.False(centerPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Active_Insert_Retargets_Layout_Without_Early_HitTesting()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var insertedBounds = rightPresenter.Bounds;

            Assert.False(rightPresenter.IsHitTestVisible);

            await Task.Delay(70);
            Dispatcher.UIThread.RunJobs();

            root.VisibleDockables = new List<IDockable> { right, splitter, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.NotEqual(insertedBounds.X, rightPresenter.Bounds.X);
            Assert.False(rightPresenter.IsHitTestVisible);

            await Task.Delay(70);
            Dispatcher.UIThread.RunJobs();

            Assert.False(rightPresenter.IsHitTestVisible);

            await Task.Delay(140);
            Dispatcher.UIThread.RunJobs();

            var visual = ElementComposition.GetElementVisual(rightPresenter);

            Assert.NotNull(visual);
            Assert.True(rightPresenter.IsHitTestVisible);
            Assert.Equal(rightPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(rightPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(rightPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(rightPresenter.Bounds.Height, visual.Size.Y, 3);
            Assert.Equal(1.0f, visual.Opacity, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Pending_Insert_Retargets_Layout_Without_Flash()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var insertedBounds = rightPresenter.Bounds;

            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            root.VisibleDockables = new List<IDockable> { right, splitter, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            Assert.NotEqual(insertedBounds.X, rightPresenter.Bounds.X);
            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            Dispatcher.UIThread.RunJobs();

            Assert.False(rightPresenter.IsHitTestVisible);

            await Task.Delay(240);
            Dispatcher.UIThread.RunJobs();

            var visual = ElementComposition.GetElementVisual(rightPresenter);

            Assert.NotNull(visual);
            Assert.True(rightPresenter.IsHitTestVisible);
            Assert.Equal(1.0, rightPresenter.Opacity);
            Assert.Equal(rightPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(rightPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(rightPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(rightPresenter.Bounds.Height, visual.Size.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Pending_Insert_Retarget_Starts_Connected_Layout_Animation()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(1000)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));

            root.VisibleDockables = new List<IDockable> { right, splitter, left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var visual = ElementComposition.GetElementVisual(rightPresenter);

            Assert.NotNull(visual);

            visual.Offset = new Vector3D(123.0, 45.0, 0.0);

            for (var attempt = 0; attempt < 10 && AreClose(visual.Offset.X, 123.0); attempt++)
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(20);
            }

            Dispatcher.UIThread.RunJobs();

            Assert.False(rightPresenter.IsHitTestVisible);
            Assert.NotEqual(123.0, visual.Offset.X, 3);
            Assert.NotEqual(45.0, visual.Offset.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Inserted_Dockable_Keeps_Existing_Presenter_Interactive()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(20)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var previousBounds = leftPresenter.Bounds;
            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotNull(visual);

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));

            Assert.NotEqual(previousBounds.Width, leftPresenter.Bounds.Width);
            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, leftPresenter.Opacity);
            Assert.Equal(leftPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(leftPresenter.Bounds.Y, visual.Offset.Y, 3);
            Assert.Equal(leftPresenter.Bounds.Width, visual.Size.X, 3);
            Assert.Equal(leftPresenter.Bounds.Height, visual.Size.Y, 3);
            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.True(rightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_LayoutResize_Animates_Size_Not_Content_Scale()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));
            var previousBounds = leftPresenter.Bounds;

            left.Proportion = 0.7;
            left.CollapsedProportion = 0.7;
            right.Proportion = 0.3;
            right.CollapsedProportion = 0.3;
            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            var visual = ElementComposition.GetElementVisual(leftPresenter);

            Assert.NotEqual(previousBounds.Width, leftPresenter.Bounds.Width);
            Assert.NotNull(visual);
            Assert.Equal(1.0, visual.Scale.X, 3);
            Assert.Equal(1.0, visual.Scale.Y, 3);
            Assert.Equal(1.0, visual.Scale.Z, 3);
            Assert.False(leftPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Disabling_Transitions_Cancels_Active_Layout_Animation()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var visibleDockables = new ObservableCollection<IDockable> { left, right };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = visibleDockables
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var leftPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, left));

            visibleDockables.Insert(2, left);
            visibleDockables.RemoveAt(0);

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.False(leftPresenter.IsHitTestVisible);

            panel.UseLayoutTransitions = false;
            Dispatcher.UIThread.RunJobs();

            Assert.True(leftPresenter.IsHitTestVisible);
            Assert.Equal(1.0, leftPresenter.Opacity);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Disabling_Transitions_Normalizes_Composition_Offset()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            var visual = ElementComposition.GetElementVisual(rightPresenter);

            Assert.NotNull(visual);

            visual.Offset = new Vector3D(12.0, 34.0, 0.0);

            panel.UseLayoutTransitions = false;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(rightPresenter.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(rightPresenter.Bounds.Y, visual.Offset.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Disabling_Transitions_Normalizes_Splitter_Composition_Offset()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var splitterControl = panel.Children
                .OfType<FlatProportionalDockSplitter>()
                .Single(control => ReferenceEquals(control.Splitter, splitter));
            var visual = ElementComposition.GetElementVisual(splitterControl);

            Assert.NotNull(visual);

            visual.Offset = new Vector3D(12.0, 34.0, 0.0);

            panel.UseLayoutTransitions = false;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(splitterControl.Bounds.X, visual.Offset.X, 3);
            Assert.Equal(splitterControl.Bounds.Y, visual.Offset.Y, 3);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public void FlatProportionalDockPanel_Disabling_Transitions_Removes_Pending_Exit_Presenter()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 0.5, CollapsedProportion = 0.5 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left, splitter, right }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(100)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            root.VisibleDockables = new List<IDockable> { left };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(GetLivePresenters(panel), presenter => ReferenceEquals(presenter.Content, right));

            panel.UseLayoutTransitions = false;
            Dispatcher.UIThread.RunJobs();

            Assert.DoesNotContain(GetLivePresenters(panel), presenter => ReferenceEquals(presenter.Content, right));
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaFact]
    public async Task FlatProportionalDockPanel_Inserted_Dockable_Live_Presenter_Remains_Disabled_Until_Transition_Completes()
    {
        var left = new DocumentDock { Id = "Left", Proportion = 1.0, CollapsedProportion = 1.0 };
        var right = new DocumentDock { Id = "Right", Proportion = 0.5, CollapsedProportion = 0.5 };
        var splitter = new ProportionalDockSplitter { Id = "Splitter" };
        var root = new ProportionalDock
        {
            Id = "Root",
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable> { left }
        };
        var panel = new FlatProportionalDockPanel
        {
            Dock = root,
            LayoutTransitionDuration = TimeSpan.FromMilliseconds(20)
        };
        var window = ShowPanel(panel);

        try
        {
            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            root.VisibleDockables = new List<IDockable> { left, splitter, right };

            panel.Measure(new Size(1000, 600));
            panel.Arrange(new Rect(0, 0, 1000, 600));

            var rightPresenter = GetLivePresenters(panel)
                .Single(presenter => ReferenceEquals(presenter.Content, right));
            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            Dispatcher.UIThread.RunJobs();

            Assert.Equal(0.0, rightPresenter.Opacity);
            Assert.False(rightPresenter.IsHitTestVisible);

            await Task.Delay(120);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1.0, rightPresenter.Opacity);
            Assert.True(rightPresenter.IsHitTestVisible);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static IReadOnlyList<ContentPresenter> GetLivePresenters(FlatProportionalDockPanel panel)
    {
        return panel.Children
            .OfType<ContentPresenter>()
            .ToList();
    }

    private static Window ShowPanel(FlatProportionalDockPanel panel)
    {
        var window = new Window
        {
            Width = 1000,
            Height = 600,
            Content = panel
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) < 1e-10;
    }
}
