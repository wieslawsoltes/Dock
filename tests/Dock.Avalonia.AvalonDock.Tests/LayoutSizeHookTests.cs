using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Controls.ProportionalStackPanel;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutSizeHookTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Changed_Lengths_Notify_Before_The_Hook_And_Equal_Values_Do_Nothing(bool vertical)
    {
        PaneProbe pane = new();
        string property = vertical ? nameof(pane.DockHeight) : nameof(pane.DockWidth);
        string hook = vertical ? "height" : "width";
        pane.PropertyChanging += (_, e) => { if (e.PropertyName == property) pane.History.Add(("changing", Length(pane, vertical))); };
        pane.PropertyChanged += (_, e) => { if (e.PropertyName == property) pane.History.Add(("changed", Length(pane, vertical))); };
        SetLength(pane, vertical, new GridLength(210));
        Assert.Equal(new[] { ("changing", GridLength.Star), ("changed", new GridLength(210)), (hook, new GridLength(210)) }, pane.History);
        pane.History.Clear();
        SetLength(pane, vertical, new GridLength(210));
        Assert.Empty(pane.History);
        SetLength(pane, vertical, GridLength.Auto);
        Assert.Equal(new[] { ("changing", new GridLength(210)), ("changed", GridLength.Auto), (hook, GridLength.Auto) }, pane.History);
    }

    [AvaloniaFact]
    public void A_Hook_Can_Redirect_A_Length_Without_Losing_Later_Notifications()
    {
        PaneProbe pane = new();
        pane.WidthChanged = () => { if (pane.DockWidth.Value == 210) pane.DockWidth = new GridLength(240); };
        pane.DockWidth = new GridLength(210);
        Assert.Equal(new GridLength(240), pane.DockWidth);
        Assert.Equal(new[] { ("width", new GridLength(210)), ("width", new GridLength(240)) }, pane.History);
        pane.DockHeight = new GridLength(180);
        Assert.Equal(("height", new GridLength(180)), pane.History[2]);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Tool_Group_Propagates_Only_Changed_Pixel_Lengths_Through_Single_Child_Groups(bool vertical)
    {
        PaneProbe pane = new();
        GroupProbe inner = new() { Children = { pane } };
        GroupProbe outer = new() { Children = { inner }, Orientation = vertical ? Orientation.Horizontal : Orientation.Vertical };
        List<GridLength> childLengthsDuringParentNotification = new();
        string property = vertical ? nameof(outer.DockHeight) : nameof(outer.DockWidth);
        outer.PropertyChanged += (_, e) => { if (e.PropertyName == property) childLengthsDuringParentNotification.Add(Length(pane, vertical)); };
        SetLength(outer, vertical, new GridLength(320));
        Assert.Equal(new[] { GridLength.Star }, childLengthsDuringParentNotification);
        Assert.Equal(new GridLength(320), Length(inner, vertical));
        Assert.Equal(new GridLength(320), Length(pane, vertical));
        Assert.Equal(Length(pane, vertical), Assert.Single(pane.History).Item2);
        Assert.Same(inner.DockModel, pane.DockModel.Owner);
        Assert.Same(outer.DockModel, inner.DockModel.Owner);
        SetLength(pane, vertical, new GridLength(170));
        SetLength(outer, vertical, new GridLength(320));
        SetLength(outer, vertical, GridLength.Auto);
        SetLength(outer, vertical, new GridLength(2, GridUnitType.Star));
        Assert.Equal(new GridLength(170), Length(pane, vertical));
        Assert.Equal(new GridLength(320), Length(inner, vertical));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Adding_Children_And_Changing_Multi_Child_Or_Other_Group_Lengths_Do_Not_Propagate(bool vertical)
    {
        GroupProbe group = new();
        SetLength(group, vertical, new GridLength(300));
        PaneProbe first = new(), second = new();
        group.Children.Add(first);
        Assert.Equal(GridLength.Star, Length(first, vertical));
        SetLength(group, vertical, new GridLength(300));
        Assert.Equal(GridLength.Star, Length(first, vertical));
        group.Children.Add(second);
        SetLength(group, vertical, new GridLength(340));
        Assert.Equal(GridLength.Star, Length(first, vertical));
        Assert.Equal(GridLength.Star, Length(second, vertical));
        group.Children.Remove(second);
        Assert.Equal(GridLength.Star, Length(first, vertical));
        SetLength(group, vertical, new GridLength(360));
        Assert.Equal(new GridLength(360), Length(first, vertical));

        LayoutDocumentPane document = new();
        LayoutDocumentPaneGroup documents = new() { Children = { document } };
        LayoutPanel panel = new(documents);
        SetLength(panel, vertical, new GridLength(410));
        SetLength(documents, vertical, new GridLength(390));
        Assert.Equal(GridLength.Star, Length(document, vertical));
        Assert.Equal(new GridLength(390), Length(documents, vertical));
    }

    [AvaloniaFact]
    public void XML_Restores_Independent_Lengths_Without_Invoking_Change_Hooks_Or_Modifying_Existing_Children()
    {
        PaneProbe pane = new() { DockWidth = new GridLength(90), DockHeight = new GridLength(120) };
        GroupProbe group = new() { Children = { pane } };
        pane.History.Clear();
        List<string?> notifications = new();
        group.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
        using StringReader text = new("<LayoutAnchorablePaneGroup DockWidth=\"350\" DockHeight=\"260\" />");
        using XmlReader reader = XmlReader.Create(text);
        group.ReadXml(reader);
        Assert.Equal(new GridLength(350), group.DockWidth);
        Assert.Equal(new GridLength(260), group.DockHeight);
        Assert.Equal(new GridLength(90), pane.DockWidth);
        Assert.Equal(new GridLength(120), pane.DockHeight);
        Assert.Empty(group.History);
        Assert.Empty(pane.History);
        Assert.DoesNotContain(nameof(group.DockWidth), notifications);
        Assert.DoesNotContain(nameof(group.DockHeight), notifications);
        group.DockWidth = new GridLength(360);
        Assert.Equal(new GridLength(360), pane.DockWidth);
        Assert.Single(group.History);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void External_Native_Proportion_Changes_Invoke_The_Hook_And_Measurement_Does_Not(bool vertical)
    {
        using Workspace workspace = new(vertical);
        Assert.Empty(workspace.First.History);
        workspace.First.DockModel.Proportion *= 1.2;
        Assert.InRange(Length(workspace.First, vertical).Value, 251.99, 252.01);
        Assert.Single(workspace.First.History);
        workspace.Window.Width += 100;
        workspace.Window.Height += 100;
        workspace.Update();
        Assert.Single(workspace.First.History);
        Assert.InRange(Length(workspace.First, vertical).Value, 251.99, 252.01);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Splitter_Preview_Invokes_The_Hook_Only_When_The_Length_Is_Committed(bool vertical)
    {
        using Workspace workspace = new(vertical);
        ProportionalStackPanelSplitter splitter = workspace.Manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().Single(control => control.IsEffectivelyVisible);
        Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), workspace.Window)!.Value;
        Point end = start + (vertical ? new Vector(0, 35) : new Vector(35, 0));
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(end, RawInputModifiers.LeftMouseButton);
        workspace.Update();
        Assert.Empty(workspace.First.History);
        Assert.Equal(new GridLength(210), Length(workspace.First, vertical));
        workspace.Window.MouseUp(end, MouseButton.Left);
        workspace.Update();
        Assert.Single(workspace.First.History);
        Assert.InRange(Length(workspace.First, vertical).Value, 244, 246);
    }

    private static GridLength Length<T>(LayoutPositionableGroup<T> group, bool vertical) where T : class, ILayoutElement =>
        vertical ? group.DockHeight : group.DockWidth;
    private static void SetLength<T>(LayoutPositionableGroup<T> group, bool vertical, GridLength value) where T : class, ILayoutElement
    {
        if (vertical) group.DockHeight = value; else group.DockWidth = value;
    }

    private sealed class PaneProbe : LayoutAnchorablePane
    {
        public List<(string, GridLength)> History { get; } = new();
        public Action? WidthChanged { get; set; }
        protected override void OnDockWidthChanged() { History.Add(("width", DockWidth)); base.OnDockWidthChanged(); WidthChanged?.Invoke(); }
        protected override void OnDockHeightChanged() { History.Add(("height", DockHeight)); base.OnDockHeightChanged(); }
    }
    private sealed class GroupProbe : LayoutAnchorablePaneGroup
    {
        public List<(string, GridLength)> History { get; } = new();
        protected override void OnDockWidthChanged() { History.Add(("width", DockWidth)); base.OnDockWidthChanged(); }
        protected override void OnDockHeightChanged() { History.Add(("height", DockHeight)); base.OnDockHeightChanged(); }
    }

    private sealed class Workspace : IDisposable
    {
        public SizingFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public PaneProbe First { get; } = new() { Children = { new LayoutAnchorable { Title = "First", Content = "First" } } };
        public Workspace(bool vertical)
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            LayoutAnchorablePane second = new() { Children = { new LayoutAnchorable { Title = "Second", Content = "Second" } } };
            SetLength(First, vertical, new GridLength(210));
            Manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel
            {
                Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal,
                Children = { First, second }
            } };
            First.History.Clear();
            Window.Show(); Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
