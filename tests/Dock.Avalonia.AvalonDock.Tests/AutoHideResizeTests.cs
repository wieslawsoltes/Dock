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
    [InlineData(AnchorSide.Left, false)]
    [InlineData(AnchorSide.Right, false)]
    [InlineData(AnchorSide.Top, false)]
    [InlineData(AnchorSide.Bottom, false)]
    [InlineData(AnchorSide.Left, true)]
    [InlineData(AnchorSide.Right, true)]
    [InlineData(AnchorSide.Top, true)]
    [InlineData(AnchorSide.Bottom, true)]
    public void Physical_Ghost_Limits_And_Model_Minima_Apply_At_Different_Stages(AnchorSide side, bool rtl)
    {
        using Workspace workspace = new(side, rtl);
        workspace.Tool.AutoHideMinWidth = 175;
        workspace.Tool.AutoHideMinHeight = 155;
        workspace.Update();
        double original = workspace.Size;
        double farEdge = workspace.Horizontal ? workspace.Area.Bounds.Width : workspace.Area.Bounds.Height - 155;
        bool forward = side is AnchorSide.Left or AnchorSide.Top;
        workspace.Begin(); workspace.Move(-2000);
        Assert.Equal(original, workspace.Size);
        Assert.Equal(forward ? 25 : farEdge, workspace.GhostPosition, 1);
        workspace.End(-2000);
        Assert.Equal(workspace.Horizontal ? 175 : 155, workspace.Size, 1);
        double extent = workspace.Horizontal ? workspace.Area.Bounds.Width : workspace.Area.Bounds.Height;
        double thickness = workspace.Horizontal ? workspace.Splitter.Bounds.Width : workspace.Splitter.Bounds.Height;
        workspace.Begin(); workspace.Move(2000);
        Assert.Equal(forward ? farEdge : 25, workspace.GhostPosition, 1);
        workspace.End(2000);
        double maximum = side switch
        {
            AnchorSide.Left => extent,
            AnchorSide.Top => extent - 155,
            _ => extent - 25 - thickness
        };
        Assert.Equal(maximum, workspace.Size, 1);
        Assert.Equal(workspace.Size, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        if (side == AnchorSide.Right) Assert.Equal(extent, workspace.BoundsInArea(workspace.Content).Right, 1);
        if (side == AnchorSide.Bottom) Assert.Equal(extent, workspace.BoundsInArea(workspace.Content).Bottom, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void A_Later_Drag_Uses_Current_Splitter_Thickness_And_Minimum_Height(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Begin(); workspace.Move(40);
        workspace.Manager.GridSplitterWidth = 19;
        workspace.Manager.GridSplitterHeight = 13;
        workspace.Tool.AutoHideMinHeight = 145;
        workspace.Update();
        Assert.False(workspace.Ghost.IsVisible);
        workspace.End(40);
        Assert.Equal(workspace.Horizontal ? 220 : 190, workspace.Size);

        double extent = workspace.Horizontal ? workspace.Area.Bounds.Width : workspace.Area.Bounds.Height;
        workspace.Begin(); workspace.Move(2000);
        double position = side switch
        {
            AnchorSide.Left => extent,
            AnchorSide.Top => extent - 145,
            _ => 25
        };
        Assert.Equal(position, workspace.GhostPosition, 1);
        workspace.End(2000);
        double maximum = side switch
        {
            AnchorSide.Left => extent,
            AnchorSide.Right => extent - 25 - 19,
            AnchorSide.Top => extent - 145,
            _ => extent - 25 - 13
        };
        Assert.Equal(maximum, workspace.Size, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Completion_Adds_Movement_To_The_Stored_Size_Even_When_The_View_Is_Larger(AnchorSide side)
    {
        using Workspace workspace = new(side);
        double stored = workspace.Size;
        workspace.Host.MinimumPinnedWidth = 290;
        workspace.Host.MinimumPinnedHeight = 270;
        workspace.Update();
        Assert.True((workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height) > stored);
        workspace.Begin(); workspace.Move(120);
        Assert.Equal(stored, workspace.Size);
        workspace.End(120);
        Assert.Equal(stored + 120, workspace.Size, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void An_Unset_Size_Uses_The_Displayed_Content_When_The_Drag_Commits(AnchorSide side)
    {
        using Workspace workspace = new(side, initializeSize: false);
        Assert.Equal(0, workspace.Size);
        double displayed = workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height;
        Assert.Equal(100, displayed, 1);
        workspace.Begin(); workspace.Move(35);
        workspace.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        workspace.End(35);
        workspace.Update(); workspace.Update();
        Assert.Equal(0, workspace.Size);
        workspace.Begin(); workspace.Move(35);
        Assert.Equal(0, workspace.Size);
        workspace.End(35);
        Assert.Equal(displayed + 35, workspace.Size, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Changing_The_Minimum_Preserves_Unset_Content_Until_Completion_Coerces_The_Size(AnchorSide side)
    {
        using Workspace workspace = new(side, initializeSize: false);
        workspace.Tool.AutoHideMinWidth = 300;
        workspace.Tool.AutoHideMinHeight = 280;
        if (workspace.Horizontal) workspace.Tool.AutoHideHeight = 400;
        else workspace.Tool.AutoHideWidth = 400;
        workspace.Update();
        Assert.Equal(0, workspace.Size);
        Assert.Equal(100, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        workspace.Begin();
        double position = workspace.GhostPosition;
        workspace.Move(35);
        double direction = side is AnchorSide.Right or AnchorSide.Bottom ? -1 : 1;
        double farEdge = workspace.Horizontal ? workspace.Area.Bounds.Width : workspace.Area.Bounds.Height - 280;
        Assert.Equal(Math.Clamp(position + direction * 35, 25, farEdge), workspace.GhostPosition, 1);
        Assert.Equal(0, workspace.Size);
        workspace.End(35);
        Assert.Equal(workspace.Horizontal ? 300 : 280, workspace.Size, 1);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void An_Unset_Size_With_A_NaN_Minimum_Uses_The_Initial_100_Pixel_Fallback(AnchorSide side)
    {
        using Workspace workspace = new(side, initializeSize: false, undefinedMinimum: true);
        workspace.Update(); workspace.Update();
        Assert.Equal(0, workspace.Size);
        Assert.Equal(100, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
    }

    [AvaloniaFact]
    public void Switching_Tools_Releases_The_Unset_Content_Dimension_And_Preserves_Both_Models()
    {
        using Workspace workspace = new(AnchorSide.Left, initializeSize: false);
        LayoutAnchorable second = workspace.Manager.Layout.LeftSide!.Children[0].Children[1];
        second.AutoHideWidth = 310;
        second.AutoHideHeight = 210;
        Assert.Equal(100, workspace.Content.Bounds.Width, 1);
        second.IsSelected = true;
        workspace.Update();
        Assert.Equal(310, workspace.Content.Bounds.Width, 1);
        Assert.True(double.IsNaN(workspace.Content.Width));
        Assert.Equal(0, workspace.Tool.AutoHideWidth);
        workspace.Tool.IsSelected = true;
        workspace.Update();
        Assert.Equal(100, workspace.Content.Bounds.Width, 1);
        Assert.Equal(0, workspace.Tool.AutoHideWidth);
        Assert.Equal(310, second.AutoHideWidth);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Moving_With_An_Inverted_Ghost_Interval_Rejects_The_Range(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Tool.AutoHideMinHeight = workspace.Area.Bounds.Height - 22;
        workspace.Update();
        double original = workspace.Size;
        workspace.Begin();
        Assert.Throws<ArgumentException>(() => workspace.Move(10));
        workspace.Window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        workspace.End(10);
        Assert.False(workspace.Ghost.IsVisible);
        Assert.Equal(original, workspace.Size);
    }

    [AvaloniaFact]
    public void Compatibility_Sizing_Styles_Respect_Local_Values_And_Release_Their_Native_Scope()
    {
        PinnedDockControl ordinary = new();
        Assert.Equal(50, ordinary.MinimumPinnedWidth);
        Assert.Equal(50, ordinary.MinimumPinnedHeight);
        Assert.Equal(50, ordinary.MinimumRemainingContentSize);
        Assert.True(ordinary.InitializePinnedBoundsOnLayout);

        using Workspace workspace = new(AnchorSide.Left, initializeSize: false);
        PinnedDockControl host = workspace.Host;
        ContentControl content = workspace.Content;
        DockControl native = host.FindAncestorOfType<DockControl>()!;
        Assert.Equal(0, host.MinimumPinnedWidth);
        Assert.Equal(0, host.MinimumPinnedHeight);
        Assert.Equal(0, host.MinimumRemainingContentSize);
        Assert.False(host.InitializePinnedBoundsOnLayout);
        Assert.Equal(100, content.Width);
        host.MinimumPinnedWidth = 20;
        Assert.Equal(20, host.MinimumPinnedWidth);
        host.ClearValue(PinnedDockControl.MinimumPinnedWidthProperty);
        Assert.Equal(0, host.MinimumPinnedWidth);

        foreach (LayoutContent model in workspace.Manager.Layout.Descendents().OfType<LayoutContent>())
            model.Content = new Border();
        Assert.True(native.Classes.Remove("avalondock"));
        workspace.Update();
        Assert.Equal(50, host.MinimumPinnedWidth);
        Assert.Equal(50, host.MinimumPinnedHeight);
        Assert.Equal(50, host.MinimumRemainingContentSize);
        Assert.True(host.InitializePinnedBoundsOnLayout);
        Assert.True(double.IsNaN(content.Width));
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Bottom)]
    public void Splitter_Dimensions_Update_While_New_Minima_Preserve_An_Existing_Pixel_Size(AnchorSide side)
    {
        using Workspace workspace = new(side);
        workspace.Manager.GridSplitterWidth = 19;
        workspace.Manager.GridSplitterHeight = 13;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 19 : 13, workspace.Horizontal ? workspace.Splitter.Bounds.Width : workspace.Splitter.Bounds.Height);
        workspace.Tool.AutoHideMinWidth = 290;
        workspace.Tool.AutoHideMinHeight = 270;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 220 : 190, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
        workspace.Tool.AutoHideMinWidth = double.NaN;
        workspace.Tool.AutoHideMinHeight = double.NaN;
        workspace.Manager.GridSplitterWidth = double.NaN;
        workspace.Manager.GridSplitterHeight = double.NaN;
        workspace.Update();
        Assert.Equal(workspace.Horizontal ? 220 : 190, workspace.Horizontal ? workspace.Content.Bounds.Width : workspace.Content.Bounds.Height, 1);
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
        public PinnedDockHostPanel Area => Host.FindAncestorOfType<PinnedDockHostPanel>()!;
        public ContentControl Content => Host.GetVisualDescendants().OfType<ContentControl>().Single(control => control.Name == "PART_PinnedDock");
        public Border Ghost => Host.GetVisualDescendants().OfType<Border>().Single(control => control.Name == "PART_PinnedDockResizePreview");
        public double GhostPosition => Horizontal ? BoundsInArea(Ghost).X : BoundsInArea(Ghost).Y;
        private Point _start;
        private IPointer? _pointer;

        public Workspace(AnchorSide side, bool rtl = false, bool initializeSize = true, bool undefinedMinimum = false)
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
            if (undefinedMinimum)
            {
                Tool.AutoHideMinWidth = double.NaN;
                Tool.AutoHideMinHeight = double.NaN;
            }
            if (initializeSize)
            {
                Tool.AutoHideWidth = 220;
                Tool.AutoHideHeight = 190;
            }
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
        public Rect BoundsInArea(Control control)
        {
            Point areaFirst = Area.TranslatePoint(default, Window)!.Value;
            Point areaLast = Area.TranslatePoint(new Point(Area.Bounds.Width, Area.Bounds.Height), Window)!.Value;
            Vector origin = new(Math.Min(areaFirst.X, areaLast.X), Math.Min(areaFirst.Y, areaLast.Y));
            Point first = control.TranslatePoint(default, Window)!.Value - origin;
            Point last = control.TranslatePoint(new Point(control.Bounds.Width, control.Bounds.Height), Window)!.Value - origin;
            return new Rect(new Point(Math.Min(first.X, last.X), Math.Min(first.Y, last.Y)),
                new Point(Math.Max(first.X, last.X), Math.Max(first.Y, last.Y)));
        }
        public void Move(double growth) { Window.MouseMove(_start + Delta(growth), RawInputModifiers.LeftMouseButton); Update(); }
        public void End(double growth) { Window.MouseUp(_start + Delta(growth), MouseButton.Left); Update(); }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
