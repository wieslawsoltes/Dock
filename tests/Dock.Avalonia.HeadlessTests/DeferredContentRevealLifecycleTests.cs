// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Controls.DeferredContentControl;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

// Shared with the Avalonia 11 suite; exercise the public controls and scheduling API.
public class DeferredContentRevealLifecycleTests
{
    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public async Task Every_Reveal_Fades_Up_To_The_Caller_Opacity(bool standardPresenter, int opacitySource)
    {
        DeferredContentControl host = CreateHost(standardPresenter);
        Window window = CreateWindow(host, TimeSpan.FromMilliseconds(300));
        IDisposable? binding = null;
        try
        {
            window.Show();
            Flush(window);
            ContentPresenter presenter = Assert.IsAssignableFrom<ContentPresenter>(host.Presenter);
            double opacity = opacitySource == 0 ? 1D : 0.6D;
            if (opacitySource == 1)
                presenter.Opacity = opacity;
            else if (opacitySource == 2)
                presenter.Styles.Add(new Style { Setters = { new Setter(Visual.OpacityProperty, opacity) } });
            else if (opacitySource == 3)
            {
                Border source = new() { Opacity = opacity };
                binding = presenter.Bind(Visual.OpacityProperty, source.GetObservable(Visual.OpacityProperty));
            }
            Flush(window);
            int styleCount = presenter.Styles.Count;

            for (int visit = 0; visit < 3; visit++)
            {
                host.Content = $"Visit {visit}";
                Flush(window);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                double start = presenter.Opacity;
                Assert.InRange(start, opacity * 0.85D, opacity * 0.98D);
                Assert.Equal(opacity, presenter.GetBaseValue(Visual.OpacityProperty).GetValueOrDefault(1D));

                await Task.Delay(30);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Assert.InRange(presenter.Opacity, start, opacity);
                await SettleAsync(window, presenter);
                Assert.Equal(opacity, presenter.Opacity);
                Assert.Equal(styleCount, presenter.Styles.Count);
                Assert.Null(presenter.Transitions);
            }
        }
        finally
        {
            binding?.Dispose();
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Revealing_Content_Does_Not_Restart_A_Caller_Style_Animation(bool standardPresenter)
    {
        DeferredContentControl host = CreateHost(standardPresenter);
        Window window = CreateWindow(host, TimeSpan.FromMilliseconds(40));
        try
        {
            window.Show();
            Flush(window);
            ContentPresenter presenter = Assert.IsAssignableFrom<ContentPresenter>(host.Presenter);
            presenter.Styles.Add(new Style
            {
                Animations =
                {
                    new Animation
                    {
                        Duration = TimeSpan.FromSeconds(2),
                        Children =
                        {
                            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(Control.WidthProperty, 100D) } },
                            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(Control.WidthProperty, 200D) } }
                        }
                    }
                }
            });
            Flush(window);
            for (int tick = 0; tick < 10; tick++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                await Task.Delay(10);
            }
            double previousWidth = presenter.Width;
            Assert.InRange(previousWidth, 102D, 199D);

            for (int visit = 0; visit < 3; visit++)
            {
                host.Content = $"Visit {visit}";
                Flush(window);
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                Assert.InRange(presenter.Width, previousWidth, 200D);
                previousWidth = presenter.Width;
                await Task.Delay(10);
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Caller_Opacity_Binding_Survives_Reveal_Clear_And_Reuse(bool standardPresenter, bool skipReveal)
    {
        DeferredContentControl host = CreateHost(standardPresenter);
        Window window = CreateWindow(host, skipReveal ? TimeSpan.Zero : TimeSpan.FromMilliseconds(40));
        try
        {
            window.Show();
            Flush(window);
            ContentPresenter presenter = Assert.IsAssignableFrom<ContentPresenter>(host.Presenter);
            Border source = new() { Opacity = 0.4 };
            using IDisposable binding = presenter.Bind(Visual.OpacityProperty, source.GetObservable(Visual.OpacityProperty));

            host.Content = "Second";
            Flush(window);
            Assert.Equal(0.4, presenter.GetBaseValue(Visual.OpacityProperty).Value);
            await SettleAsync(window, presenter);
            Assert.Equal(0.4, presenter.Opacity);

            host.ClearPresentation();
            Assert.Null(presenter.Child);
            Assert.Equal(0.4, presenter.GetBaseValue(Visual.OpacityProperty).Value);
            source.Opacity = 0.6;
            host.Content = "Third";
            Flush(window);
            await SettleAsync(window, presenter);
            Assert.Equal("Third", presenter.Child!.DataContext);
            Assert.Equal(0.6, presenter.Opacity);
            Assert.Equal(0.6, presenter.GetBaseValue(Visual.OpacityProperty).Value);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void Clear_Detach_And_Template_Replacement_Cancel_The_Old_Reveal(bool standardPresenter, int action)
    {
        DeferredContentControl host = CreateHost(standardPresenter);
        Window window = CreateWindow(host, TimeSpan.FromSeconds(30));
        try
        {
            window.Show();
            Flush(window);
            ContentPresenter presenter = Assert.IsAssignableFrom<ContentPresenter>(host.Presenter);
            host.Content = "Second";
            Flush(window);
            Assert.True(presenter.IsAnimating(Visual.OpacityProperty));

            if (action == 0)
                host.ClearPresentation();
            else if (action == 1)
                window.Content = null;
            else
            {
                host.Template = CreateTemplate(!standardPresenter);
                Flush(window);
                Assert.NotSame(presenter, host.Presenter);
            }

            Assert.False(presenter.IsAnimating(Visual.OpacityProperty));
            Assert.Equal(1D, presenter.Opacity);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, 40)]
    [InlineData(true, 40)]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    public async Task A_Later_Reveal_Cancels_The_Previous_Animation_And_Preserves_Custom_Transition(bool standardPresenter, int duration)
    {
        DeferredContentControl host = CreateHost(standardPresenter);
        Window window = CreateWindow(host, TimeSpan.FromSeconds(30));
        try
        {
            window.Show();
            Flush(window);
            ContentPresenter presenter = Assert.IsAssignableFrom<ContentPresenter>(host.Presenter);
            DoubleTransition transition = new()
            {
                Property = Visual.OpacityProperty, Duration = TimeSpan.FromSeconds(30),
                Delay = TimeSpan.FromMilliseconds(10), Easing = new QuadraticEaseIn()
            };
            presenter.Transitions = new Transitions { transition };
            host.Content = "Second";
            Flush(window);
            // The caller-supplied delay defers the first animation value until clock ticks.
            for (int tick = 0; tick < 4; tick++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                await Task.Delay(10);
            }
            Assert.True(presenter.IsAnimating(Visual.OpacityProperty));

            transition.Duration = TimeSpan.FromMilliseconds(duration);
            host.Content = "Third";
            Flush(window);
            await SettleAsync(window, presenter);

            Assert.Same(transition, Assert.Single(presenter.Transitions));
            Assert.Equal(TimeSpan.FromMilliseconds(duration), transition.Duration);
            Assert.Equal(TimeSpan.FromMilliseconds(10), transition.Delay);
            Assert.IsType<QuadraticEaseIn>(transition.Easing);
            Assert.Equal("Third", presenter.Child!.DataContext);
            Assert.Equal(1D, presenter.Opacity);
            Assert.Equal(1D, presenter.GetBaseValue(Visual.OpacityProperty).GetValueOrDefault(1D));
        }
        finally { window.Close(); }
    }

    private static DeferredContentControl CreateHost(bool standardPresenter) => new()
    {
        Content = "First",
        ContentTemplate = new FuncDataTemplate<string>((_, _) => new Border()),
        Template = CreateTemplate(standardPresenter)
    };

    private static FuncControlTemplate<DeferredContentControl> CreateTemplate(bool standardPresenter) =>
        new((_, names) => (standardPresenter
            ? new ContentPresenter { Name = "PART_ContentPresenter" }
            : new DeferredContentPresenter { Name = "PART_ContentPresenter" }).RegisterInNameScope(names));

    private static Window CreateWindow(DeferredContentControl host, TimeSpan revealDuration)
    {
        Window window = new() { Width = 200, Height = 100, Content = host };
        DeferredContentScheduling.SetTimeline(window, new DeferredContentPresentationTimeline { RevealDuration = revealDuration });
        return window;
    }

    private static void Flush(Window window)
    {
        for (int pass = 0; pass < 5; pass++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static async Task SettleAsync(Window window, Control presenter)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        do
        {
            Flush(window);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            await Task.Delay(10);
        }
        while (elapsed.Elapsed < TimeSpan.FromMilliseconds(150)
            || (presenter.IsAnimating(Visual.OpacityProperty) && elapsed.Elapsed < TimeSpan.FromSeconds(2)));
        Assert.False(presenter.IsAnimating(Visual.OpacityProperty));
    }
}
