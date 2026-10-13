using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideResizeTests
{
    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Native_Inline_Previews_Can_Grow_Into_The_Available_Host_Area(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Tool.DockModel.PinnedDockDisplayModeOverride = PinnedDockDisplayMode.Inline;
        workspace.Update();
        double before = workspace.Size;
        workspace.Begin(); workspace.Move(40); workspace.End(40);
        Assert.Equal(before + 40, workspace.Size, 1);
        Assert.Equal(workspace.Size, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    public void Changing_The_Public_Size_Updates_An_Open_Preview(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Tool.AutoHideWidth = 310;
        workspace.Tool.AutoHideHeight = 280;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 310 : 280, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        workspace.Begin(); workspace.Move(40);
        if (workspace.Horizontal) workspace.Tool.AutoHideWidth = 260;
        else workspace.Tool.AutoHideHeight = 260;
        workspace.Update();
        Assert.False(workspace.Ghost.IsVisible);
        workspace.End(40);
        Assert.Equal(260, workspace.Size);
    }

    [AvaloniaFact]
    public void Secondary_Button_Does_Not_Start_A_Resize()
    {
        using Workspace workspace = new(AnchorSide.Left);
        Point start = workspace.Splitter.TranslatePoint(new Point(3, 20), workspace.Window)!.Value;
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Right);
        workspace.Window.MouseMove(start + new Vector(70, 0), RawInputModifiers.RightMouseButton);
        workspace.Window.MouseUp(start + new Vector(70, 0), MouseButton.Right);
        workspace.Update();
        Assert.False(workspace.Ghost.IsVisible);
        Assert.Equal(220, workspace.Size);
    }

    [AvaloniaFact]
    public async Task A_Retained_Tool_Does_Not_Retain_A_Discarded_Resizer()
    {
        var (tool, splitter, manager) = DiscardResizer();
        // Wait on Avalonia's timer queue so the canceled hold gesture has
        // released its press route before checking model subscription lifetime.
        TaskCompletionSource drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable timer = DispatcherTimer.RunOnce(() => drained.SetResult(),
            Application.Current!.PlatformSettings!.HoldWaitDuration + TimeSpan.FromMilliseconds(50));
        await drained.Task;
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
        Assert.False(splitter.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(tool);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (LayoutAnchorable tool, WeakReference<LayoutGridResizerControl> splitter, WeakReference<DockingManager> manager) DiscardResizer()
    {
        using Workspace workspace = new(AnchorSide.Left);
        workspace.Begin(); workspace.Move(50);
        var result = (workspace.Tool, new WeakReference<LayoutGridResizerControl>(workspace.Splitter), new WeakReference<DockingManager>(workspace.Manager));
        workspace.Manager.Layout = new LayoutRoot();
        workspace.End(50);
        workspace.Window.MouseMove(new Point(895, 645));
        workspace.Window.Content = null; workspace.Update();
        workspace.Window.CaptureRenderedFrame()?.Dispose();
        return result;
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left, false)]
    [InlineData(AnchorSide.Right, false)]
    [InlineData(AnchorSide.Top, false)]
    [InlineData(AnchorSide.Bottom, false)]
    [InlineData(AnchorSide.Left, true)]
    [InlineData(AnchorSide.Right, true)]
    [InlineData(AnchorSide.Top, true)]
    [InlineData(AnchorSide.Bottom, true)]
    public void Public_Resizer_Previews_All_Edges_And_Commits_Only_On_Release(AnchorSide side, bool rtl)
    {
        using Workspace workspace = new(side, rtl);
        double original = workspace.Size;
        object? owner = workspace.Tool.DockModel.Owner;
        Assert.Equal(Brushes.Teal, workspace.Ghost.Background);
        Assert.Equal(0.35, workspace.Ghost.Opacity);
        workspace.Begin();
        Assert.True(workspace.Ghost.IsVisible);
        Point before = workspace.Ghost.TranslatePoint(default, workspace.Window)!.Value;
        workspace.Move(35);
        Assert.Equal(original, workspace.Size);
        Point after = workspace.Ghost.TranslatePoint(default, workspace.Window)!.Value;
        Assert.Equal(35, Math.Abs(workspace.Horizontal ? after.X - before.X : after.Y - before.Y), 1);
        workspace.Splitter.BackgroundWhileDragging = Brushes.Magenta;
        workspace.Splitter.OpacityWhileDragging = 0.7;
        workspace.Update();
        Assert.Equal(Brushes.Magenta, workspace.Ghost.Background);
        Assert.Equal(0.7, workspace.Ghost.Opacity);
        workspace.End(35);
        Assert.False(workspace.Ghost.IsVisible);
        Assert.Equal(original + 35, workspace.Size, 1);
        Assert.Equal(workspace.Size, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        Assert.Same(owner, workspace.Tool.DockModel.Owner);
        Assert.Same(workspace.Tool.DockModel, workspace.Manager.DockLayout.PinnedDock!.ActiveDockable);
        workspace.Tool.DockModel.GetPinnedBounds(out _, out _, out double width, out double height);
        Assert.Equal(workspace.Size, workspace.Horizontal ? width : height);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Minimum_And_Available_Space_Constrain_The_Preview_And_Committed_Size(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Tool.AutoHideMinWidth = 175;
        workspace.Tool.AutoHideMinHeight = 155;
        workspace.Update();
        Assert.Equal(175, workspace.Host.MinimumPinnedWidth);
        Assert.Equal(155, workspace.Host.MinimumPinnedHeight);
        workspace.Begin(); workspace.Move(-1000); workspace.End(-1000);
        Assert.Equal(workspace.Horizontal ? 175 : 155, workspace.Size, 1);
        double extent = workspace.Horizontal ? workspace.Host.Bounds.Width : workspace.Host.Bounds.Height;
        double thickness = workspace.Horizontal ? workspace.Splitter.Bounds.Width : workspace.Splitter.Bounds.Height;
        workspace.Begin(); workspace.Move(2000); workspace.End(2000);
        Assert.Equal(extent - thickness - 50, workspace.Size, 1);
        Assert.Equal(workspace.Size, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Manager_Splitter_Dimensions_And_Tool_Minima_Update_The_Native_Template(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Manager.GridSplitterWidth = 19;
        workspace.Manager.GridSplitterHeight = 13;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 19 : 13, workspace.Horizontal ? workspace.Splitter.Bounds.Width : workspace.Splitter.Bounds.Height);
        workspace.Tool.AutoHideMinWidth = 290;
        workspace.Tool.AutoHideMinHeight = 270;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 290 : 270, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        workspace.Tool.AutoHideMinWidth = double.NaN;
        workspace.Tool.AutoHideMinHeight = double.NaN;
        workspace.Manager.GridSplitterWidth = double.NaN;
        workspace.Manager.GridSplitterHeight = double.NaN;
        workspace.Update();
        Assert.Equal(100, workspace.Host.MinimumPinnedWidth);
        Assert.Equal(100, workspace.Host.MinimumPinnedHeight);
        Assert.Equal(6, workspace.Horizontal ? workspace.Splitter.Bounds.Width : workspace.Splitter.Bounds.Height);
    }

    [AvaloniaTheory]
    [InlineData("escape")]
    [InlineData("capture")]
    [InlineData("detach")]
    [InlineData("switch")]
    [InlineData("layout")]
    [InlineData("disable")]
    [InlineData("minimum")]
    public void Interrupted_Drag_Discards_The_Preview_Without_Changing_Pinned_Bounds(string interruption)
    {
        using Workspace workspace = new(AnchorSide.Left);
        LayoutAnchorable tool = workspace.Tool;
        double original = tool.AutoHideWidth;
        Border ghost = workspace.Ghost;
        workspace.Begin(); workspace.Move(60);
        switch (interruption)
        {
            case "escape": workspace.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null); break;
            case "capture": workspace.LoseCapture(); break;
            case "detach": workspace.Window.Content = null; break;
            case "switch": workspace.Manager.Layout.LeftSide!.Children[0].Children[1].IsSelected = true; break;
            case "layout": workspace.Manager.Layout = new LayoutRoot(); break;
            case "disable": workspace.Splitter.IsResizingEnabled = false; break;
            case "minimum": tool.AutoHideMinWidth = 180; break;
        }
        workspace.Update();
        Assert.False(ghost.IsVisible);
        workspace.End(60);
        Assert.Equal(original, tool.AutoHideWidth, 1);
        if (interruption == "detach")
        {
            workspace.Window.Content = workspace.Manager; workspace.Update();
            tool.IsSelected = true; workspace.Update();
            workspace.Begin(); workspace.Move(20); workspace.End(20);
            Assert.Equal(original + 20, tool.AutoHideWidth, 1);
        }
    }

    private sealed class Workspace : IDisposable
    {
        public AnchorControlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutAnchorable Tool { get; }
        public AnchorSide Side { get; }
        public bool Horizontal => Side is AnchorSide.Left or AnchorSide.Right;
        public double Size => Horizontal ? Tool.AutoHideWidth : Tool.AutoHideHeight;
        public LayoutGridResizerControl Splitter => Manager.GetVisualDescendants().OfType<LayoutGridResizerControl>()
            .Single(control => control.Name == "PART_PinnedDockSplitter" && control.IsEffectivelyVisible);
        public PinnedDockControl Host => Splitter.FindAncestorOfType<PinnedDockControl>()!;
        public ContentControl Content => Host.GetVisualDescendants().OfType<ContentControl>().Single(control => control.Name == "PART_PinnedDock");
        public Border Ghost => Host.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_PinnedDockResizePreview");
        private Point _start;
        private IPointer? _pointer;

        public Workspace(AnchorSide side, bool rtl = false)
        {
            Side = side;
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Manager.AutoHideWindowClosingTimer = int.MaxValue;
            Manager.FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            Tool = (side switch
            {
                AnchorSide.Left => Manager.Layout.LeftSide!, AnchorSide.Right => Manager.Layout.RightSide!,
                AnchorSide.Top => Manager.Layout.TopSide!, _ => Manager.Layout.BottomSide!
            }).Children[0].Children[0];
            Tool.AutoHideWidth = 220;
            Tool.AutoHideHeight = 190;
            Window.Show(); Update(); Tool.IsSelected = true; Update();
        }

        private Vector Delta(double growth)
        {
            double value = Side is AnchorSide.Right or AnchorSide.Bottom ? -growth : growth;
            return Horizontal ? new Vector(value, 0) : new Vector(0, value);
        }

        public void Begin()
        {
            LayoutGridResizerControl splitter = Splitter;
            _start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), Window)!.Value;
            void Pressed(object? sender, PointerPressedEventArgs args) => _pointer = args.Pointer;
            splitter.AddHandler(InputElement.PointerPressedEvent, Pressed, handledEventsToo: true);
            try { Window.MouseMove(_start); Window.MouseDown(_start, MouseButton.Left); Update(); }
            finally { splitter.RemoveHandler(InputElement.PointerPressedEvent, Pressed); }
        }
        public void LoseCapture() => _pointer!.Capture(Window);
        public void Move(double growth) { Window.MouseMove(_start + Delta(growth), RawInputModifiers.LeftMouseButton); Update(); }
        public void End(double growth) { Window.MouseUp(_start + Delta(growth), MouseButton.Left); Update(); }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
