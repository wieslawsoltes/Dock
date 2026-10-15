using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
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

public class PixelSizingTests
{
    [AvaloniaFact]
    public void Public_Minimums_Start_At_TwentyFive_And_Native_Defaults_Are_Unchanged()
    {
        Check(new LayoutPanel());
        Check(new LayoutAnchorablePane());
        Check(new LayoutDocumentPane());
        Check(new LayoutAnchorablePaneGroup());
        Check(new LayoutDocumentPaneGroup());
        Assert.True(double.IsNaN(new global::Dock.Model.Avalonia.Controls.ToolDock().MinWidth));
        Assert.True(double.IsNaN(new global::Dock.Model.Avalonia.Controls.DocumentDock().MinHeight));
        Assert.True(new DockingManager().DockFactory.CreateProportionalDockSplitter().ResizePreview);
        Assert.False(new global::Dock.Model.Avalonia.Factory().CreateProportionalDockSplitter().ResizePreview);

        static void Check<T>(LayoutPositionableGroup<T> model) where T : class, ILayoutElement
        {
            Assert.Equal(25, model.DockMinWidth); Assert.Equal(25, model.DockMinHeight);
            Assert.Equal(25, model.DockModel.MinWidth); Assert.Equal(25, model.DockModel.MinHeight);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Minimum_Changes_Notify_Public_Bindings_And_Reject_Negative_Values(bool vertical)
    {
        LayoutAnchorablePane model = new();
        string property = vertical ? nameof(model.DockMinHeight) : nameof(model.DockMinWidth);
        List<double> changing = new(), changed = new();
        double Get() => vertical ? model.DockMinHeight : model.DockMinWidth;
        void Set(double value) { if (vertical) model.DockMinHeight = value; else model.DockMinWidth = value; }
        model.PropertyChanging += (_, e) => { if (e.PropertyName == property) changing.Add(Get()); };
        model.PropertyChanged += (_, e) => { if (e.PropertyName == property) changed.Add(Get()); };
        Set(40);
        Assert.Equal(new[] { 25d }, changing);
        Assert.Equal(new[] { 40d }, changed);
        Set(40);
        Assert.Single(changing); Assert.Single(changed);
        if (vertical) model.DockModel.MinHeight = 60; else model.DockModel.MinWidth = 60;
        Assert.Equal(new[] { 40d, 60d }, changed);
        Assert.Throws<ArgumentException>(() => Set(-1));
        Assert.Equal(60, Get());
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void All_Pixel_Panes_Keep_Their_Lengths_And_Leave_Unused_Space(bool vertical, bool floating)
    {
        using Workspace workspace = new(vertical, floating);
        AssertFixed(workspace, 200, 300);
        workspace.Host.Width += 150;
        workspace.Host.Height += 150;
        workspace.Update();
        AssertFixed(workspace, 200, 300);
        if (vertical) workspace.Manager.GridSplitterHeight = 30;
        else workspace.Manager.GridSplitterWidth = 30;
        workspace.Update();
        AssertFixed(workspace, 200, 300);
        ProportionalStackPanelSplitter splitter = workspace.Splitter;
        Assert.Equal(30, workspace.Extent(splitter));
        Assert.InRange(workspace.Start(workspace.SecondControl), workspace.Start(workspace.FirstControl) + 229, workspace.Start(workspace.FirstControl) + 231);
        Assert.True(workspace.Extent(workspace.Panel) > 530);
        workspace.SetLength(workspace.First, new GridLength(230));
        workspace.Update();
        AssertFixed(workspace, 230, 300);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Size_Updates_Restore_Cleared_Definitions_Without_Rewriting_Model_Lengths(bool vertical, bool floating)
    {
        using Workspace workspace = new(vertical, floating);
        Grid grid = Assert.IsAssignableFrom<Grid>(workspace.Panel);
        double proportion = workspace.First.DockModel.Proportion;
        // Definitions are public Grid state. A subsequent model size change
        // recreates removed definitions from the unchanged requested lengths.
        if (vertical) grid.RowDefinitions.Clear();
        else grid.ColumnDefinitions.Clear();
        Assert.Equal(proportion, workspace.First.DockModel.Proportion);
        if (vertical) workspace.First.DockMinHeight++;
        else workspace.First.DockMinWidth++;
        workspace.Update();
        AssertFixed(workspace, 200, 300);
        Assert.Equal(new GridLength(200), vertical ? grid.RowDefinitions[0].Height : grid.ColumnDefinitions[0].Width);
        Assert.Equal(proportion, workspace.First.DockModel.Proportion);
        workspace.SetLength(workspace.First, new GridLength(220));
        workspace.Update();
        AssertFixed(workspace, 220, 300);
    }

    [AvaloniaTheory]
    [InlineData(false, 350)]
    [InlineData(true, 350)]
    [InlineData(false, 4)]
    [InlineData(true, 4)]
    public void A_Constrained_Viewport_Does_Not_Rewrite_Pixel_Requests(bool vertical, double extent)
    {
        using Workspace workspace = new(vertical);
        if (vertical) workspace.Host.Height = extent;
        else workspace.Host.Width = extent;
        workspace.Update();
        AssertFixed(workspace, 200, 300);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Minimums_Affect_Rendering_Without_Replacing_The_Requested_Length(bool vertical)
    {
        using Workspace workspace = new(vertical);
        if (vertical) workspace.First.DockMinHeight = 250;
        else workspace.First.DockMinWidth = 250;
        workspace.Update();
        Assert.Equal(new GridLength(200), workspace.GetLength(workspace.First));
        Assert.InRange(workspace.Extent(workspace.FirstControl), 249, 251);
        Assert.Equal(new GridLength(300), workspace.GetLength(workspace.Second));
        Assert.InRange(workspace.Extent(workspace.SecondControl), 299, 301);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Star_Panes_Share_The_Remainder_After_Fixed_And_Minimum_Lengths(bool vertical)
    {
        LayoutAnchorablePane first = Pane("First"), second = Pane("Second"), third = Pane("Third");
        using Workspace workspace = new(vertical, panes: new[] { first, second, third });
        workspace.SetLength(first, new GridLength(1, GridUnitType.Star));
        workspace.SetLength(second, new GridLength(2, GridUnitType.Star));
        workspace.SetLength(third, new GridLength(1, GridUnitType.Star));
        double total = workspace.Extent(workspace.Panel) - 12;
        if (vertical) first.DockMinHeight = total / 2;
        else first.DockMinWidth = total / 2;
        workspace.Update();
        Assert.InRange(workspace.Extent(workspace.ControlFor(first)), total / 2 - 1, total / 2 + 1);
        Assert.InRange(workspace.Extent(workspace.ControlFor(second)), total / 3 - 1, total / 3 + 1);
        Assert.InRange(workspace.Extent(workspace.ControlFor(third)), total / 6 - 1, total / 6 + 1);
        Assert.Equal(GridLength.Star, workspace.GetLength(first));
        Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.GetLength(second));
        workspace.SetLength(first, new GridLength(100));
        workspace.Update();
        Assert.Equal(new GridLength(100), workspace.GetLength(first));
        Assert.InRange(workspace.Extent(workspace.ControlFor(second)), total / 3 - 1, total / 3 + 1);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Native_Preview_Drag_Commits_Exact_Pixel_Lengths_On_Release(bool vertical, bool floating)
    {
        using Workspace workspace = new(vertical, floating);
        if (vertical) workspace.Manager.GridSplitterHeight = 40;
        else workspace.Manager.GridSplitterWidth = 40;
        workspace.Update();
        ProportionalStackPanelSplitter splitter = workspace.Splitter;
        Assert.True(splitter.PreviewResize);
        Assert.Equal(0, ProportionalStackPanelSplitter.GetMinimumProportionSize(splitter));
        Point start = workspace.BeginDrag(splitter);
        workspace.Host.MouseMove(start + workspace.Vector(60));
        workspace.Update();
        AssertFixed(workspace, 200, 300);
        workspace.Host.MouseUp(start + workspace.Vector(60), MouseButton.Left);
        workspace.Update();
        AssertFixed(workspace, 260, 240);
        workspace.Host.Width += 120; workspace.Host.Height += 120;
        workspace.Update();
        AssertFixed(workspace, 260, 240);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Preview_Capture_Loss_Cancels_The_Resize_And_Removes_The_Adorner(bool vertical)
    {
        using Workspace workspace = new(vertical);
        ProportionalStackPanelSplitter splitter = workspace.Splitter;
        IPointer? pointer = null;
        splitter.AddHandler(InputElement.PointerPressedEvent, (_, e) => pointer = e.Pointer, RoutingStrategies.Bubble, handledEventsToo: true);
        AdornerLayer layer = AdornerLayer.GetAdornerLayer(workspace.Panel)!;
        int before = layer.Children.Count;
        Point start = workspace.BeginDrag(splitter);
        workspace.Host.MouseMove(start + workspace.Vector(60));
        Assert.Equal(before + 1, layer.Children.Count);
        Assert.NotNull(pointer);
        pointer.Capture(null);
        workspace.Host.MouseUp(start + workspace.Vector(60), MouseButton.Left);
        workspace.Update();
        Assert.Equal(before, layer.Children.Count);
        AssertFixed(workspace, 200, 300);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resize_Respects_Public_Minimums_Without_The_Native_SeventyFive_Pixel_Floor(bool vertical)
    {
        using Workspace workspace = new(vertical);
        if (vertical) workspace.First.DockMinHeight = 25;
        else workspace.First.DockMinWidth = 25;
        Point start = workspace.BeginDrag(workspace.Splitter);
        workspace.Host.MouseMove(start + workspace.Vector(-190));
        workspace.Host.MouseUp(start + workspace.Vector(-190), MouseButton.Left);
        workspace.Update();
        AssertFixed(workspace, 25, 475);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resizing_Two_Star_Panes_Preserves_Their_Weight_Sum_And_Other_Panes(bool vertical)
    {
        LayoutAnchorablePane first = Pane("First"), second = Pane("Second"), third = Pane("Third");
        using Workspace workspace = new(vertical, panes: new[] { first, second, third });
        workspace.SetLength(first, new GridLength(2, GridUnitType.Star));
        workspace.SetLength(second, new GridLength(1, GridUnitType.Star));
        workspace.SetLength(third, new GridLength(4, GridUnitType.Star));
        workspace.Update();
        double a = workspace.Extent(workspace.ControlFor(first));
        double b = workspace.Extent(workspace.ControlFor(second));
        double c = workspace.Extent(workspace.ControlFor(third));
        ProportionalStackPanelSplitter splitter = workspace.Splitter;
        Point start = workspace.BeginDrag(splitter);
        workspace.Host.MouseMove(start + workspace.Vector(30));
        workspace.Host.MouseUp(start + workspace.Vector(30), MouseButton.Left);
        workspace.Update();
        Assert.Equal((a + 30) / (a + b) * 3, workspace.GetLength(first).Value, 8);
        Assert.Equal((b - 30) / (a + b) * 3, workspace.GetLength(second).Value, 8);
        Assert.Equal(3, workspace.GetLength(first).Value + workspace.GetLength(second).Value, 8);
        Assert.Equal(new GridLength(4, GridUnitType.Star), workspace.GetLength(third));
        Assert.InRange(workspace.Extent(workspace.ControlFor(third)), c - 1, c + 1);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resizing_A_Mixed_Pair_Persists_The_Upstream_Star_Fraction(bool vertical)
    {
        using Workspace workspace = new(vertical);
        workspace.SetLength(workspace.Second, new GridLength(3, GridUnitType.Star));
        workspace.Update();
        double a = workspace.Extent(workspace.FirstControl), b = workspace.Extent(workspace.SecondControl);
        Point start = workspace.BeginDrag(workspace.Splitter);
        workspace.Host.MouseMove(start + workspace.Vector(40));
        workspace.Host.MouseUp(start + workspace.Vector(40), MouseButton.Left);
        workspace.Update();
        Assert.Equal(new GridLength(240), workspace.GetLength(workspace.First));
        Assert.Equal((b - 40) / (a + b), workspace.GetLength(workspace.Second).Value, 8);
    }

    [AvaloniaFact]
    public void XML_And_Reattachment_Preserve_Fixed_Root_Panel_Lengths()
    {
        using Workspace workspace = new();
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter xml = new();
        serializer.Serialize(xml);
        serializer.Deserialize(new StringReader(xml.ToString()));
        workspace.Update();
        LayoutAnchorablePane[] panes = workspace.Manager.Layout.RootPanel.Children.Cast<LayoutAnchorablePane>().ToArray();
        Assert.Equal(new GridLength(200), panes[0].DockWidth);
        Assert.Equal(new GridLength(300), panes[1].DockWidth);
        workspace.Window.Content = null;
        workspace.Update();
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        Assert.InRange(workspace.Extent(workspace.ControlFor(panes[0])), 199, 201);
        Assert.InRange(workspace.Extent(workspace.ControlFor(panes[1])), 299, 301);
    }

    private static LayoutAnchorablePane Pane(string title) => new(new LayoutAnchorable { ContentId = title, Title = title, Content = title });
    private static void AssertFixed(Workspace workspace, double first, double second)
    {
        Assert.Equal(new GridLength(first), workspace.GetLength(workspace.First));
        Assert.Equal(new GridLength(second), workspace.GetLength(workspace.Second));
        Assert.InRange(workspace.Extent(workspace.FirstControl), first - 1, first + 1);
        Assert.InRange(workspace.Extent(workspace.SecondControl), second - 1, second + 1);
    }

    private sealed class Workspace : IDisposable
    {
        private readonly bool _vertical;
        public Window Window { get; } = new() { Width = 1000, Height = 900 };
        public DockingManager Manager { get; } = new();
        public LayoutAnchorablePane First { get; }
        public LayoutAnchorablePane Second { get; }
        public Window Host { get; }
        public Control Scope => Host == Window ? Manager : Host;
        public Control FirstControl => ControlFor(First);
        public Control SecondControl => ControlFor(Second);
        public Panel Panel => FirstControl.GetVisualAncestors().OfType<IProportionalSplitterHost>().First().Panel;
        public ProportionalStackPanelSplitter Splitter => Panel.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().First();
        public Workspace(bool vertical = false, bool floating = false, LayoutAnchorablePane[]? panes = null)
        {
            _vertical = vertical;
            panes ??= new[] { Pane("First"), Pane("Second") };
            First = panes[0]; Second = panes[1];
            LayoutPanel root = new() { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal };
            LayoutAnchorablePaneGroup? floatingGroup = null;
            if (floating)
            {
                floatingGroup = new() { Orientation = root.Orientation };
                foreach (LayoutAnchorablePane pane in panes) floatingGroup.Children.Add(pane);
                root.Children.Add(floatingGroup);
                root.Children.Add(new LayoutDocumentPane());
            }
            else foreach (LayoutAnchorablePane pane in panes) root.Children.Add(pane);
            SetLength(First, new GridLength(200)); SetLength(Second, new GridLength(300));
            Manager.Layout = new LayoutRoot { RootPanel = root };
            Window.Content = Manager;
            Host = Window;
            Window.Show(); Update();
            if (floating)
            {
                Manager.DockFactory.FloatDockable(floatingGroup!.DockModel);
                Update();
                Host = Manager.FloatingWindows.Single();
                Host.Width = 1000; Host.Height = 900;
                Update();
                // A floating tool group normalizes initial lengths, as upstream.
                SetLength(First, new GridLength(200)); SetLength(Second, new GridLength(300));
                Update();
            }
        }
        public Control ControlFor(LayoutAnchorablePane pane) => Scope.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
        public GridLength GetLength(LayoutAnchorablePane pane) => _vertical ? pane.DockHeight : pane.DockWidth;
        public void SetLength(LayoutAnchorablePane pane, GridLength length) { if (_vertical) pane.DockHeight = length; else pane.DockWidth = length; }
        public double Extent(Control control) => _vertical ? control.Bounds.Height : control.Bounds.Width;
        public double Start(Control control)
        {
            Point point = control.TranslatePoint(default, Panel)!.Value;
            return _vertical ? point.Y : point.X;
        }
        public Vector Vector(double value) => _vertical ? new Vector(0, value) : new Vector(value, 0);
        public Point BeginDrag(ProportionalStackPanelSplitter splitter)
        {
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), Host)!.Value;
            Host.MouseMove(start);
            IInputElement? hit = Host.InputHitTest(start);
            Assert.True(ReferenceEquals(hit, splitter) || hit is Visual visual && visual.GetVisualAncestors().Contains(splitter));
            Host.MouseDown(start, MouseButton.Left);
            return start;
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); Host.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
