using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Controls.DeferredContentControl;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DeferredContentControlTests
{
    private sealed class CountingTemplate : IRecyclingDataTemplate
    {
        private readonly Func<Control> _factory;

        public CountingTemplate(Func<Control> factory)
        {
            _factory = factory;
        }

        public int BuildCount { get; private set; }

        public Control? Build(object? param)
        {
            return Build(param, null);
        }

        public Control? Build(object? data, Control? existing)
        {
            if (existing is not null)
            {
                return existing;
            }

            BuildCount++;
            return _factory();
        }

        public bool Match(object? data)
        {
            return true;
        }
    }

    private sealed class TestDeferredTarget : IDeferredContentPresentationTarget
    {
        public TimeSpan ApplyDelay { get; set; }

        public bool CanApply { get; set; }

        public int ApplyCount { get; private set; }

        public int SuccessfulApplyCount { get; private set; }

        public bool ApplyDeferredPresentation()
        {
            if (ApplyDelay > TimeSpan.Zero)
            {
                Thread.Sleep(ApplyDelay);
            }

            ApplyCount++;
            if (CanApply)
            {
                SuccessfulApplyCount++;
                return true;
            }

            return false;
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Defers_Content_Materialization_Until_Dispatcher_Run()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var template = new CountingTemplate(() => new Border
        {
            Child = new TextBlock { Text = "Deferred" }
        });
        var control = new DeferredContentControl
        {
            Content = new object(),
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.NotNull(control.Presenter);
            Assert.Null(control.Presenter!.Child);
            Assert.Equal(0, template.BuildCount);

            DrainDeferredQueueBatch(window);

            Assert.NotNull(control.Presenter.Child);
            Assert.Equal(1, template.BuildCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Batches_Content_Changes_Into_A_Single_Build()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var template = new CountingTemplate(() => new TextBlock());
        var control = new DeferredContentControl
        {
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();

        try
        {
            control.Content = "First";
            control.Content = "Second";
            window.UpdateLayout();

            Assert.Equal(0, template.BuildCount);
            Assert.Null(control.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(control.Presenter!.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Reattaches_And_Applies_Deferred_Content_Changes()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var template = new CountingTemplate(() => new TextBlock());
        var control = new DeferredContentControl
        {
            Content = "First",
            ContentTemplate = template
        };
        var firstWindow = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        firstWindow.Show();
        control.ApplyTemplate();
        DrainDeferredQueueBatch(firstWindow);

        Assert.Equal(1, template.BuildCount);

        firstWindow.Content = null;
        firstWindow.Close();
        control.Content = "Second";

        Assert.Equal(1, template.BuildCount);

        var secondWindow = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        secondWindow.Show();

        try
        {
            DrainDeferredQueueBatch(secondWindow);

            Assert.Equal(2, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(control.Presenter!.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            secondWindow.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Applies_Deferred_Content_With_Standard_ContentPresenter_Template()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var template = new CountingTemplate(() => new Border
        {
            Child = new TextBlock { Text = "FallbackPresenter" }
        });
        var control = new DeferredContentControl
        {
            Template = new FuncControlTemplate<DeferredContentControl>((_, nameScope) =>
                new ContentPresenter
                {
                    Name = "PART_ContentPresenter"
                }.RegisterInNameScope(nameScope)),
            Content = new object(),
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.NotNull(control.Presenter);
            Assert.Null(control.Presenter!.Child);
            Assert.Equal(0, template.BuildCount);

            DrainDeferredQueueBatch(window);

            Assert.NotNull(control.Presenter.Child);
            Assert.Equal(1, template.BuildCount);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentControl_Limits_Realization_Batch_Per_Render_Tick()
    {
        var templates = Enumerable.Range(0, 5)
            .Select(_ => new CountingTemplate(() => new TextBlock()))
            .ToArray();
        var panel = new StackPanel();
        var controls = templates
            .Select(template => new DeferredContentControl
            {
                ContentTemplate = template
            })
            .ToArray();

        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }

        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = panel
        };

        using var _ = new DeferredBatchLimitScope(limit: 2, autoSchedule: false);

        window.Show();

        foreach (var control in controls)
        {
            control.ApplyTemplate();
        }

        window.UpdateLayout();

        try
        {
            Assert.Equal(0, templates.Sum(template => template.BuildCount));

            foreach (var control in controls)
            {
                control.Content = new object();
            }

            Assert.Equal(5, DeferredContentPresentationQueue.PendingCount);

            DrainDeferredQueueBatch(window);
            Assert.Equal(2, templates.Sum(template => template.BuildCount));

            DrainDeferredQueueBatch(window);
            Assert.Equal(4, templates.Sum(template => template.BuildCount));

            DrainDeferredQueueBatch(window);
            Assert.Equal(5, templates.Sum(template => template.BuildCount));
            Assert.All(controls, control => Assert.NotNull(control.Presenter?.Child));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DeferredContentQueue_Does_Not_Starve_Ready_Targets_Behind_NotReady_Ones()
    {
        using var _ = new DeferredBatchLimitScope(limit: 2, autoSchedule: false);

        var firstBlocked = new TestDeferredTarget();
        var secondBlocked = new TestDeferredTarget();
        var firstReady = new TestDeferredTarget { CanApply = true };
        var secondReady = new TestDeferredTarget { CanApply = true };
        var thirdReady = new TestDeferredTarget { CanApply = true };
        var targets = new[]
        {
            firstBlocked,
            secondBlocked,
            firstReady,
            secondReady,
            thirdReady
        };

        try
        {
            foreach (var target in targets)
            {
                DeferredContentPresentationQueue.Enqueue(target);
            }

            Assert.Equal(5, DeferredContentPresentationQueue.PendingCount);

            DeferredContentPresentationQueue.FlushPendingBatchForTesting();

            Assert.Equal(1, firstBlocked.ApplyCount);
            Assert.Equal(1, secondBlocked.ApplyCount);
            Assert.Equal(1, firstReady.SuccessfulApplyCount);
            Assert.Equal(1, secondReady.SuccessfulApplyCount);
            Assert.Equal(0, thirdReady.SuccessfulApplyCount);
            Assert.Equal(3, DeferredContentPresentationQueue.PendingCount);
            Assert.Equal(2, targets.Sum(target => target.SuccessfulApplyCount));

            firstBlocked.CanApply = true;
            secondBlocked.CanApply = true;

            DeferredContentPresentationQueue.FlushPendingBatchForTesting();

            Assert.Equal(4, targets.Sum(target => target.SuccessfulApplyCount));
            Assert.Equal(1, DeferredContentPresentationQueue.PendingCount);

            DeferredContentPresentationQueue.FlushPendingBatchForTesting();

            Assert.Equal(5, targets.Sum(target => target.SuccessfulApplyCount));
            Assert.Equal(0, DeferredContentPresentationQueue.PendingCount);
        }
        finally
        {
            foreach (var target in targets)
            {
                DeferredContentPresentationQueue.Remove(target);
            }
        }
    }

    [AvaloniaFact]
    public void DeferredContentQueue_Limits_Realization_Batch_By_Time_Budget()
    {
        using var _ = new DeferredBatchLimitScope(
            budgetMode: DeferredContentPresentationBudgetMode.RealizationTime,
            maxRealizationTimePerPass: TimeSpan.FromMilliseconds(10),
            autoSchedule: false);

        var slowReady = new TestDeferredTarget
        {
            CanApply = true,
            ApplyDelay = TimeSpan.FromMilliseconds(25)
        };
        var firstReady = new TestDeferredTarget { CanApply = true };
        var secondReady = new TestDeferredTarget { CanApply = true };
        var targets = new[]
        {
            slowReady,
            firstReady,
            secondReady
        };

        try
        {
            foreach (var target in targets)
            {
                DeferredContentPresentationQueue.Enqueue(target);
            }

            Assert.Equal(3, DeferredContentPresentationQueue.PendingCount);

            DeferredContentPresentationQueue.FlushPendingBatchForTesting();

            Assert.Equal(1, slowReady.SuccessfulApplyCount);
            Assert.Equal(0, firstReady.SuccessfulApplyCount);
            Assert.Equal(0, secondReady.SuccessfulApplyCount);
            Assert.Equal(2, DeferredContentPresentationQueue.PendingCount);

            DeferredContentPresentationQueue.FlushPendingBatchForTesting();

            Assert.Equal(1, firstReady.SuccessfulApplyCount);
            Assert.Equal(1, secondReady.SuccessfulApplyCount);
            Assert.Equal(0, DeferredContentPresentationQueue.PendingCount);
        }
        finally
        {
            foreach (var target in targets)
            {
                DeferredContentPresentationQueue.Remove(target);
            }
        }
    }

    [AvaloniaFact]
    public void DocumentContentControl_Defers_Document_Template_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var buildCount = 0;
        var document = new Document
        {
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "Document" };
            })
        };
        var control = new DocumentContentControl
        {
            DataContext = document
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.Equal(0, buildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Document");

            DrainDeferredQueueBatch(window);

            Assert.InRange(buildCount, 1, 2);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Document");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolContentControl_Defers_Tool_Template_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var buildCount = 0;
        var tool = new Tool
        {
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "Tool" };
            })
        };
        var control = new ToolContentControl
        {
            DataContext = tool
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();

        try
        {
            Assert.Equal(0, buildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Tool");

            DrainDeferredQueueBatch(window);

            Assert.InRange(buildCount, 1, 2);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "Tool");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentControl_Defers_Active_Document_Template_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var factory = new Factory();
        var buildCount = 0;
        var dock = new DocumentDock
        {
            Factory = factory,
            LayoutMode = DocumentLayoutMode.Tabbed,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var document = new Document
        {
            Id = "doc-1",
            Title = "Doc 1",
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "DocumentControl" };
            })
        };
        dock.VisibleDockables!.Add(document);
        dock.ActiveDockable = document;

        var control = new DocumentControl
        {
            DataContext = dock
        };

        var window = ShowInWindow(control, new DockFluentTheme());
        var presenterHost = GetDeferredPresenter(control);

        try
        {
            Assert.Equal(0, buildCount);
            Assert.Null(presenterHost.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.InRange(buildCount, 1, 2);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Presenter!.Child);
            Assert.Equal("DocumentControl", textBlock.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolControl_Defers_Active_Tool_Template_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var factory = new Factory();
        var buildCount = 0;
        var dock = new ToolDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };

        var tool = new Tool
        {
            Id = "tool-1",
            Title = "Tool 1",
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "ToolControl" };
            })
        };
        dock.VisibleDockables!.Add(tool);
        dock.ActiveDockable = tool;

        var control = new ToolControl
        {
            DataContext = dock
        };

        var window = ShowInWindow(control, new DockFluentTheme());
        var presenterHost = GetDeferredPresenter(control);

        try
        {
            Assert.Equal(0, buildCount);
            Assert.Null(presenterHost.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.InRange(buildCount, 1, 2);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Presenter!.Child);
            Assert.Equal("ToolControl", textBlock.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MdiDocumentWindow_Defers_Window_Content_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var buildCount = 0;
        var document = new Document
        {
            Id = "doc-1",
            Title = "Doc 1",
            Content = (Func<IServiceProvider, object>)(_ =>
            {
                buildCount++;
                return new TextBlock { Text = "MdiDocumentWindow" };
            })
        };

        var control = new MdiDocumentWindow
        {
            DataContext = document
        };

        var window = ShowInWindow(control, new DockFluentTheme());
        var presenterHost = GetDeferredPresenter(control);

        try
        {
            Assert.Equal(0, buildCount);
            Assert.Null(presenterHost.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, buildCount);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Presenter!.Child);
            Assert.Equal("MdiDocumentWindow", textBlock.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SplitViewDockControl_Defers_Pane_And_Content_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var factory = new Factory();
        var paneDockable = new Tool
        {
            Id = "tool-1",
            Title = "Pane"
        };
        var contentDockable = new DocumentDock
        {
            Factory = factory,
            Id = "doc-dock-1",
            Title = "Content",
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var splitViewDock = new SplitViewDock
        {
            Factory = factory,
            PaneDockable = paneDockable,
            ContentDockable = contentDockable,
            IsPaneOpen = true
        };

        var paneBuildCount = 0;
        var contentBuildCount = 0;
        var control = new SplitViewDockControl
        {
            DataContext = splitViewDock
        };
        control.DataTemplates.Add(CreateCountingTemplate<Tool>("SplitPane", () => paneBuildCount++));
        control.DataTemplates.Add(CreateCountingTemplate<DocumentDock>("SplitContent", () => contentBuildCount++));

        var window = ShowInWindow(control);

        try
        {
            Assert.Equal(0, paneBuildCount);
            Assert.Equal(0, contentBuildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "SplitPane");
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "SplitContent");

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, paneBuildCount);
            Assert.Equal(1, contentBuildCount);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "SplitPane");
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "SplitContent");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PinnedDockControl_Defers_Pinned_Dock_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var factory = new Factory();
        var root = new RootDock
        {
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        var pinnedDock = new ToolDock
        {
            Alignment = Alignment.Left,
            Factory = factory,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        pinnedDock.VisibleDockables!.Add(new Tool { Id = "tool-1", Title = "Tool" });
        root.PinnedDock = pinnedDock;

        var buildCount = 0;
        var control = new PinnedDockControl
        {
            DataContext = root
        };
        control.DataTemplates.Add(CreateCountingTemplate<ToolDock>("PinnedDock", () => buildCount++));

        var window = ShowInWindow(control);

        try
        {
            Assert.Equal(0, buildCount);
            Assert.DoesNotContain(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "PinnedDock");

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, buildCount);
            Assert.Contains(control.GetVisualDescendants(), visual => visual is TextBlock textBlock && textBlock.Text == "PinnedDock");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DockControl_Defers_Layout_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var buildCount = 0;
        var root = new RootDock
        {
            Id = "root-1"
        };

        var control = new DockControl
        {
            AutoCreateDataTemplates = false,
            Layout = root
        };

        var window = ShowInWindow(control, new DockFluentTheme());
        var presenterHost = GetDeferredHost(control, "PART_ContentControl");
        presenterHost.DataTemplates.Add(CreateCountingTemplate<RootDock>("DockControl", () => buildCount++));

        try
        {
            Assert.Equal(0, buildCount);
            Assert.Null(presenterHost.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, buildCount);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Presenter!.Child);
            Assert.Equal("DockControl", textBlock.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void RootDockControl_Defers_Active_Dock_Materialization()
    {
        using var _ = new DeferredBatchLimitScope(autoSchedule: false);
        var factory = new Factory();
        var buildCount = 0;
        var activeDock = new DocumentDock
        {
            Factory = factory,
            Id = "doc-dock-1"
        };
        var root = new RootDock
        {
            Factory = factory,
            ActiveDockable = activeDock
        };

        var control = new RootDockControl
        {
            DataContext = root
        };

        var window = ShowInWindow(control, new DockFluentTheme());
        var presenterHost = GetDeferredHost(control, "PART_MainContent");
        presenterHost.DataTemplates.Add(CreateCountingTemplate<DocumentDock>("RootDockControl", () => buildCount++));

        try
        {
            Assert.Equal(0, buildCount);
            Assert.Null(presenterHost.Presenter?.Child);

            DrainDeferredQueueBatch(window);

            Assert.Equal(1, buildCount);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Presenter!.Child);
            Assert.Equal("RootDockControl", textBlock.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolChromeControl_Defers_Content_Materialization()
    {
        var factory = new Factory();
        var toolDock = new ToolDock
        {
            Factory = factory,
            ActiveDockable = new Tool { Id = "tool-1", Title = "Tool 1" }
        };
        var template = new CountingTemplate(() => new TextBlock { Text = "ToolChromeControl" });
        var control = new ToolChromeControl
        {
            DataContext = toolDock,
            Content = "First",
            ContentTemplate = template
        };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };
        window.Styles.Add(new DockFluentTheme());
        window.Show();
        control.ApplyTemplate();

        var presenterHost = GetDeferredPresenterHost(control, "PART_ContentPresenter");

        try
        {
            window.UpdateLayout();
            Assert.Equal(1, template.BuildCount);
            var initialTextBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("First", initialTextBlock.DataContext);

            control.Content = "Second";

            var staleTextBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("First", staleTextBlock.DataContext);

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(1, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void HostWindow_Defers_Content_Materialization()
    {
        var template = new CountingTemplate(() => new TextBlock { Text = "HostWindow" });
        var window = new HostWindow
        {
            Width = 800,
            Height = 600,
            Content = "First",
            ContentTemplate = template
        };
        window.Styles.Add(new DockFluentTheme());

        window.Show();
        window.ApplyTemplate();

        var presenterHost = GetDeferredPresenterHost(window, "PART_ContentPresenter");

        try
        {
            window.UpdateLayout();
            Assert.Equal(1, template.BuildCount);
            var initialTextBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("First", initialTextBlock.DataContext);

            window.Content = "Second";

            var staleTextBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("First", staleTextBlock.DataContext);

            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Assert.Equal(1, template.BuildCount);
            var textBlock = Assert.IsType<TextBlock>(presenterHost.Child);
            Assert.Equal("Second", textBlock.DataContext);
        }
        finally
        {
            window.Close();
        }
    }

    private static FuncDataTemplate<T> CreateCountingTemplate<T>(string text, Action onBuild)
    {
        return new FuncDataTemplate<T>(
            (_, _) =>
            {
                onBuild();
                return new TextBlock { Text = text };
            },
            true);
    }

    private static DeferredContentControl GetDeferredPresenter(Control control)
    {
        return GetDeferredHost(control, "PART_ContentPresenter");
    }

    private static DeferredContentControl GetDeferredHost(Visual visual, string name)
    {
        var presenter = visual.GetVisualDescendants()
            .OfType<DeferredContentControl>()
            .FirstOrDefault(candidate => candidate.Name == name);
        Assert.NotNull(presenter);
        return presenter!;
    }

    private static DeferredContentPresenter GetDeferredPresenterHost(Visual visual, string name)
    {
        var presenter = visual.GetVisualDescendants()
            .OfType<DeferredContentPresenter>()
            .FirstOrDefault(candidate => candidate.Name == name);
        Assert.NotNull(presenter);
        return presenter!;
    }

    private static Window ShowInWindow(Control control, params IStyle[] styles)
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        foreach (var style in styles)
        {
            window.Styles.Add(style);
        }

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();
        return window;
    }

    private static void DrainDeferredQueueBatch(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        DeferredContentPresentationQueue.FlushPendingBatchForTesting();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private sealed class DeferredBatchLimitScope : IDisposable
    {
        private readonly DeferredContentPresentationBudgetMode _previousBudgetMode = DeferredContentPresentationSettings.BudgetMode;
        private readonly int _previousLimit = DeferredContentPresentationSettings.MaxPresentationsPerPass;
        private readonly TimeSpan _previousMaxRealizationTime = DeferredContentPresentationSettings.MaxRealizationTimePerPass;
        private readonly bool _previousAutoSchedule = DeferredContentPresentationQueue.AutoSchedule;

        public DeferredBatchLimitScope(
            DeferredContentPresentationBudgetMode budgetMode = DeferredContentPresentationBudgetMode.ItemCount,
            int limit = int.MaxValue,
            TimeSpan? maxRealizationTimePerPass = null,
            bool autoSchedule = true)
        {
            DeferredContentPresentationSettings.BudgetMode = budgetMode;
            DeferredContentPresentationSettings.MaxPresentationsPerPass = limit;
            DeferredContentPresentationSettings.MaxRealizationTimePerPass = maxRealizationTimePerPass ?? TimeSpan.FromMilliseconds(10);
            DeferredContentPresentationQueue.AutoSchedule = autoSchedule;
        }

        public void Dispose()
        {
            DeferredContentPresentationSettings.BudgetMode = _previousBudgetMode;
            DeferredContentPresentationSettings.MaxPresentationsPerPass = _previousLimit;
            DeferredContentPresentationSettings.MaxRealizationTimePerPass = _previousMaxRealizationTime;
            DeferredContentPresentationQueue.AutoSchedule = _previousAutoSchedule;
        }
    }
}
