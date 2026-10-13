using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideWindowTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void The_Public_Window_Wraps_The_Native_Preview_And_Existing_Item_View(AnchorSide side)
    {
        using Workspace workspace = new();
        LayoutAutoHideWindowControl preview = workspace.Preview;
        Assert.Null(preview.Model);
        Assert.False(preview.IsVisible);
        LayoutAnchorable tool = workspace.Tool(side);
        Control view = workspace.Manager.GetLayoutItemFromModel(tool)!.View;
        workspace.Show(tool);
        Assert.Same(tool, preview.Model);
        Assert.Same(tool, ((ILayoutControl)preview).Model);
        Assert.Same(tool, preview.Tag);
        Assert.Same(tool.DockModel, workspace.Manager.DockLayout.PinnedDock!.ActiveDockable);
        Assert.True(preview.IsVisible);
        Assert.Contains(preview, workspace.Manager.GetVisualDescendants());
        Assert.Contains(view, preview.GetVisualDescendants());
        Assert.Same(tool, Assert.Single(preview.GetVisualDescendants().OfType<LayoutAnchorableControl>()).Model);
        Assert.True(preview.Bounds.Width > 0 && preview.Bounds.Height > 0);
        workspace.Manager.DockFactory.HidePreviewingDockables(workspace.Manager.DockLayout);
        workspace.Update();
        Assert.Null(preview.Model);
        Assert.Null(preview.Tag);
        Assert.False(preview.IsVisible);
        Assert.Null(workspace.Manager.DockLayout.PinnedDock);
    }

    [AvaloniaTheory]
    [InlineData("AutoHideContent", "Styled preview")]
    [InlineData("LegacyAutoHideContent", "Legacy styled preview")]
    public void Preview_Background_And_Anchorable_Theme_Change_Without_Replacing_Content(string key, string tag)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool();
        Control view = workspace.Manager.GetLayoutItemFromModel(tool)!.View;
        workspace.Show(tool);
        LayoutAutoHideWindowControl preview = workspace.Preview;
        Assert.Equal(Colors.Coral, Assert.IsAssignableFrom<ISolidColorBrush>(preview.Presenter!.Background).Color);
        preview.Background = Brushes.Teal;
        ControlTheme theme = (ControlTheme)workspace.Window.Resources[key]!;
        preview.AnchorableStyle = theme;
        workspace.Update();
        Assert.Equal(Colors.Teal, Assert.IsAssignableFrom<ISolidColorBrush>(preview.Presenter.Background).Color);
        DockableControl body = Assert.IsType<DockableControl>(preview.Content);
        Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(body.Background).Color);
        LayoutAnchorableControl content = Assert.Single(preview.GetVisualDescendants().OfType<LayoutAnchorableControl>());
        Assert.Same(theme, content.Theme);
        Assert.Equal(tag, content.Tag);
        Assert.Contains(view, content.GetVisualDescendants());
        preview.AnchorableStyle = null;
        workspace.Update();
        Assert.Null(content.Theme);
        Assert.Null(content.Tag);
        Assert.Contains(view, content.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Switching_Anchors_Reuses_The_Public_Window_And_Native_Ownership()
    {
        using Workspace workspace = new();
        LayoutAnchorable first = workspace.Tool();
        LayoutAnchorable second = workspace.Tool(AnchorSide.Right);
        LayoutAutoHideWindowControl preview = workspace.Preview;
        workspace.Show(first);
        workspace.Show(second);
        Assert.Same(preview, workspace.Preview);
        Assert.Same(second, preview.Model);
        Assert.Same(second.DockModel, workspace.Manager.DockLayout.PinnedDock!.ActiveDockable);
        Assert.Contains(workspace.Manager.GetLayoutItemFromModel(second)!.View, preview.GetVisualDescendants());
        Assert.DoesNotContain(workspace.Manager.GetLayoutItemFromModel(first)!.View, preview.GetVisualDescendants());
        Assert.Same(workspace.Manager.Layout.LeftSide!.Children[0], first.Parent);
        Assert.True(first.IsAutoHidden);
    }

    [AvaloniaFact]
    public async Task An_Idle_Passive_Preview_Closes_After_The_Configured_Delay()
    {
        using Workspace workspace = new();
        workspace.Manager.AutoHideWindowClosingTimer = 10000;
        LayoutDocument document = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Single();
        document.IsActive = true;
        LayoutAnchorable tool = workspace.Tool();
        workspace.Show(tool);
        workspace.MoveAway();
        Assert.Same(document, workspace.Manager.Layout.ActiveContent);
        TimeSpan elapsed = await workspace.ObserveClosing(() => workspace.Manager.AutoHideWindowClosingTimer = 100);
        Assert.True(elapsed.TotalMilliseconds >= 99, $"The configured 100 ms delay ended after {elapsed.TotalMilliseconds} ms.");
        Assert.Null(workspace.Preview.Model);
        Assert.Null(workspace.Manager.DockLayout.PinnedDock);
        Assert.Same(document, workspace.Manager.Layout.ActiveContent);
        Assert.True(tool.IsAutoHidden);
    }

    [AvaloniaTheory]
    [InlineData("active")]
    [InlineData("anchor")]
    [InlineData("preview")]
    [InlineData("keepVisible")]
    public async Task Interaction_Keeps_A_Preview_Open_And_Leaving_Restarts_Idle_Closing(string hold)
    {
        using Workspace workspace = new();
        workspace.Manager.AutoHideWindowClosingTimer = 100;
        LayoutAnchorable tool = workspace.Tool();
        workspace.Show(tool);
        workspace.MoveAway();
        if (hold == "active") tool.IsActive = true;
        if (hold == "keepVisible") tool.DockModel.KeepPinnedDockableVisible = true;
        if (hold == "anchor") workspace.Window.MouseMove(workspace.Center(workspace.Manager.LeftSidePanel!.Children[0].Children[0]));
        if (hold == "preview") workspace.Window.MouseMove(workspace.Center(workspace.Preview));
        await workspace.Wait(80); await workspace.Wait(150);
        Assert.Same(tool, workspace.Preview.Model);
        workspace.MoveAway();
        if (hold == "active") tool.IsActive = false;
        if (hold == "keepVisible") tool.DockModel.KeepPinnedDockableVisible = false;
        await workspace.WaitUntilClosed();
        Assert.Null(workspace.Preview.Model);
    }

    [AvaloniaFact]
    public async Task A_Live_Delay_Change_Updates_The_Pending_Close_Deadline()
    {
        using Workspace workspace = new();
        Assert.Equal(1500, workspace.Manager.AutoHideWindowClosingTimer);
        workspace.Manager.AutoHideWindowClosingTimer = -20;
        Assert.Equal(1500, workspace.Manager.AutoHideWindowClosingTimer);
        workspace.Manager.AutoHideWindowClosingTimer = 10000;
        workspace.Show(workspace.Tool()); workspace.MoveAway();
        await workspace.Wait(80);
        Assert.NotNull(workspace.Preview.Model);
        await workspace.ObserveClosing(() => workspace.Manager.AutoHideWindowClosingTimer = 0);
        Assert.Null(workspace.Preview.Model);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Suppressing_Or_Throwing_From_The_Delay_Callback_Preserves_The_Applied_Interval(bool fail)
    {
        using Workspace workspace = new(probe: true);
        ManagerProbe manager = (ManagerProbe)workspace.Manager;
        manager.AutoHideWindowClosingTimer = int.MaxValue;
        manager.SuppressTimer = true;
        manager.FailTimer = fail;
        if (fail) Assert.Throws<InvalidOperationException>(() => manager.AutoHideWindowClosingTimer = 0);
        else manager.AutoHideWindowClosingTimer = 0;
        Assert.Equal(0, manager.AutoHideWindowClosingTimer);
        workspace.Show(workspace.Tool()); workspace.MoveAway();
        await workspace.Wait(150);
        Assert.NotNull(workspace.Preview.Model);
        await workspace.ObserveClosing(() => manager.ApplyTimerChange(manager.LastTimerChange!));
        Assert.Null(workspace.Preview.Model);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Redirecting_A_Delay_Change_Respects_The_Order_Of_Base_Callbacks(bool afterBase)
    {
        using Workspace workspace = new(probe: true);
        ManagerProbe manager = (ManagerProbe)workspace.Manager;
        manager.AutoHideWindowClosingTimer = int.MaxValue;
        manager.TimerCallback = (args, after) =>
        {
            if (after == afterBase && Equals(args.NewValue, 0)) manager.AutoHideWindowClosingTimer = int.MaxValue;
        };
        manager.AutoHideWindowClosingTimer = 0;
        Assert.Equal(int.MaxValue, manager.AutoHideWindowClosingTimer);
        workspace.Show(workspace.Tool()); workspace.MoveAway();
        if (afterBase)
        {
            await workspace.Wait(150);
            Assert.NotNull(workspace.Preview.Model);
        }
        else
        {
            await workspace.WaitUntilClosed();
            Assert.Null(workspace.Preview.Model);
        }
    }

    [AvaloniaFact]
    public async Task Replaying_A_Delay_Callback_Uses_Its_Argument_While_The_Property_Remains_Unchanged()
    {
        using Workspace workspace = new(probe: true);
        ManagerProbe manager = (ManagerProbe)workspace.Manager;
        manager.AutoHideWindowClosingTimer = 0;
        AvaloniaPropertyChangedEventArgs captured = manager.LastTimerChange!;
        manager.AutoHideWindowClosingTimer = int.MaxValue;
        workspace.Show(workspace.Tool()); workspace.MoveAway();
        await workspace.Wait(80);
        await workspace.ObserveClosing(() => manager.ApplyTimerChange(captured));
        Assert.Null(workspace.Preview.Model);
        Assert.Equal(int.MaxValue, manager.AutoHideWindowClosingTimer);
    }

    [AvaloniaFact]
    public async Task Initial_Timer_Setup_Reads_The_Current_Property_Then_Retains_The_Applied_Interval()
    {
        DelayedManagerProbe manager = new() { SuppressTimer = true, AutoHideWindowClosingTimer = 0 };
        using Workspace workspace = new(configuredManager: manager);
        await workspace.ObserveClosing(() => { workspace.Show(workspace.Tool()); workspace.MoveAway(); });
        manager.AutoHideWindowClosingTimer = int.MaxValue;
        await workspace.ObserveClosing(() =>
        {
            workspace.Tool().IsSelected = false;
            workspace.Show(workspace.Tool()); workspace.MoveAway();
        });
        Assert.Null(workspace.Preview.Model);
        Assert.Equal(int.MaxValue, manager.AutoHideWindowClosingTimer);
    }

    [AvaloniaTheory]
    [InlineData("unpin")]
    [InlineData("hide")]
    [InlineData("float")]
    [InlineData("layout")]
    [InlineData("xml")]
    public void Native_And_Public_Layout_Changes_Clear_The_Preview_Model(string change)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool();
        workspace.Show(tool);
        switch (change)
        {
            case "unpin": tool.ToggleAutoHide(); break;
            case "hide": tool.Hide(); break;
            case "float": tool.Float(); break;
            case "layout": workspace.Manager.Layout = new LayoutRoot(); break;
            case "xml":
                XmlLayoutSerializer serializer = new(workspace.Manager);
                using (StringWriter xml = new()) { serializer.Serialize(xml); serializer.Deserialize(new StringReader(xml.ToString())); }
                break;
        }
        workspace.Update();
        Assert.Null(workspace.Preview.Model);
        Assert.False(workspace.Preview.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Detaching_Stops_Closing_And_Reattachment_Can_Show_A_New_Preview(bool keepVisible)
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Tool();
        tool.DockModel.KeepPinnedDockableVisible = keepVisible;
        workspace.Show(tool);
        workspace.Window.Content = null; workspace.Update();
        Assert.Null(workspace.Preview.Model);
        Assert.Null(workspace.Manager.DockLayout.PinnedDock);
        workspace.Window.Content = workspace.Manager; workspace.Update();
        workspace.Show(tool);
        Assert.Same(tool, workspace.Preview.Model);
        await workspace.Wait(80);
        Assert.Same(tool, workspace.Preview.Model);
    }

    [AvaloniaTheory]
    [InlineData(80)]
    [InlineData(350)]
    public async Task Pointer_Reentry_At_The_Close_Deadline_Returns_To_Idle_Monitoring(int reentryDelay)
    {
        using Workspace workspace = new();
        // Hold the deadline while awaiting the first idle poll. A busy runner
        // may resume this continuation much later than the requested delay.
        workspace.Manager.AutoHideWindowClosingTimer = int.MaxValue;
        LayoutAnchorable tool = workspace.Tool();
        workspace.Show(tool); workspace.MoveAway();
        await workspace.Wait(reentryDelay);
        workspace.Window.MouseMove(workspace.Center(workspace.Preview));
        workspace.Manager.AutoHideWindowClosingTimer = 0;
        await workspace.Wait(80);
        Assert.Same(tool, workspace.Preview.Model);
        workspace.Manager.AutoHideWindowClosingTimer = 200;
        TimeSpan elapsed = await workspace.ObserveClosing(workspace.MoveAway);
        Assert.True(elapsed.TotalMilliseconds >= 199, $"The restarted 200 ms delay ended after {elapsed.TotalMilliseconds} ms.");
        Assert.Null(workspace.Preview.Model);
    }

    [AvaloniaFact]
    public async Task Native_Splitter_Drag_Keeps_The_Preview_Open_And_Persists_Its_Width()
    {
        using Workspace workspace = new();
        workspace.Manager.AutoHideWindowClosingTimer = 100;
        LayoutAnchorable tool = workspace.Tool();
        tool.AutoHideWidth = 230;
        workspace.Show(tool);
        LayoutGridResizerControl splitter = workspace.Manager.GetVisualDescendants().OfType<LayoutGridResizerControl>()
            .Single(control => control.Name == "PART_PinnedDockSplitter" && control.IsEffectivelyVisible);
        Assert.NotNull(splitter.FindAncestorOfType<PinnedDockControl>());
        Point start = workspace.Center(splitter);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(75, 0), RawInputModifiers.LeftMouseButton);
        workspace.Update();
        Assert.False(tool.IsActive);
        await workspace.Wait(80); await workspace.Wait(150);
        Assert.Same(tool, workspace.Preview.Model);
        workspace.Window.MouseUp(start + new Vector(75, 0), MouseButton.Left);
        workspace.Update();
        Assert.Equal(305, tool.AutoHideWidth, 1);
        tool.DockModel.GetPinnedBounds(out _, out _, out double width, out _);
        Assert.Equal(tool.AutoHideWidth, width, 1);
        workspace.MoveAway();
        await workspace.WaitUntilClosed();
        Assert.Null(workspace.Preview.Model);
    }

    [AvaloniaFact]
    public void Derived_Manager_Hooks_Can_Clear_And_Restore_The_Readonly_Preview_Property()
    {
        using Workspace workspace = new(probe: true);
        ManagerProbe manager = (ManagerProbe)workspace.Manager;
        Assert.True(DockingManager.AutoHideWindowProperty.IsReadOnly);
        Assert.Equal(1, manager.WindowChanges);
        LayoutAutoHideWindowControl preview = workspace.Preview;
        workspace.Show(workspace.Tool());
        manager.ReplacePreview(null); workspace.Update();
        Assert.Equal(2, manager.WindowChanges);
        Assert.Null(manager.AutoHideWindow);
        Assert.Null(preview.Model);
        Assert.Null(preview.Content);
        manager.DockFactory.PreviewPinnedDockable(workspace.Tool(AnchorSide.Right).DockModel);
        workspace.Update();
        Assert.Null(manager.AutoHideWindow);
        manager.ReplacePreview(preview); workspace.Update();
        Assert.Equal(3, manager.WindowChanges);
        Assert.Same(workspace.Tool(AnchorSide.Right), preview.Model);
        Assert.Contains(preview, manager.GetVisualDescendants());
        Assert.Single(preview.GetVisualDescendants().OfType<LayoutAnchorableControl>());
        manager.AutoHideWindowClosingTimer = 120;
        Assert.Equal(1, manager.TimerChanges);
        manager.AutoHideWindowClosingTimer = -1;
        Assert.Equal(2, manager.TimerChanges);
        Assert.Equal(1500, manager.AutoHideWindowClosingTimer);
    }

    [AvaloniaFact]
    public async Task A_Retained_Previous_Layout_Does_Not_Retain_The_Preview_Or_Manager()
    {
        var (model, preview, manager) = ReplaceLayout();
        await Task.Delay(100);
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
        Assert.False(preview.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(model);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (LayoutRoot Model, WeakReference<Control> Preview, WeakReference<DockingManager> Manager) ReplaceLayout()
    {
        using Workspace workspace = new();
        workspace.Show(workspace.Tool());
        var result = (workspace.Manager.Layout, new WeakReference<Control>(workspace.Preview), new WeakReference<DockingManager>(workspace.Manager));
        workspace.Manager.Layout = new LayoutRoot(); workspace.Update();
        workspace.Window.CaptureRenderedFrame()?.Dispose();
        return result;
    }

    private class ManagerProbe : DockingManager
    {
        public int WindowChanges { get; private set; }
        public int TimerChanges { get; private set; }
        public bool SuppressTimer { get; set; }
        public bool FailTimer { get; set; }
        public Action<AvaloniaPropertyChangedEventArgs, bool>? TimerCallback { get; set; }
        public AvaloniaPropertyChangedEventArgs? LastTimerChange { get; private set; }
        public void ApplyTimerChange(AvaloniaPropertyChangedEventArgs args) => base.OnAutoHideWindowClosingTimerChanged(args);
        public void ReplacePreview(LayoutAutoHideWindowControl? value) => SetAutoHideWindow(value);
        protected override void OnAutoHideWindowChanged(AvaloniaPropertyChangedEventArgs e) { WindowChanges++; base.OnAutoHideWindowChanged(e); }
        protected override void OnAutoHideWindowClosingTimerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            TimerChanges++;
            LastTimerChange = e;
            TimerCallback?.Invoke(e, false);
            if (FailTimer) throw new InvalidOperationException("Timer callback failure.");
            if (!SuppressTimer) base.OnAutoHideWindowClosingTimerChanged(e);
            TimerCallback?.Invoke(e, true);
        }
    }

    private sealed class DelayedManagerProbe : ManagerProbe
    {
        static DelayedManagerProbe() => AutoHideWindowClosingTimerProperty.OverrideDefaultValue<DelayedManagerProbe>(int.MaxValue);
    }

    private sealed class Workspace : IDisposable
    {
        public AnchorControlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutAutoHideWindowControl Preview => Manager.AutoHideWindow!;
        public Workspace(bool probe = false, ManagerProbe? configuredManager = null)
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            if (probe || configuredManager is not null)
            {
                LayoutRoot layout = Manager.Layout;
                Manager.Layout = new LayoutRoot();
                Manager = configuredManager ?? new ManagerProbe();
                Manager.Layout = layout;
                Window.Content = Manager;
            }
            Window.Show(); Update();
        }
        public LayoutAnchorable Tool(AnchorSide side = AnchorSide.Left) => (side switch
        {
            AnchorSide.Left => Manager.Layout.LeftSide!, AnchorSide.Top => Manager.Layout.TopSide!,
            AnchorSide.Right => Manager.Layout.RightSide!, _ => Manager.Layout.BottomSide!
        }).Children[0].Children[0];
        public void Show(LayoutAnchorable tool) { tool.IsSelected = true; Update(); }
        public void MoveAway() { Window.MouseMove(new Point(895, 645)); Update(); }
        public Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), Window)!.Value;
        public async Task Wait(int milliseconds) { await Task.Delay(milliseconds); Update(); }
        public async Task<TimeSpan> ObserveClosing(Action begin)
        {
            LayoutAutoHideWindowControl preview = Preview;
            TaskCompletionSource<TimeSpan> closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            long started = Stopwatch.GetTimestamp();
            void Changed(object? sender, AvaloniaPropertyChangedEventArgs args)
            {
                if (args.Property == LayoutAutoHideWindowControl.ModelProperty && preview.Model is null)
                    closed.TrySetResult(Stopwatch.GetElapsedTime(started));
            }
            preview.PropertyChanged += Changed;
            try
            {
                begin();
                return await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            finally { preview.PropertyChanged -= Changed; Update(); }
        }
        public async Task WaitUntilClosed()
        {
            if (Preview.Model is not null) await ObserveClosing(() => { });
            Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
