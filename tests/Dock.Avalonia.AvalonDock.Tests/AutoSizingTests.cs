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
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoSizingTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Auto_Uses_Content_Size_And_Stars_Use_The_Remaining_Space(bool vertical)
    {
        using Workspace workspace = new(vertical);
        double natural = workspace.Natural(workspace.FirstControl);
        Assert.InRange(natural, vertical ? 160 : 220, 300);
        workspace.AssertExtent(workspace.FirstControl, natural);
        workspace.AssertExtent(workspace.SecondControl, workspace.Extent(workspace.Panel) - natural - 6);
        workspace.Window.Width += 100; workspace.Window.Height += 100;
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, natural);
        workspace.AssertExtent(workspace.SecondControl, workspace.Extent(workspace.Panel) - natural - 6);
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.First));
        Assert.Equal(new GridLength(3, GridUnitType.Star), workspace.GetLength(workspace.Second));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Live_Content_Size_Changes_Invalidate_Intrinsic_Measurement(bool vertical)
    {
        using Workspace workspace = new(vertical);
        double before = workspace.Extent(workspace.FirstControl);
        double remaining = workspace.Extent(workspace.SecondControl);
        Border content = (Border)workspace.First.Children[0].Content!;
        if (vertical) content.Height += 75; else content.Width += 75;
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, before + 75);
        workspace.AssertExtent(workspace.SecondControl, remaining - 75);
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.First));
        if (vertical) content.Height -= 100; else content.Width -= 100;
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, before - 25);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void All_Auto_Panes_Leave_Unused_Space_And_Keep_Their_Requests_In_A_Small_Viewport(bool vertical)
    {
        using Workspace workspace = new(vertical);
        workspace.SetLength(workspace.Second, GridLength.Auto);
        workspace.Update();
        double first = workspace.Natural(workspace.FirstControl), second = workspace.Natural(workspace.SecondControl);
        workspace.AssertExtent(workspace.FirstControl, first);
        workspace.AssertExtent(workspace.SecondControl, second);
        Assert.True(workspace.Extent(workspace.Panel) > first + second + 6);
        if (vertical) workspace.Window.Height = 100; else workspace.Window.Width = 100;
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, first);
        workspace.AssertExtent(workspace.SecondControl, second);
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.First));
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.Second));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_Minimums_Constrain_Auto_Without_Replacing_It(bool vertical)
    {
        using Workspace workspace = new(vertical);
        double natural = workspace.Natural(workspace.FirstControl);
        workspace.SetMinimum(workspace.First, 400);
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, 400);
        workspace.AssertExtent(workspace.SecondControl, workspace.Extent(workspace.Panel) - 406);
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.First));
        workspace.SetMinimum(workspace.First, 25);
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, natural);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Preview_Converts_Auto_To_Pixels_Only_When_Committed(bool vertical)
    {
        using Workspace workspace = new(vertical);
        double before = workspace.Extent(workspace.FirstControl), remaining = workspace.Extent(workspace.SecondControl);
        ProportionalStackPanelSplitter splitter = workspace.Splitter;
        Assert.True(splitter.PreviewResize);
        Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), workspace.Window)!.Value;
        Vector delta = vertical ? new Vector(0, 40) : new Vector(40, 0);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + delta);
        workspace.Update();
        Assert.Equal(GridLength.Auto, workspace.GetLength(workspace.First));
        workspace.AssertExtent(workspace.FirstControl, before);
        workspace.Window.MouseUp(start + delta, MouseButton.Left);
        workspace.Update();
        Assert.Equal(new GridLength(before + 40), workspace.GetLength(workspace.First));
        Assert.Equal((remaining - 40) / (before + remaining), workspace.GetLength(workspace.Second).Value, 8);
        workspace.AssertExtent(workspace.FirstControl, before + 40);
        Border content = (Border)workspace.First.Children[0].Content!;
        if (vertical) content.Height += 80; else content.Width += 80;
        workspace.Update();
        workspace.AssertExtent(workspace.FirstControl, before + 40);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Nested_Panels_Measure_Stars_Intrinsically_Inside_An_Auto_Pane(bool vertical, bool perpendicular)
    {
        LayoutAnchorablePane first = Pane("A", 220, 160), second = Pane("B", 140, 100);
        LayoutPanel inner = new()
        {
            Orientation = vertical != perpendicular ? Orientation.Vertical : Orientation.Horizontal,
            DockWidth = GridLength.Auto, DockHeight = GridLength.Auto,
            Children = { first, second }
        };
        first.DockWidth = first.DockHeight = new GridLength(3, GridUnitType.Star);
        second.DockWidth = second.DockHeight = GridLength.Star;
        LayoutPanel outer = new()
        {
            Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal,
            Children = { inner, Pane("C", 80, 70) }
        };
        using Workspace workspace = new(vertical, outer, first, second);
        Panel parent = workspace.Panel.GetVisualAncestors().OfType<IProportionalSplitterHost>().First().Panel;
        Control innerPresenter = parent.Children.Single(child => ReferenceEquals(child.DataContext, inner.DockModel));
        double a = workspace.Natural(workspace.FirstControl), b = workspace.Natural(workspace.SecondControl);
        double expected = perpendicular ? Math.Max(a, b) : a + b + 6;
        workspace.AssertExtent(innerPresenter, expected);
        workspace.AssertExtent(workspace.FirstControl, perpendicular ? expected : a);
        workspace.AssertExtent(workspace.SecondControl, perpendicular ? expected : b);
        if (!perpendicular)
        {
            workspace.SetMinimum(inner, expected + 120);
            workspace.Update();
            workspace.AssertExtent(innerPresenter, expected + 120);
            // Stars keep their natural minima while allocating the larger host.
            double available = expected + 114;
            double secondExpected = Math.Max(b, available / 4);
            workspace.AssertExtent(workspace.SecondControl, secondExpected);
            workspace.AssertExtent(workspace.FirstControl, available - secondExpected);
        }
        Assert.Equal(new GridLength(3, GridUnitType.Star), workspace.GetLength(first));
        Assert.Equal(GridLength.Star, workspace.GetLength(second));
        Assert.Equal(GridLength.Auto, workspace.GetLength(inner));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Serialization_And_Reattachment_Remeasure_Auto_With_The_Original_Content(bool vertical)
    {
        using Workspace workspace = new(vertical);
        Border content = (Border)workspace.First.Children[0].Content!;
        double original = workspace.Extent(workspace.FirstControl);
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter xml = new();
        serializer.Serialize(xml);
        serializer.Deserialize(new StringReader(xml.ToString()));
        workspace.Update();
        LayoutAnchorablePane restored = (LayoutAnchorablePane)workspace.Manager.Layout.RootPanel.Children[0];
        Assert.Same(content, restored.Children[0].Content);
        Assert.Equal(GridLength.Auto, workspace.GetLength(restored));
        workspace.AssertExtent(workspace.ControlFor(restored), original);
        workspace.Window.Content = null; workspace.Update();
        if (vertical) content.Height += 50; else content.Width += 50;
        workspace.Window.Content = workspace.Manager; workspace.Update();
        workspace.AssertExtent(workspace.ControlFor(restored), original + 50);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Groups_Measure_Runtime_Auto_And_Keep_Their_Rebuild_Policy(bool vertical)
    {
        LayoutAnchorablePane first = Pane("A", 220, 160), second = Pane("B", 140, 100);
        LayoutAnchorablePaneGroup group = new()
        {
            Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal, Children = { first, second }
        };
        LayoutPanel root = new() { Children = { group, new LayoutDocumentPane() } };
        using Workspace workspace = new(vertical, root, first, second);
        workspace.Manager.DockFactory.FloatDockable(group.DockModel);
        workspace.Update();
        Window host = workspace.Manager.FloatingWindows.Single();
        host.Width = 800; host.Height = 700;
        workspace.SetLength(first, GridLength.Auto);
        workspace.SetLength(second, new GridLength(3, GridUnitType.Star));
        workspace.Update();
        Control view = View(first);
        double natural = workspace.Natural(view);
        workspace.AssertExtent(view, natural);
        Border content = (Border)first.Children[0].Content!;
        if (vertical) content.Height += 60; else content.Width += 60;
        workspace.Update();
        workspace.AssertExtent(view, natural + 60);
        Assert.Equal(GridLength.Auto, workspace.GetLength(first));
        group.Children.Add(Pane("C", 100, 100));
        workspace.Update();
        Assert.Equal(GridLength.Star, workspace.GetLength(first));
        Assert.Equal(new GridLength(3, GridUnitType.Star), workspace.GetLength(second));

        Control View(LayoutAnchorablePane pane) => host.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collapsed_Auto_Panes_Release_Their_Minimum_And_Remeasure_When_Restored(bool vertical)
    {
        using Workspace workspace = new(vertical);
        LayoutAnchorablePane empty = new() { DockWidth = GridLength.Auto, DockHeight = GridLength.Auto };
        empty.DockModel.IsCollapsable = true;
        workspace.SetMinimum(empty, 300);
        double a = workspace.Extent(workspace.FirstControl), b = workspace.Extent(workspace.SecondControl);
        workspace.Manager.Layout.RootPanel.Children.Insert(0, empty);
        workspace.Update();
        Assert.True(empty.DockModel.IsEmpty);
        workspace.AssertExtent(workspace.FirstControl, a);
        workspace.AssertExtent(workspace.SecondControl, b);
        Border content = new() { Width = 350, Height = 350 };
        empty.Children.Add(new LayoutAnchorable { Title = "Restored", Content = content });
        workspace.Update();
        Control view = workspace.ControlFor(empty);
        workspace.AssertExtent(view, workspace.Natural(view));
        Assert.True(workspace.Extent(view) >= 350);
        workspace.AssertExtent(workspace.SecondControl, b - workspace.Extent(view) - 6);
        Assert.Equal(GridLength.Auto, workspace.GetLength(empty));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selecting_A_Document_Remeasures_Auto_Using_The_Existing_Content_View(bool vertical)
    {
        Border small = new() { Width = 300, Height = 160 }, large = new() { Width = 420, Height = 260 };
        LayoutDocument a = new() { Title = "A", Content = small }, b = new() { Title = "B", Content = large };
        LayoutDocumentPane pane = new() { DockWidth = GridLength.Auto, DockHeight = GridLength.Auto, Children = { a, b } };
        using Workspace workspace = new(vertical);
        workspace.Manager.Layout.RootPanel.Children.Insert(0, pane);
        workspace.Update();
        Control view = workspace.Manager.GetVisualDescendants().OfType<DocumentDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
        a.IsSelected = true;
        workspace.Update();
        double before = workspace.Extent(view);
        Control existing = workspace.Manager.GetLayoutItemFromModel(b)!.View;
        b.IsSelected = true;
        workspace.Update();
        workspace.AssertExtent(view, before + (vertical ? 100 : 120));
        Assert.Contains(existing, view.GetVisualDescendants());
        workspace.SetMinimum(pane, 500);
        workspace.Update();
        workspace.AssertExtent(view, 500);
        Assert.Equal(GridLength.Auto, workspace.GetLength(pane));
    }

    [AvaloniaFact]
    public void Retained_Models_Do_Not_Retain_Discarded_Intrinsic_Sizing_Views()
    {
        var result = ReplaceAutoLayout();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
        Assert.False(result.Panel.TryGetTarget(out _));
        GC.KeepAlive(result.Manager);
        GC.KeepAlive(result.Model);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockingManager Manager, LayoutRoot Model, WeakReference<Control> Panel) ReplaceAutoLayout()
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable { Title = "A", Content = "Data" }) { DockWidth = GridLength.Auto };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(pane) } };
        Window window = new() { Width = 700, Height = 500, Content = manager };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var result = (manager, manager.Layout, new WeakReference<Control>(manager.GetVisualDescendants().OfType<IProportionalSplitterHost>().Single().Panel));
            manager.Layout = new LayoutRoot();
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            window.CaptureRenderedFrame()?.Dispose();
            return result;
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static LayoutAnchorablePane Pane(string id, double width, double height) => new(new LayoutAnchorable
    {
        ContentId = id, Title = id, Content = new Border { Width = width, Height = height }
    });

    private sealed class Workspace : IDisposable
    {
        private readonly bool _vertical;
        public Window Window { get; } = new() { Width = 1000, Height = 800 };
        public DockingManager Manager { get; } = new();
        public LayoutAnchorablePane First { get; }
        public LayoutAnchorablePane Second { get; }
        public Control FirstControl => ControlFor(First);
        public Control SecondControl => ControlFor(Second);
        public Panel Panel => FirstControl.GetVisualAncestors().OfType<IProportionalSplitterHost>().First().Panel;
        public ProportionalStackPanelSplitter Splitter => Panel.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().First();
        public Workspace(bool vertical, LayoutPanel? root = null, LayoutAnchorablePane? first = null, LayoutAnchorablePane? second = null)
        {
            _vertical = vertical;
            First = first ?? Pane("A", 220, 160);
            Second = second ?? Pane("B", 140, 100);
            if (root is null)
            {
                SetLength(First, GridLength.Auto);
                SetLength(Second, new GridLength(3, GridUnitType.Star));
                root = new LayoutPanel { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal, Children = { First, Second } };
            }
            Manager.Layout = new LayoutRoot { RootPanel = root };
            Window.Content = Manager;
            Window.Show(); Update();
        }
        public Control ControlFor(LayoutAnchorablePane pane) => Manager.GetVisualDescendants().OfType<ToolDockControl>()
            .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
        public GridLength GetLength<T>(LayoutPositionableGroup<T> pane) where T : class, ILayoutElement => _vertical ? pane.DockHeight : pane.DockWidth;
        public void SetLength<T>(LayoutPositionableGroup<T> pane, GridLength length) where T : class, ILayoutElement { if (_vertical) pane.DockHeight = length; else pane.DockWidth = length; }
        public void SetMinimum<T>(LayoutPositionableGroup<T> pane, double minimum) where T : class, ILayoutElement { if (_vertical) pane.DockMinHeight = minimum; else pane.DockMinWidth = minimum; }
        public double Extent(Control control) => _vertical ? control.Bounds.Height : control.Bounds.Width;
        public double Natural(Control control) => _vertical ? control.DesiredSize.Height : control.DesiredSize.Width;
        public void AssertExtent(Control control, double expected) => Assert.InRange(Extent(control), expected - 1, expected + 1);
        public void Update()
        {
            Dispatcher.UIThread.RunJobs(); Window.UpdateLayout();
            foreach (Window host in Manager.FloatingWindows) host.UpdateLayout();
        }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
