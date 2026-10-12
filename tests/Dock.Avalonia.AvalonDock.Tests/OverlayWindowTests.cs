using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class OverlayWindowTests
{
    [AvaloniaTheory]
    [InlineData(DropAreaType.DocumentPane, DropTargetType.DocumentPaneDockRight)]
    [InlineData(DropAreaType.AnchorablePane, DropTargetType.AnchorablePaneDockRight)]
    [InlineData(DropAreaType.DockingManager, DropTargetType.DockingManagerDockRight)]
    public void Custom_Template_Operations_And_Replacement_Match_Native_Hit_Testing(DropAreaType kind, DropTargetType expected)
    {
        using Workspace workspace = new(kind);
        Border selector = new() { Width = 60, Height = 60 };
        Panel indicator = new() { IsHitTestVisible = false };
        DockProperties.SetIndicatorDockOperation(selector, DockOperation.Right);
        DockProperties.SetIndicatorDockOperation(indicator, DockOperation.Right);
        workspace.Target.Template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register("PART_LeftSelector", selector);
            scope.Register("PART_LeftIndicator", indicator);
            return new StackPanel
            {
                Children =
                {
                    new Panel { Children = { indicator, selector } },
                    // A nested namescope/template can reuse a familiar name without
                    // registering a selector with the owning native dock target.
                    new Border { Name = "PART_CenterSelector", Width = 60, Height = 60 }
                }
            };
        });
        workspace.Update();
        Assert.Same(selector, workspace.Target.GetSelector(DockOperation.Right));
        Assert.Null(workspace.Target.GetSelector(DockOperation.Left));
        PixelPoint point = selector.PointToScreen(new Point(30, 30));
        Assert.Equal(DockOperation.Right, workspace.Target.GetDockOperation(workspace.Surface.PointToClient(point),
            workspace.Surface, workspace.Surface, DragAction.Move, (_, _, _, _) => true));
        OverlayWindowDropTarget snapshot = Assert.Single(workspace.Overlay.DropTargets);
        Assert.Equal(expected, snapshot.TargetType);
        Assert.Equal(ScreenBounds(selector), snapshot.ScreenDetectionArea);

        workspace.Target.Template = new FuncControlTemplate((_, _) => new Border());
        workspace.Update();
        Assert.Empty(workspace.Overlay.DropTargets);
        Assert.Null(workspace.Target.GetSelector(DockOperation.Right));
        Assert.Equal(expected, snapshot.TargetType);
        Assert.Equal(new Size(60, 60), snapshot.ScreenDetectionArea.Size);
    }

    [AvaloniaTheory]
    [InlineData(DropAreaType.DocumentPane)]
    [InlineData(DropAreaType.AnchorablePane)]
    [InlineData(DropAreaType.DocumentPaneGroup)]
    [InlineData(DropAreaType.DockingManager)]
    public void Overlay_Projections_Describe_Actual_Native_Areas_And_Selectors(DropAreaType kind)
    {
        using Workspace workspace = new(kind);
        IDropArea area = workspace.Overlay.DropArea!;
        Assert.Equal(kind, area.Type);
        Assert.Equal(ScreenBounds(workspace.Surface), area.DetectionRect);
        Assert.Equal(area.DetectionRect, workspace.Overlay.Area!.ScreenDetectionArea);
        if (kind == DropAreaType.AnchorablePane)
        {
            Assert.IsType<AnchorablePaneControlOverlayArea>(workspace.Overlay.Area);
            Assert.Same(workspace.Surface, Assert.IsType<DropArea<LayoutAnchorablePaneControl>>(area).AreaElement);
        }
        else if (kind is DropAreaType.DocumentPane or DropAreaType.DocumentPaneGroup)
            Assert.IsType<DocumentPaneControlOverlayArea>(workspace.Overlay.Area);
        else Assert.IsType<DockingManagerOverlayArea>(workspace.Overlay.Area);

        var operations = kind == DropAreaType.DocumentPaneGroup ? new[] { DockOperation.Fill } :
            kind == DropAreaType.DocumentPane ? new[] { DockOperation.Left, DockOperation.Top, DockOperation.Right,
                DockOperation.Bottom, DockOperation.Fill, DockOperation.OuterLeft, DockOperation.OuterTop,
                DockOperation.OuterRight, DockOperation.OuterBottom } :
            new[] { DockOperation.Left, DockOperation.Top, DockOperation.Right, DockOperation.Bottom, DockOperation.Fill };
        int count = kind == DropAreaType.DockingManager ? 4 : operations.Length;
        for (int index = 0; index < count; index++)
        {
            DockOperation expected = operations[index];
            Control selector = workspace.Selector(expected);
            PixelPoint point = selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2));
            DockOperation actual = workspace.Target.GetDockOperation(workspace.Surface.PointToClient(point), workspace.Surface,
                workspace.Surface, DragAction.Move, (_, _, _, _) => true);
            Assert.Equal(expected, actual);
            OverlayWindowDropTarget snapshot = workspace.Overlay.DropTargets.Single(target => target.ScreenDetectionArea.Contains(new Point(point.X, point.Y)));
            Assert.Equal(ScreenBounds(selector), snapshot.ScreenDetectionArea);
            int selectorBase = kind switch
            {
                DropAreaType.DockingManager => 0, DropAreaType.AnchorablePane => 9, _ => 4
            };
            int targetBase = kind switch
            {
                DropAreaType.DockingManager => 0, DropAreaType.AnchorablePane => 10, _ => 4
            };
            int offset = expected == DockOperation.Fill ? 4 : index >= 5 ? index - 5 : index;
            Assert.Equal((OverlayWindowDropTargetType)(selectorBase + offset), snapshot.Type);
            string prefix = kind switch
            {
                DropAreaType.DockingManager => "DockingManagerDock", DropAreaType.AnchorablePane => "AnchorablePaneDock",
                _ => "DocumentPaneDock"
            };
            Assert.Equal(prefix + (expected == DockOperation.Fill ? "Inside" : expected.ToString().Replace("Outer", "")), snapshot.Type.ToString());
            Assert.Equal(kind == DropAreaType.DocumentPaneGroup && expected == DockOperation.Fill
                ? DropTargetType.DocumentPaneGroupDockInside : index >= 5
                    ? (DropTargetType)((int)DropTargetType.DocumentPaneDockAsAnchorableLeft + index - 5)
                    : (DropTargetType)(targetBase + index), snapshot.TargetType);
        }
        Assert.Equal(count, workspace.Overlay.DropTargets.Count);
        workspace.Target.ShowHorizontalTargets = false;
        workspace.Target.GetDockOperation(default, workspace.Surface, workspace.Surface, DragAction.Move,
            (_, _, _, _) => true, (_, operation, _, _) => operation is not (DockOperation.Top or DockOperation.OuterTop));
        Assert.DoesNotContain(workspace.Overlay.DropTargets, target => target.Type.ToString().EndsWith("Left", StringComparison.Ordinal)
            || target.Type.ToString().EndsWith("Right", StringComparison.Ordinal) || target.Type.ToString().EndsWith("Top", StringComparison.Ordinal));
        workspace.Overlay.Close();
        Assert.Null(workspace.Overlay.DropArea);
        Assert.Null(workspace.Overlay.Area);
        Assert.Empty(workspace.Overlay.DropTargets);
        Assert.Null(workspace.Overlay.Content);
    }

    [AvaloniaTheory]
    [InlineData(DropAreaType.DockingManager, DockOperation.Right, "DockingManager", "Right")]
    [InlineData(DropAreaType.AnchorablePane, DockOperation.Bottom, "AnchorablePane", "Bottom")]
    [InlineData(DropAreaType.DocumentPane, DockOperation.Fill, "DocumentPane", "Into")]
    [InlineData(DropAreaType.DocumentPaneGroup, DockOperation.Fill, "DocumentPane", "Into")]
    public void Window_Template_Parts_Drive_Native_Hit_Testing_And_Projected_Preview(
        DropAreaType kind, DockOperation operation, string prefix, string direction)
    {
        using Workspace workspace = new(kind);
        Border selector = new() { Width = 51, Height = 37, RenderTransform = new ScaleTransform(1.25, 1.5) };
        Path preview = new() { Fill = Brushes.Blue };
        Grid group = new() { Children = { selector } };
        Canvas canvas = new() { Children = { preview, group } };
        workspace.Overlay.Template = new FuncControlTemplate((owner, scope) =>
        {
            scope.Register("PART_DropTargetsContainer", canvas);
            scope.Register("PART_" + prefix + "DropTargets", group);
            scope.Register("PART_" + prefix + "DropTarget" + direction, selector);
            scope.Register("PART_PreviewBox", preview);
            ContentPresenter presenter = new() { Content = ((OverlayWindow)owner).Content,
                HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch };
            scope.Register("PART_NativeTargetPresenter", presenter);
            return new Panel { Children = { presenter, canvas } };
        });
        workspace.Update();
        Assert.Same(selector, workspace.Target.GetSelector(operation));
        Assert.Equal(workspace.Surface.Bounds.Size, group.Bounds.Size);
        PixelPoint point = selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2));
        Assert.Equal(operation, workspace.Target.GetDockOperation(workspace.Surface.PointToClient(point),
            workspace.Surface, workspace.Surface, DragAction.Move, (_, candidate, _, _) => candidate == operation));
        Assert.True(preview.IsVisible);
        Rect projected = new(17, 23, 81, 63);
        workspace.Target.SetPreviewBounds(projected);
        workspace.Update();
        Assert.Equal(projected, Assert.IsType<RectangleGeometry>(preview.Data).Rect);
        Assert.Equal(ScreenBounds(selector), Assert.Single(workspace.Overlay.DropTargets).ScreenDetectionArea);
        ((IDockTarget)workspace.Target).Reset();
        Assert.False(preview.IsVisible);
        Assert.Empty(workspace.Overlay.DropTargets);
        workspace.Overlay.Template = new FuncControlTemplate((owner, _) =>
            new ContentPresenter { Content = ((OverlayWindow)owner).Content });
        workspace.Update();
        Assert.Null(workspace.Target.Presentation);
        Assert.Null(preview.Data);
        Assert.NotSame(selector, workspace.Target.GetSelector(operation));
    }

    [AvaloniaFact]
    public void Native_Only_Window_Templates_Can_Be_Replaced_Without_Retaining_The_Target()
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        ContentPresenter retired = new() { Content = workspace.Target };
        workspace.Overlay.Template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register("PART_NativeTargetPresenter", retired);
            return retired;
        });
        workspace.Update();
        Assert.Null(workspace.Target.Presentation);
        Assert.Same(retired, workspace.Target.GetVisualParent());
        ContentPresenter replacement = new() { Content = workspace.Target };
        workspace.Overlay.Template = new FuncControlTemplate((_, scope) =>
        {
            scope.Register("PART_NativeTargetPresenter", replacement);
            return replacement;
        });
        workspace.Update();
        Assert.Null(retired.Child);
        Assert.Same(replacement, workspace.Target.GetVisualParent());
        workspace.Overlay.Close();
        Assert.Null(replacement.Child);
        Assert.Null(workspace.Target.GetVisualParent());
    }

    [AvaloniaFact]
    public void Full_Document_Grid_Follows_Outer_Policy_And_Drops_Release_All_Parts()
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        Grid normal = workspace.Overlay.GetVisualDescendants().OfType<Grid>().Single(control => control.Name == "PART_DocumentPaneDropTargets");
        Grid full = workspace.Overlay.GetVisualDescendants().OfType<Grid>().Single(control => control.Name == "PART_DocumentPaneFullDropTargets");
        Assert.Equal(1, normal.Opacity);
        Assert.Equal(0, full.Opacity);
        workspace.Target.GetDockOperation(default, workspace.Surface, workspace.Surface, DragAction.Move,
            (_, _, _, _) => true, (_, operation, _, _) => operation == DockOperation.OuterRight);
        Assert.Equal(0, normal.Opacity);
        Assert.Equal(1, full.Opacity);
        Assert.Equal(DropTargetType.DocumentPaneDockAsAnchorableRight, Assert.Single(workspace.Overlay.DropTargets).TargetType);
        workspace.Target.GetDockOperation(default, workspace.Surface, workspace.Surface, DragAction.Move,
            (_, _, _, _) => true, (_, operation, _, _) => operation == DockOperation.Fill);
        Assert.Equal(1, normal.Opacity);
        Assert.Equal(0, full.Opacity);
        Assert.Equal(DropTargetType.DocumentPaneDockInside, Assert.Single(workspace.Overlay.DropTargets).TargetType);
        workspace.Overlay.Close();
        Assert.Null(workspace.Target.Presentation);
    }

    [AvaloniaTheory]
    [InlineData("Overlay", 18)]
    [InlineData("LegacyOverlay", 13)]
    public void Both_XML_Namespaces_Compile_Overlay_Themes_And_Stable_Enum_Values(string key, int ordinal)
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        SizingFixture resources = new();
        workspace.Overlay.Theme = (ControlTheme)resources.Resources[key]!;
        workspace.Overlay.OnApplyTemplate();
        workspace.Update();
        Assert.Equal(ordinal, Convert.ToInt32(workspace.Overlay.Tag));
        Assert.False(workspace.Overlay.ShowActivated);
        Assert.False(workspace.Overlay.ShowInTaskbar);
        Assert.False(workspace.Overlay.CanResize);
        Assert.Equal(WindowDecorations.None, workspace.Overlay.WindowDecorations);
        Assert.NotEmpty(workspace.Target.GetVisualDescendants());
    }

    [AvaloniaFact]
    public void Overlay_Windows_Receive_Live_Manager_Resources_And_Release_Them_When_Closed()
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        ResourceDictionary resources = new() { ["OverlayBrush"] = Brushes.Red };
        workspace.Manager.Theme = new ResourceTheme(resources);
        workspace.Overlay.Theme = (ControlTheme)new SizingFixture().Resources["Overlay"]!;
        workspace.Update();
        Assert.Same(Brushes.Red, workspace.Overlay.Background);
        resources["OverlayBrush"] = Brushes.Blue;
        workspace.Update();
        Assert.Same(Brushes.Blue, workspace.Overlay.Background);
        workspace.Manager.Theme = new ResourceTheme(new ResourceDictionary { ["OverlayBrush"] = Brushes.Green });
        workspace.Update();
        Assert.Same(Brushes.Green, workspace.Overlay.Background);
        workspace.Overlay.Close();
        workspace.Manager.Theme = null;
        Assert.False(workspace.Overlay.Resources.TryGetResource("OverlayBrush", null, out _));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Screen_Snapshots_Are_Immutable_And_Include_Transforms_And_Source_Detachment(bool rotated)
    {
        DockingManager manager = new();
        Border surface = new() { Width = 120, Height = 80, RenderTransformOrigin = RelativePoint.TopLeft,
            RenderTransform = rotated ? new RotateTransform(90) : new ScaleTransform(2, 2) };
        Window owner = new() { Width = 400, Height = 300, Content = surface };
        owner.Show(); owner.UpdateLayout();
        DockTarget target = new();
        OverlayWindow overlay = Assert.IsType<OverlayWindow>(DockAdornerWindow.GetFactory(manager)!.CreateWindow(surface, target));
        try
        {
            IDropArea first = overlay.DropArea!;
            Assert.Equal(rotated ? 80 : 240, first.DetectionRect.Width);
            Assert.Equal(rotated ? 120 : 160, first.DetectionRect.Height);
            surface.RenderTransform = new ScaleTransform(1, 1);
            owner.UpdateLayout();
            Assert.Equal(120, overlay.DropArea!.DetectionRect.Width);
            Assert.Equal(rotated ? 80 : 240, first.DetectionRect.Width);
            owner.Content = null;
            Assert.Equal(default, overlay.DropArea!.DetectionRect);
        }
        finally { overlay.Close(); owner.Close(); }
    }

    [AvaloniaFact]
    public void Floating_Content_Inherits_The_Manager_Overlay_Factory()
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        LayoutDocument document = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().First();
        document.Float(); workspace.Update();
        LayoutFloatingWindowControl host = workspace.Manager.FloatingWindows.Single();
        Assert.Same(DockAdornerWindow.GetFactory(workspace.Manager), DockAdornerWindow.GetFactory(host));
        Control pane = host.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
        Assert.Same(DockAdornerWindow.GetFactory(workspace.Manager), DockAdornerWindow.GetFactory(pane));
        document.Dock(); workspace.Update();
    }

    [AvaloniaFact]
    public void Canceling_Overlay_Close_Preserves_The_Active_Projection_Until_Close_Completes()
    {
        using Workspace workspace = new(DropAreaType.DocumentPane);
        void Cancel(object? sender, WindowClosingEventArgs args) => args.Cancel = true;
        workspace.Overlay.Closing += Cancel;
        workspace.Overlay.Close();
        Assert.True(workspace.Overlay.IsVisible);
        Assert.NotNull(workspace.Overlay.DropArea);
        Assert.Same(workspace.Target, workspace.Overlay.Content);
        workspace.Overlay.Closing -= Cancel;
        workspace.Overlay.Close();
        Assert.Null(workspace.Overlay.DropArea);
        Assert.Null(workspace.Overlay.Content);
    }

    [AvaloniaFact]
    public async Task Reused_Native_Target_Does_Not_Retain_The_Closed_Overlay_Or_Its_Parts()
    {
        (DockTargetBase target, WeakReference window, WeakReference selector) = CaptureClosedOverlay();
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            await Task.Delay(25);
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        }
        Assert.Null(target.Presentation);
        Assert.False(window.IsAlive);
        Assert.False(selector.IsAlive);
        GC.KeepAlive(target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockTargetBase, WeakReference, WeakReference) CaptureClosedOverlay()
    {
        Workspace workspace = new(DropAreaType.DocumentPane);
        var result = (workspace.Target, new WeakReference(workspace.Overlay),
            new WeakReference(workspace.Target.GetSelector(DockOperation.Fill)!));
        workspace.Dispose();
        Assert.Null(workspace.Target.Parent);
        Assert.Null(workspace.Target.GetVisualParent());
        Assert.Null(workspace.Target.TemplatedParent);
        return result;
    }

    [AvaloniaFact]
    public void Retaining_An_Overlay_Factory_Does_Not_Keep_The_Manager_Alive()
    {
        (IDockAdornerWindowFactory factory, WeakReference manager) = CaptureFactory();
        Dispatcher.UIThread.RunJobs();
        for (int count = 0; count < 3 && manager.IsAlive; count++)
        { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }
        Assert.False(manager.IsAlive);
        Assert.Null(factory.CreateWindow(new Border(), new DockTarget()));
        GC.KeepAlive(factory);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IDockAdornerWindowFactory, WeakReference) CaptureFactory()
    {
        DockingManager manager = new();
        return (DockAdornerWindow.GetFactory(manager)!, new WeakReference(manager));
    }

    private static Rect ScreenBounds(Control control)
    {
        PixelPoint start = control.PointToScreen(default);
        PixelPoint end = control.PointToScreen(new Point(control.Bounds.Width, control.Bounds.Height));
        return new Rect(start.X, start.Y, end.X - start.X, end.Y - start.Y);
    }

    private sealed class ResourceTheme(ResourceDictionary resources) : DictionaryTheme(resources);

    private sealed class Workspace : IDisposable
    {
        public Window Owner { get; }
        public DockingManager Manager { get; }
        public Control Surface { get; }
        public DockTargetBase Target { get; }
        public OverlayWindow Overlay { get; }

        public Workspace(DropAreaType kind)
        {
            LayoutDocumentPane docs = new(new LayoutDocument { Title = "First", Content = "Editor" });
            LayoutDocumentPane other = new(new LayoutDocument { Title = "Second", Content = "Other" });
            LayoutDocumentPaneGroup group = new() { Children = { docs, other } };
            LayoutAnchorablePane tools = new(new LayoutAnchorable { Title = "Tools", Content = "Properties" });
            Manager = new() { Layout = new() { RootPanel = new LayoutPanel { Children = { tools, group } } } };
            Owner = new Window { Width = 1000, Height = 700, Content = Manager };
            Owner.Show(); Owner.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            Surface = kind switch
            {
                DropAreaType.DocumentPane => Manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().First(),
                DropAreaType.AnchorablePane => Manager.GetVisualDescendants().OfType<LayoutAnchorablePaneControl>().Single(),
                DropAreaType.DocumentPaneGroup => Manager.GetVisualDescendants().OfType<LayoutDocumentPaneGroupControl>().Single(),
                _ => Manager.GetVisualDescendants().OfType<DockControl>().Single()
            };
            Target = kind == DropAreaType.DockingManager ? new GlobalDockTarget() : new DockTarget();
            Overlay = (OverlayWindow)DockAdornerWindow.GetFactory(Surface)!.CreateWindow(Surface, Target)!;
            Overlay.Content = Target;
            Overlay.Width = Surface.Bounds.Width; Overlay.Height = Surface.Bounds.Height;
            Overlay.Position = Surface.PointToScreen(default);
            Overlay.SizeToContent = SizeToContent.Manual;
            Overlay.Show(Owner); Update();
        }

        public Control Selector(DockOperation operation)
        {
            return Target.GetSelector(operation)!;
        }

        public void Update() { Dispatcher.UIThread.RunJobs(); Owner.UpdateLayout(); Overlay.UpdateLayout(); }
        public void Dispose() { Overlay.Close(); Manager.Layout.FloatingWindows.Clear(); Owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
