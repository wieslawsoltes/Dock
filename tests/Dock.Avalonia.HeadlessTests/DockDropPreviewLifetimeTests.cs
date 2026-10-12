using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewLifetimeTests
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

    [AvaloniaTheory]
    [InlineData(1)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void First_split_style_probe_is_collected_when_dock_control_is_detached(double scale)
    {
        var window = new ScaledWindow { LayoutScaling = scale, Width = 1000, Height = 600 };
        window.Styles.Add(new Style(x => x.OfType<ProportionalStackPanelSplitter>())
        {
            Setters = { new Setter(ProportionalStackPanelSplitter.ThicknessProperty, 20d) }
        });
        try
        {
            var (control, probe) = CreatePreviewAndDetach(window);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using var frame = window.CaptureRenderedFrame();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.False(control.IsAlive, "The detached DockControl must not be retained by its former style host.");
            Assert.False(probe.IsAlive, "The nonvisual style probe must not be retained by the live window.");
            GC.KeepAlive(window);
        }
        finally { window.Close(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference control, WeakReference probe) CreatePreviewAndDetach(Window window)
    {
        var (_, root, panes) = DockDropPreviewTests.CreateLayout(1.0);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        var control = new DockControl { Layout = root };
        window.Content = control;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        var service = new DockDropPreviewService();
        Assert.NotNull(service.GetBounds(sources[0].ActiveDockable!, panes[0], DockOperation.Right, control, double.NaN));
        Assert.Empty(control.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>());
        var probe = Assert.Single(control.GetLogicalChildren().OfType<ProportionalStackPanelSplitter>());
        Assert.Equal(20, probe.Thickness);
        var result = (new WeakReference(control), new WeakReference(probe));
#if AVALONIA_11
        window.FocusManager?.ClearFocus();
#else
        window.FocusManager?.Focus(null);
#endif
        window.Content = null;
        return result;
    }
}
