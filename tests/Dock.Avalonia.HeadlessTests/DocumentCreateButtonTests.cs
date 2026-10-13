using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Rendering.Composition;
using Orientation = Avalonia.Layout.Orientation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentCreateButtonTests
{
    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal, false)]
    [InlineData(Orientation.Vertical, false)]
    [InlineData(Orientation.Horizontal, true)]
    [InlineData(Orientation.Vertical, true)]
    public void Create_Button_Renders_And_Executes_Without_Parenting_The_Model(Orientation orientation, bool custom)
    {
        Factory factory = new();
        RootDock root = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>() };
        DocumentDock model = new()
        {
            Factory = factory,
            CanCreateDocument = true,
            VisibleDockables = factory.CreateList<IDockable>(),
            DocumentTemplate = new DocumentTemplate { Content = (Func<IServiceProvider, object>)(_ => new TextBlock()) }
        };
        factory.AddDockable(root, model);
        DocumentTabStrip strip = new() { DataContext = model, ItemsSource = model.VisibleDockables, Orientation = orientation };
        if (custom)
            strip.CreateButtonTemplate = new FuncDataTemplate<DocumentDock>((dock, _) => new Button { Content = "Add", Command = dock!.CreateDocument });
        Window window = new() { Content = strip, Width = 500, Height = 300 };
        try
        {
            window.Show(); Update(window);
            ContentPresenter host = strip.GetVisualDescendants().OfType<ContentPresenter>().Single(control => control.Name == "PART_CreateButtonHost");
            Assert.True(host.IsVisible);
            Assert.Same(strip.CreateButtonTemplate, host.ContentTemplate);
            Assert.Null(model.Parent);
            Button button = Assert.Single(host.GetVisualDescendants().OfType<Button>());
            Assert.Same(model, button.DataContext);
            Point point = button.TranslatePoint(new Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left); window.MouseUp(point, MouseButton.Left);
            Update(window);
            Assert.Single(model.VisibleDockables!);
            model.CanCreateDocument = false;
            Update(window);
            Assert.False(host.IsVisible);
            window.Content = null;
            Update(window);
            Assert.Null(model.Parent);
            window.Content = strip;
            model.CanCreateDocument = true;
            Update(window);
            Assert.True(host.IsVisible);
            Assert.Null(model.Parent);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Retaining_The_Model_Does_Not_Retain_A_Removed_Create_Button_Host(bool visible)
    {
        var result = Detach(visible);
        // Closing the window can leave a compositor batch and its dispatcher
        // continuation holding the old scene. Wait for an acknowledged render
        // before using the weak reference to diagnose model-owned retention.
        Task rendered = result.Compositor.RequestCompositionBatchCommitAsync().Rendered;
        Stopwatch deadline = Stopwatch.StartNew();
        while (!rendered.IsCompleted && deadline.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            if (!rendered.IsCompleted) await Task.Delay(10);
        }
        Assert.True(rendered.IsCompletedSuccessfully, "The headless compositor did not finish its cleanup batch.");
        for (int i = 0; i < 20; i++)
        {
            Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            // A render commit can enqueue cleanup after the timer tick. Allow
            // the render thread and its dispatcher continuation to finish.
            Dispatcher.UIThread.RunJobs();
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            if (!IsAlive(result.Strip)) break;
            await Task.Delay(10);
        }
        Assert.False(IsAlive(result.Strip));
        GC.KeepAlive(result.Model);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAlive(WeakReference<Control> reference) => reference.TryGetTarget(out _);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DocumentDock Model, WeakReference<Control> Strip, Compositor Compositor) Detach(bool visible)
    {
        DocumentDock model = new() { CanCreateDocument = visible };
        DocumentTabStrip strip = new() { DataContext = model };
        Window window = new() { Content = strip, Width = 500, Height = 300 };
        try
        {
            window.Show(); Update(window);
            Compositor compositor = ElementComposition.GetElementVisual(window)!.Compositor;
            window.Content = new Border();
            Update(window);
            window.CaptureRenderedFrame()?.Dispose();
            Assert.Null(model.Parent);
            return (model, new(strip), compositor);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void Update(Window window) { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
}
