using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Core;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class SplitterSizingTests
{
    [AvaloniaFact]
    public void Default_Size_Is_Six_And_Unprefixed_Xaml_Configures_Each_Axis()
    {
        DockingManager defaults = new();
        Assert.Equal(6, defaults.GridSplitterWidth);
        Assert.Equal(6, defaults.GridSplitterHeight);
        using Workspace workspace = new();
        Assert.Equal(12, Splitter(workspace.Manager, workspace.Manager.Layout.RootPanel).Bounds.Width);
        Assert.Equal(8, Splitter(workspace.Manager, workspace.Group).Bounds.Height);
        workspace.Manager.ClearValue(DockingManager.GridSplitterWidthProperty);
        workspace.Manager.ClearValue(DockingManager.GridSplitterHeightProperty);
        workspace.Update();
        Assert.Equal(6, Splitter(workspace.Manager, workspace.Manager.Layout.RootPanel).Bounds.Width);
        Assert.Equal(6, Splitter(workspace.Manager, workspace.Group).Bounds.Height);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nested_Pixel_Panes_Use_Actual_Docked_Or_Floating_Space(bool floating)
    {
        using Workspace workspace = new(floating);
        AssertExtent(workspace.TopControl.Bounds.Height, 200);
        double previousBottom = workspace.BottomControl.Bounds.Height;
        workspace.Manager.GridSplitterHeight = 28;
        workspace.Update();
        Assert.Equal(28, Splitter(workspace.Scope, workspace.Group).Bounds.Height);
        AssertExtent(workspace.TopControl.Bounds.Height, 200);
        AssertExtent(workspace.BottomControl.Bounds.Height, previousBottom - 20);
        workspace.Top.DockHeight = new GridLength(240);
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, 240);
        workspace.Host.Height += 100;
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, 240);
        AssertExtent(workspace.BottomControl.Bounds.Height, previousBottom + 40);
        if (!floating)
        {
            AssertExtent(workspace.GroupControl.Bounds.Width, 420);
            workspace.Manager.GridSplitterWidth = 24;
            workspace.Update();
            AssertExtent(workspace.GroupControl.Bounds.Width, 420);
            Assert.Equal(24, Splitter(workspace.Manager, workspace.Manager.Layout.RootPanel).Bounds.Width);
        }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Orientation_Changes_Select_The_Correct_Splitter_Dimension(bool floating, bool equalDimensions)
    {
        using Workspace workspace = new(floating);
        if (equalDimensions) workspace.Manager.GridSplitterWidth = 8;
        workspace.Group.Orientation = Orientation.Horizontal;
        workspace.Update();
        Assert.Equal(GridLength.Star, workspace.Top.DockWidth);
        workspace.Top.DockWidth = new GridLength(140);
        workspace.Update();
        ProportionalStackPanelSplitter splitter = Splitter(workspace.Scope, workspace.Group);
        IProportionalSplitterHost host = splitter.GetVisualAncestors().OfType<IProportionalSplitterHost>().First();
        Panel panel = host.Panel;
        Assert.Equal(equalDimensions ? 8 : 12, splitter.Bounds.Width);
        AssertExtent(splitter.Bounds.Height, panel.Bounds.Height);
        AssertExtent(workspace.TopControl.Bounds.Width, 140);
        workspace.Manager.GridSplitterWidth = 30;
        workspace.Update();
        Assert.Equal(30, Splitter(workspace.Scope, workspace.Group).Bounds.Width);
        AssertExtent(workspace.TopControl.Bounds.Width, 140);
        workspace.Group.Orientation = Orientation.Vertical;
        workspace.Update();
        Assert.Equal(GridLength.Star, workspace.Top.DockHeight);
        workspace.Top.DockHeight = new GridLength(200);
        workspace.Update();
        Assert.Equal(8, Splitter(workspace.Scope, workspace.Group).Bounds.Height);
        AssertExtent(splitter.Bounds.Width, panel.Bounds.Width);
        AssertExtent(workspace.TopControl.Bounds.Height, 200);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Drag_Persists_Pixel_Dimensions_After_Resizing_And_Changing_Splitters(bool floating)
    {
        using Workspace workspace = new(floating);
        ProportionalStackPanelSplitter splitter = Splitter(workspace.Scope, workspace.Group);
        Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), workspace.Host)!.Value;
        workspace.Host.MouseMove(start);
        IInputElement? hit = workspace.Host.InputHitTest(start);
        Assert.True(ReferenceEquals(hit, splitter) || hit is Visual visual && visual.GetVisualAncestors().Contains(splitter),
            $"Splitter at {start} in {workspace.Host.Bounds} hit {hit?.GetType().Name}.");
        workspace.Host.MouseDown(start, MouseButton.Left);
        workspace.Host.MouseMove(start + new Vector(0, 40));
        workspace.Host.MouseUp(start + new Vector(0, 40), MouseButton.Left);
        workspace.Update();
        AssertExtent(workspace.Top.DockHeight.Value, 240);
        AssertExtent(workspace.TopControl.Bounds.Height, workspace.Top.DockHeight.Value);
        workspace.Manager.GridSplitterHeight = 18;
        workspace.Host.Height += 120;
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, workspace.Top.DockHeight.Value);
        AssertExtent(workspace.Top.DockHeight.Value, 240);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Star_Sizes_Keep_Their_Ratio_After_Splitter_Changes(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Top.DockHeight = new GridLength(2, GridUnitType.Star);
        workspace.Bottom.DockHeight = new GridLength(1, GridUnitType.Star);
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, (workspace.TopControl.Bounds.Height + workspace.BottomControl.Bounds.Height) * 2 / 3);
        workspace.Manager.GridSplitterHeight = 32;
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, (workspace.TopControl.Bounds.Height + workspace.BottomControl.Bounds.Height) * 2 / 3);
        Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.Top.DockHeight);
        Assert.Equal(new GridLength(1, GridUnitType.Star), workspace.Bottom.DockHeight);
    }

    [AvaloniaFact]
    public void Splitter_Settings_Belong_To_Each_Manager_And_Do_Not_Change_Native_Dock_Defaults()
    {
        using Workspace first = new(true);
        using Workspace second = new(true);
        first.Manager.GridSplitterHeight = 22;
        first.Update();
        second.Update();
        Assert.Equal(22, Splitter(first.Scope, first.Group).Thickness);
        Assert.Equal(8, Splitter(second.Scope, second.Group).Thickness);
        Assert.Equal(4, new ProportionalStackPanelSplitter().Thickness);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collapsed_Return_Panes_Leave_One_Splitter_Between_Visible_Panes(bool floating)
    {
        using Workspace workspace = new(floating);
        LayoutAnchorablePane empty = new() { DockHeight = new GridLength(120) };
        empty.DockModel.IsCollapsable = true;
        workspace.Group.Children.Insert(1, empty);
        workspace.Update();
        Assert.True(empty.DockModel.IsEmpty);
        Assert.Equal(GridLength.Star, workspace.Top.DockHeight);
        Assert.Equal(GridLength.Star, empty.DockHeight);
        workspace.Top.DockHeight = new GridLength(200);
        workspace.Update();
        AssertExtent(workspace.TopControl.Bounds.Height, 200);
        // Upstream keeps the splitter after a visible pane when any later pane
        // is visible, even when an empty return pane sits between them.
        AssertExtent(workspace.TopControl.Bounds.Height + workspace.BottomControl.Bounds.Height + workspace.Manager.GridSplitterHeight,
            workspace.GroupControl.Bounds.Height);
        empty.Children.Add(new LayoutAnchorable { Title = "Restored pane" });
        workspace.Update();
        Assert.Equal(GridLength.Star, workspace.Top.DockHeight);
        workspace.Top.DockHeight = new GridLength(200);
        empty.DockHeight = new GridLength(120);
        workspace.Update();
        ToolDockControl restored = workspace.Scope.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, empty.DockModel));
        AssertExtent(restored.Bounds.Height, 120);
        AssertExtent(workspace.TopControl.Bounds.Height, 200);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void XML_Replacement_And_View_Reattachment_Keep_Manager_Sizing(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Manager.GridSplitterWidth = 16;
        workspace.Manager.GridSplitterHeight = 20;
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        LayoutAnchorable top = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single(tool => tool.ContentId == "top");
        LayoutAnchorablePaneGroup group = (LayoutAnchorablePaneGroup)top.Parent!.Parent!;
        Control scope = floating ? (HostWindow)top.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : workspace.Manager;
        Assert.Equal(20, Splitter(scope, group).Thickness);
        workspace.Window.Content = null;
        workspace.Update();
        workspace.Manager.GridSplitterHeight = 26;
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        scope = floating ? (HostWindow)top.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : workspace.Manager;
        Assert.Equal(26, Splitter(scope, group).Bounds.Height);
        ((LayoutAnchorablePane)top.Parent!).DockHeight = new GridLength(180);
        workspace.Update();
        ToolDockControl pane = scope.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, top.DockModel.Owner));
        AssertExtent(pane.Bounds.Height, 180);
    }

    [AvaloniaFact]
    public void Manager_Does_Not_Retain_Discarded_Hosts_Through_Size_Updates()
    {
        DockingManager manager = new();
        WeakReference<HostWindow> reference = CreateUnusedHost(manager);
        Collect();
        Assert.False(reference.TryGetTarget(out _));
        manager.GridSplitterWidth = 30;
        manager.GridSplitterHeight = 24;
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Manager_Does_Not_Retain_Panels_From_A_Replaced_Layout(bool floating)
    {
        var result = ReplaceLayout(floating);
        Collect();
        Assert.False(result.Panel.TryGetTarget(out _));
        GC.KeepAlive(result.Manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockingManager Manager, WeakReference<Panel> Panel) ReplaceLayout(bool floating)
    {
        using Workspace workspace = new(floating);
        Panel panel = workspace.GroupControl;
        workspace.Manager.Layout = new LayoutRoot();
        workspace.Update();
        // Closed windows stop rendering and can retain their last composition frame.
        // Flush the replacement frame while the window is still open.
        workspace.Window.CaptureRenderedFrame()?.Dispose();
        return (workspace.Manager, new(panel));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<HostWindow> CreateUnusedHost(DockingManager manager)
    {
        HostWindow host = (HostWindow)manager.DockHostWindowFactory()!;
        host.Close();
        return new(host);
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static ProportionalStackPanelSplitter Splitter(Control scope, LayoutElement group) =>
        scope.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>()
            .Single(control => control.DataContext is IDockable model && ReferenceEquals(model.Owner, group.DockModel));

    private static void AssertExtent(double actual, double expected) => Assert.InRange(actual, expected - 1, expected + 1);

    private sealed class Workspace : IDisposable
    {
        public SizingFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutAnchorablePaneGroup Group { get; }
        public LayoutAnchorablePane Top { get; }
        public LayoutAnchorablePane Bottom { get; }
        public Control Scope { get; }
        public Window Host { get; }
        public ToolDockControl TopControl => Scope.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, Top.DockModel));
        public ToolDockControl BottomControl => Scope.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, Bottom.DockModel));
        public LayoutAnchorablePaneGroupControl GroupControl => Scope.GetVisualDescendants().OfType<LayoutAnchorablePaneGroupControl>()
            .Single(control => ReferenceEquals(control.DataContext, Group.DockModel));

        public Workspace(bool floating = false)
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Group = Manager.Layout.Descendents().OfType<LayoutAnchorablePaneGroup>().Single();
            Top = (LayoutAnchorablePane)Group.Children[0];
            Bottom = (LayoutAnchorablePane)Group.Children[1];
            Window.Show();
            Update();
            if (floating) Manager.DockFactory.FloatDockable(Group.DockModel);
            Update();
            Scope = floating ? (HostWindow)Group.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : Manager;
            Host = floating ? (Window)Scope : Window;
            if (floating) { Host.Width = 600; Host.Height = 460; }
            Update();
            // Upstream pane groups normalize initial/rebuilt lengths to stars.
            // These tests exercise explicit pixel changes after the first layout.
            Top.DockHeight = new GridLength(200);
            Update();
        }

        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Host?.UpdateLayout();
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
