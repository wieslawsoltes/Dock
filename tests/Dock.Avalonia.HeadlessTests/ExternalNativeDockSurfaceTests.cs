using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using Avalonia.Media;
using Avalonia.Threading;
using Dock.Avalonia.Contract;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ExternalNativeDockSurfaceTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Registered_Surface_In_Another_Root_Is_Hit_Using_Screen_Coordinates(bool embedded)
    {
        using Fixture fixture = new(embedded);
        Border target = fixture.Target;
        DockProperties.SetIsDropArea(target, true);
        DockProperties.SetIsDockTarget(target, true);
        Point point = fixture.Host.PointToClient(target.PointToScreen(new Point(10, 10)));
        Assert.Same(target, DockHelpers.GetControlIncludingExternal(fixture.Host, point, DockProperties.IsDropAreaProperty));
        Assert.Same(fixture.Host, DockHelpers.ResolveDockControl(target));
        fixture.Host.UnregisterExternalDockSurface(fixture.Surface);
        Assert.Null(DockHelpers.GetControlIncludingExternal(fixture.Host, point, DockProperties.IsDropAreaProperty));
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Hidden_Or_Detached_External_Root_Is_Not_A_Drop_Target(int change)
    {
        using Fixture fixture = new(false);
        DockProperties.SetIsDropArea(fixture.Target, true);
        Point point = fixture.Host.PointToClient(fixture.Target.PointToScreen(new Point(10, 10)));
        Assert.Same(fixture.Target, DockHelpers.GetExternalControl(fixture.Host, point, DockProperties.IsDropAreaProperty));
        if (change == 0) fixture.Root.IsVisible = false;
        else if (change == 1) fixture.Surface.IsVisible = false;
        else if (change == 2) fixture.Host.IsVisible = false;
        else fixture.Root.Content = null;
        fixture.Root.UpdateLayout();
        Assert.Null(DockHelpers.GetExternalControl(fixture.Host, point, DockProperties.IsDropAreaProperty));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Adorner_Uses_Registered_Docking_Viewport_And_Real_Owner(bool embedded)
    {
        using Fixture fixture = new(embedded);
        AdornerHelper<DockTarget> helper = new(true);
        try
        {
            helper.AddAdorner(fixture.Target, false);
            Dispatcher.UIThread.RunJobs();
            DockAdornerWindow window = Assert.IsAssignableFrom<DockAdornerWindow>(TopLevel.GetTopLevel(helper.Adorner!));
            Assert.Same(embedded ? fixture.Owner : fixture.Root, window.Owner);
            Point origin = fixture.Host.PointToClient(fixture.Target.PointToScreen(default));
            Rect extent = new Rect(fixture.Host.Bounds.Size).Union(new Rect(origin, fixture.Target.Bounds.Size));
            Assert.Equal(fixture.Host.PointToScreen(extent.Position), window.Position);
            Assert.Equal(extent.Size, new Size(window.Width, window.Height));
            Assert.Equal(origin.X - extent.X, helper.Adorner!.Margin.Left);
            Assert.Equal(origin.Y - extent.Y, helper.Adorner.Margin.Top);
            Assert.True(new Rect(window.ClientSize).Contains(new Rect(helper.Adorner.Margin.Left,
                helper.Adorner.Margin.Top, fixture.Target.Bounds.Width, fixture.Target.Bounds.Height)));
        }
        finally { helper.RemoveAdorner(fixture.Target); }
    }

    [AvaloniaTheory]
    [InlineData(false, 1, 2, 1)]
    [InlineData(false, 2, 1, 1.5)]
    [InlineData(true, 1, 2, 1)]
    [InlineData(true, 2, 1, 1.5)]
    public void Floating_Adorner_Converts_Both_Corners_Across_Root_Scaling(bool embedded, double ownerScale, double surfaceScale, double overlayScale)
    {
        using Fixture fixture = new(embedded);
        fixture.Owner.SetRenderScaling(ownerScale);
        fixture.Root.SetRenderScaling(surfaceScale);
        fixture.Owner.UpdateLayout(); fixture.Root.UpdateLayout();
        AdornerHelper<DockTarget> helper = new(true);
        try
        {
            helper.AddAdorner(fixture.Target, false);
            DockAdornerWindow window = Assert.IsAssignableFrom<DockAdornerWindow>(TopLevel.GetTopLevel(helper.Adorner!));
            window.SetRenderScaling(overlayScale);
            helper.UpdateGeometry();
            window.UpdateLayout();
            PixelPoint first = fixture.Target.PointToScreen(default);
            PixelPoint last = fixture.Target.PointToScreen(new Point(fixture.Target.Bounds.Width, fixture.Target.Bounds.Height));
            Point renderedFirst = window.PointToClient(first);
            Point renderedLast = window.PointToClient(last);
            Assert.Equal(renderedFirst.X, helper.Adorner!.Margin.Left, 3);
            Assert.Equal(renderedFirst.Y, helper.Adorner.Margin.Top, 3);
            Assert.Equal(renderedLast.X - renderedFirst.X, helper.Adorner.Width, 3);
            Assert.Equal(renderedLast.Y - renderedFirst.Y, helper.Adorner.Height, 3);
            PixelPoint viewportFirst = fixture.Host.PointToScreen(default);
            PixelPoint viewportLast = fixture.Host.PointToScreen(new Point(fixture.Host.Bounds.Width, fixture.Host.Bounds.Height));
            double physicalWidth = Math.Max(last.X, viewportLast.X) - Math.Min(first.X, viewportFirst.X);
            double physicalHeight = Math.Max(last.Y, viewportLast.Y) - Math.Min(first.Y, viewportFirst.Y);
            Assert.Equal(physicalWidth / window.RenderScaling, window.Width, 3);
            Assert.Equal(physicalHeight / window.RenderScaling, window.Height, 3);
        }
        finally { helper.RemoveAdorner(fixture.Target); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expanded_Adorner_Uses_Overlay_Dips_For_Host_And_Selector_Margins(bool embedded)
    {
        using Fixture fixture = new(embedded);
        fixture.Root.SetRenderScaling(2);
        fixture.Root.UpdateLayout();
        DockTarget.SetAdornerHostProvider(fixture.Target, new ExpandedHost(fixture.Surface));
        AdornerHelper<DockTarget> helper = new(true);
        try
        {
            helper.AddAdorner(fixture.Target, false);
            DockAdornerWindow window = Assert.IsAssignableFrom<DockAdornerWindow>(TopLevel.GetTopLevel(helper.Adorner!));
            window.SetRenderScaling(1.5);
            helper.UpdateGeometry();
            window.UpdateLayout();
            PixelPoint hostFirst = fixture.Surface.PointToScreen(default);
            PixelPoint hostLast = fixture.Surface.PointToScreen(new Point(fixture.Surface.Bounds.Width, fixture.Surface.Bounds.Height));
            PixelPoint targetFirst = fixture.Target.PointToScreen(default);
            PixelPoint targetLast = fixture.Target.PointToScreen(new Point(fixture.Target.Bounds.Width, fixture.Target.Bounds.Height));
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            Assert.Equal(hostFirst, window.Position);
            Assert.Equal((hostLast.X - hostFirst.X) / 1.5, target.Width, 3);
            Assert.Equal((hostLast.Y - hostFirst.Y) / 1.5, target.Height, 3);
            Assert.Equal(new Thickness((targetFirst.X - hostFirst.X) / 1.5, (targetFirst.Y - hostFirst.Y) / 1.5,
                (hostLast.X - targetLast.X) / 1.5, (hostLast.Y - targetLast.Y) / 1.5), target.TargetMargin);
        }
        finally { helper.RemoveAdorner(fixture.Target); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_Adorner_Converts_External_Root_Geometry_And_Expanded_Host(bool expanded)
    {
        using Fixture fixture = new(true);
        Factory factory = new();
        var root = factory.CreateRootDock();
        root.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
        factory.InitLayout(root);
        fixture.Host.Layout = root;
        ManagedWindowLayer layer = new() { Dock = root };
        fixture.Host.Template = new FuncControlTemplate<DockControl>((_, _) => layer);
        fixture.Owner.SetRenderScaling(1.5);
        fixture.Root.SetRenderScaling(2);
        fixture.Owner.UpdateLayout(); fixture.Root.UpdateLayout();
        if (expanded) DockTarget.SetAdornerHostProvider(fixture.Target, new ExpandedHost(fixture.Surface));
        AdornerHelper<DockTarget> helper = new(true);
        try
        {
            helper.AddAdorner(fixture.Target, false);
            fixture.Owner.UpdateLayout();
            Control host = expanded ? fixture.Surface : fixture.Target;
            DockTarget target = Assert.IsType<DockTarget>(helper.Adorner);
            Assert.Same(fixture.Owner, TopLevel.GetTopLevel(target));
            PixelPoint first = host.PointToScreen(default);
            PixelPoint last = host.PointToScreen(new Point(host.Bounds.Width, host.Bounds.Height));
            Assert.Equal(first, target.PointToScreen(default));
            Assert.Equal(last, target.PointToScreen(new Point(target.Bounds.Width, target.Bounds.Height)));
            if (expanded)
            {
                PixelPoint local = fixture.Target.PointToScreen(default);
                Assert.Equal((local.X - first.X) / fixture.Owner.RenderScaling, target.TargetMargin.Left, 3);
                Assert.Equal((local.Y - first.Y) / fixture.Owner.RenderScaling, target.TargetMargin.Top, 3);
            }
        }
        finally { helper.RemoveAdorner(fixture.Target); }
    }

    private sealed class ExpandedHost(Control host) : IDockAdornerHostProvider
    {
        public Control? GetAdornerHost(Visual surface) => host;
    }

    [AvaloniaFact]
    public void Root_Host_Mode_And_Global_Target_Are_Resolved_From_An_External_Surface()
    {
        using Fixture fixture = new(true);
        Factory factory = new();
        var pane = factory.CreateDocumentDock();
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(pane);
        root.ActiveDockable = pane;
        root.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
        factory.InitLayout(root);
        fixture.Host.Layout = root;
        Assert.Equal(DockFloatingWindowHostMode.Managed, DockHelpers.ResolveFloatingWindowHostMode(fixture.Target));
        Assert.Same(pane, GlobalDockingService.Instance.ResolveGlobalTargetDock(fixture.Target));
    }

    [AvaloniaFact]
    public void Registered_Native_Surface_Takes_Precedence_Over_Content_Below_It()
    {
        using Fixture fixture = new(false);
        Border local = new() { Background = Brushes.Transparent };
        fixture.Host.Template = new FuncControlTemplate<DockControl>((_, _) => local);
        fixture.Owner.UpdateLayout();
        ((Window)fixture.Root).Position = fixture.Host.PointToScreen(new Point(20, 20));
        DockProperties.SetIsDropArea(local, true);
        DockProperties.SetIsDropArea(fixture.Target, true);
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Point point = fixture.Host.PointToClient(fixture.Target.PointToScreen(new Point(10, 10)));
        Assert.Same(fixture.Target, DockHelpers.GetControlIncludingExternal(fixture.Host, point, DockProperties.IsDropAreaProperty));
        fixture.Host.UnregisterExternalDockSurface(fixture.Surface);
        Assert.Same(local, DockHelpers.GetControlIncludingExternal(fixture.Host, point, DockProperties.IsDropAreaProperty));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_Tab_Drag_Captures_Owner_And_Restores_The_Live_Tab_Preview(bool document)
    {
        using Fixture fixture = new(false);
        Factory factory = new();
        IDock pane = document ? factory.CreateDocumentDock() : factory.CreateToolDock();
        IDockable first = document ? factory.CreateDocument() : factory.CreateTool();
        IDockable second = document ? factory.CreateDocument() : factory.CreateTool();
        first.Title = "First tab"; first.CanFloat = false;
        second.Title = "Second tab";
        pane.VisibleDockables = factory.CreateList<IDockable>(first, second);
        pane.ActiveDockable = first;
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(pane); root.ActiveDockable = pane;
        factory.InitLayout(root);
        fixture.Host.Layout = root;
        ItemsControl tabs = document ? new DocumentTabStrip() : new ToolTabStrip();
        tabs.DataContext = pane;
        tabs.ItemsSource = pane.VisibleDockables;
        tabs.Width = 170; tabs.Height = 35;
        fixture.Surface.Children.Clear();
        fixture.Surface.Children.Add(tabs);
        Dispatcher.UIThread.RunJobs(); fixture.Root.UpdateLayout(); fixture.Owner.UpdateLayout();
        Control tab = tabs.ContainerFromIndex(0)!;
        Window external = (Window)fixture.Root;
        Point start = tab.TranslatePoint(new Point(10, 12), external)!.Value;
        IPointer? pointer = null;
        tab.AddHandler(InputElement.PointerPressedEvent, (_, args) => pointer = args.Pointer, handledEventsToo: true);
        external.MouseMove(start);
        external.MouseDown(start, MouseButton.Left);
        Assert.NotNull(pointer);
        pointer!.Capture(tab);
        external.MouseMove(start + new Vector(0, 80), RawInputModifiers.LeftMouseButton);
        Assert.Same(fixture.Host, pointer.Captured);
        Assert.False(tab.IsVisible);
        Assert.Same(second, pane.ActiveDockable);
        var state = (DockControlState)fixture.Host.DockControlState;
        state.Process(default, default, EventType.CaptureLost, DragAction.None, fixture.Host, factory.DockControls);
        pointer.Capture(null);
        external.MouseUp(start + new Vector(0, 80), MouseButton.Left);
        Assert.True(tab.IsVisible);
        Assert.Same(first, pane.ActiveDockable);
        Assert.Equal(new[] { first, second }, pane.VisibleDockables);
    }

    private sealed class Fixture : IDisposable
    {
        internal DockControl Host { get; } = new() { Width = 500, Height = 300, Background = Brushes.Transparent };
        internal Surface Surface { get; } = new() { Width = 180, Height = 120, Background = Brushes.Transparent };
        internal Border Target { get; } = new() { Width = 80, Height = 50, Background = Brushes.Blue };
        internal Window Owner { get; }
        internal TopLevel Root { get; }

        internal Fixture(bool embedded)
        {
            Owner = new Window { Content = Host, Width = 600, Height = 400, Position = new PixelPoint(90, 80) };
            Owner.Show();
            Owner.UpdateLayout();
            Canvas.SetLeft(Target, 30);
            Canvas.SetTop(Target, 20);
            Surface.Children.Add(Target);
            if (embedded)
            {
                EmbeddableControlRoot root = new(PlatformManager.CreateWindow())
                { Content = Surface };
                root.Prepare();
                root.StartRendering();
                Root = root;
            }
            else
            {
                Window root = new() { Content = Surface, Width = 240, Height = 180, Position = new PixelPoint(760, 240) };
                root.Show();
                Root = root;
            }
            Root.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Host.RegisterExternalDockSurface(Surface);
        }

        public void Dispose()
        {
            if (Root is Window window) window.Close();
            else ((EmbeddableControlRoot)Root).Dispose();
            Owner.Close();
        }
    }

    private sealed class Surface : Canvas, IExternalDockSurface
    {
        public DockControl? DockControl { get; set; }
        public Control SurfaceControl => this;
    }
}
