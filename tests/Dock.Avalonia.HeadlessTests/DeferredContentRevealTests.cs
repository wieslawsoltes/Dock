using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Controls.DeferredContentControl;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DeferredContentRevealTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Repeated_Reveals_Finish_At_Full_Opacity_After_Transitions_Settle(bool customTransition)
    {
        DeferredContentPresentationTimeline timeline = new() { AutoSchedule = false, RevealDuration = TimeSpan.FromMilliseconds(40) };
        DeferredContentControl host = new()
        {
            Content = "First",
            ContentTemplate = new FuncDataTemplate<string>((_, _) => new Border { Background = Brushes.Magenta })
        };
        Window window = new() { Width = 400, Height = 300, Content = host };
        DeferredContentScheduling.SetTimeline(window, timeline);
        try
        {
            window.Show();
            Flush(window, timeline);
            Control presenter = Assert.IsAssignableFrom<Control>(host.Presenter);
            DoubleTransition? suppliedTransition = null;
            if (customTransition)
            {
                suppliedTransition = new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromMilliseconds(60) };
                presenter.Transitions = new Transitions { suppliedTransition };
            }

            for (int visit = 0; visit < 8; visit++)
            {
                host.Content = visit % 2 == 0 ? "Second" : "First";
                Flush(window, timeline);
                Assert.NotNull(presenter.Transitions);
                Assert.True(presenter.IsAnimating(Visual.OpacityProperty), "Content replacement must retain the reveal animation.");
                await SettleAsync(window, presenter);

                Assert.Equal(1D, presenter.Opacity);
                Assert.Equal(1D, presenter.GetBaseValue(Visual.OpacityProperty).GetValueOrDefault(1D));
                if (suppliedTransition is not null)
                {
                    Assert.Same(suppliedTransition, Assert.Single(presenter.Transitions!));
                    Assert.Equal(TimeSpan.FromMilliseconds(60), suppliedTransition.Duration);
                }
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Browser_Sample_Tab_Clicks_Keep_Rendered_Content_Color_After_Every_Reveal()
    {
        Factory factory = new();
        Document first = new() { Title = "First", Content = (Func<IServiceProvider, object>)(_ => new Border { Background = Brushes.Magenta }) };
        Document second = new() { Title = "Second", Content = (Func<IServiceProvider, object>)(_ => new Border { Background = Brushes.Magenta }) };
        DocumentDock dock = new()
        {
            IsCollapsable = false, VisibleDockables = factory.CreateList<IDockable>(first, second), ActiveDockable = first
        };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(dock), ActiveDockable = dock, DefaultDockable = dock };
        factory.InitLayout(root);
        using BrowserTabTheme.MainWindowViewModel viewModel = new(factory, root);
        BrowserTabTheme.MainWindow window = new() { DataContext = viewModel, Width = 600, Height = 400 };
        window.Styles.Add(new global::Dock.Avalonia.Themes.Browser.BrowserTabTheme());
        DeferredContentPresentationTimeline timeline = new() { AutoSchedule = false, RevealDuration = TimeSpan.FromMilliseconds(40) };
        DeferredContentScheduling.SetTimeline(window, timeline);
        try
        {
            window.Show();
            Flush(window, timeline);
            DeferredContentControl host = window.GetVisualDescendants().OfType<DeferredContentControl>()
                .Single(control => control.Name == "PART_ContentPresenter");
            Control presenter = Assert.IsAssignableFrom<Control>(host.Presenter);
            await SettleAsync(window, presenter);
            AssertMagentaPixel(window, presenter);

            for (int visit = 0; visit < 8; visit++)
            {
                if (visit == 4)
                {
                    factory.MoveDockable(dock, first, second);
                    Flush(window, timeline);
                    Assert.Same(second, dock.VisibleDockables![0]);
                }
                Document next = visit % 2 == 0 ? second : first;
                DocumentTabStripItem tab = window.GetVisualDescendants().OfType<DocumentTabStripItem>()
                    .Single(item => ReferenceEquals(item.DataContext, next));
                Point center = tab.TranslatePoint(new Point(tab.Bounds.Width / 2, tab.Bounds.Height / 2), window)!.Value;
                window.MouseDown(center, MouseButton.Left);
                window.MouseUp(center, MouseButton.Left);
                Flush(window, timeline);
                Assert.Same(next, dock.ActiveDockable);
                await SettleAsync(window, presenter);
                AssertMagentaPixel(window, presenter);
                Assert.Equal(1D, presenter.Opacity);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, 0.4)]
    [InlineData(true, 0.4)]
    [InlineData(false, 0.85)]
    [InlineData(true, 0.85)]
    public async Task InFlight_Reveal_Does_Not_Overwrite_A_Newer_Opacity_Binding(bool detach, double opacity)
    {
        Border presenter = new() { Background = Brushes.Magenta };
        Border opacitySource = new() { Opacity = opacity };
        Window window = new() { Width = 200, Height = 100, Content = presenter };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            using IDisposable? reveal = DeferredContentPresentationTargetHelpers.ApplyRevealAnimation(presenter, TimeSpan.FromMilliseconds(40), true);
            if (detach) window.Content = null;
            using IDisposable binding = presenter.Bind(Visual.OpacityProperty, opacitySource.GetObservable(Visual.OpacityProperty));
            if (detach) window.Content = presenter;

            Dispatcher.UIThread.RunJobs();

            Assert.Equal(opacity, presenter.GetBaseValue(Visual.OpacityProperty).Value);
            opacitySource.Opacity = 0.6;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0.6, presenter.GetBaseValue(Visual.OpacityProperty).Value);
            await SettleAsync(window, presenter);
            Assert.Equal(0.6, presenter.Opacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0.3)]
    [InlineData(0.85)]
    public async Task InFlight_Reveal_Does_Not_Overwrite_A_Newer_Local_Opacity(double opacity)
    {
        Border presenter = new();
        Window window = new() { Width = 200, Height = 100, Content = presenter };
        try
        {
            window.Show();
            using IDisposable? reveal = DeferredContentPresentationTargetHelpers.ApplyRevealAnimation(presenter, TimeSpan.FromMilliseconds(40), true);
            presenter.Opacity = opacity;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(opacity, presenter.GetBaseValue(Visual.OpacityProperty).Value);
            await SettleAsync(window, presenter);
            Assert.Equal(opacity, presenter.Opacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task Clearing_An_InFlight_Reveal_Cancels_Its_Endpoint_And_Allows_Reuse()
    {
        DeferredContentPresentationTimeline timeline = new() { AutoSchedule = false, RevealDuration = TimeSpan.FromMilliseconds(40) };
        DeferredContentControl host = new()
        {
            Content = "First", ContentTemplate = new FuncDataTemplate<string>((_, _) => new Border())
        };
        Window window = new() { Width = 200, Height = 100, Content = host };
        DeferredContentScheduling.SetTimeline(window, timeline);
        try
        {
            window.Show();
            Flush(window, timeline);
            Control presenter = Assert.IsAssignableFrom<Control>(host.Presenter);
            host.Content = "Second";
            timeline.FlushPendingBatchForTesting();
            host.ClearPresentation();
            presenter.Opacity = 0.4;
            Dispatcher.UIThread.RunJobs();
            Assert.Null(host.Presenter!.Child);
            Assert.Equal(0.4, presenter.GetBaseValue(Visual.OpacityProperty).Value);

            window.Content = null;
            window.Content = host;
            Flush(window, timeline);
            host.Content = "Third";
            timeline.FlushPendingBatchForTesting();
            host.Content = "Fourth";
            timeline.FlushPendingBatchForTesting();
            Flush(window, timeline);
            await SettleAsync(window, presenter);
            Assert.Equal("Fourth", host.Presenter.Child!.DataContext);
            Assert.Equal(0.4, presenter.Opacity);
            Assert.Equal(0.4, presenter.GetBaseValue(Visual.OpacityProperty).Value);
        }
        finally { window.Close(); }
    }

    private static void Flush(Window window, DeferredContentPresentationTimeline timeline)
    {
        for (int pass = 0; pass < 20; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            timeline.FlushPendingBatchForTesting();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            if (timeline.PendingCount == 0) return;
        }
        Assert.Equal(0, timeline.PendingCount);
    }

    private static async Task SettleAsync(Window window, Control presenter)
    {
        // An effective value of 1 can be a temporary animation value. Observe the
        // completed transition, including enough clock ticks to start it first.
        Stopwatch elapsed = Stopwatch.StartNew();
        do
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            await Task.Delay(10);
        }
        while (elapsed.Elapsed < TimeSpan.FromMilliseconds(150)
               || (presenter.IsAnimating(Visual.OpacityProperty) && elapsed.Elapsed < TimeSpan.FromSeconds(2)));
        Assert.False(presenter.IsAnimating(Visual.OpacityProperty));
    }

    private static void AssertMagentaPixel(Window window, Control presenter)
    {
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        Point center = presenter.TranslatePoint(new Point(presenter.Bounds.Width / 2, presenter.Bounds.Height / 2), window)!.Value;
        int x = (int)(center.X * window.RenderScaling);
        int y = (int)(center.Y * window.RenderScaling);
        byte[] pixel = new byte[4];
        GCHandle pinned = GCHandle.Alloc(pixel, GCHandleType.Pinned);
        try { frame.CopyPixels(new PixelRect(x, y, 1, 1), pinned.AddrOfPinnedObject(), pixel.Length, 4); }
        finally { pinned.Free(); }
        Assert.Equal(new byte[] { 255, 0, 255, 255 }, pixel);
    }
}
