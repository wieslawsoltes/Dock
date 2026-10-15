using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Orientation = Avalonia.Layout.Orientation;
using Xunit;

namespace DockAvalonDockSample.HeadlessTests;

public class DragSourcePreviewTests
{
    public static IEnumerable<object[]> PaneCases()
    {
        foreach (bool tool in new[] { false, true })
            foreach (bool grouped in new[] { false, true })
                foreach (bool vertical in new[] { false, true })
                    yield return new object[] { tool, grouped, vertical };
    }

    [AvaloniaTheory]
    [MemberData(nameof(PaneCases))]
    public void Last_tab_vacates_its_source_and_cancel_restores_layout(bool tool, bool grouped, bool vertical)
    {
        using Workspace workspace = new(tool, grouped, vertical);
        Rect originalSource = workspace.SourceControl.Bounds;
        Rect originalTarget = workspace.TargetControl.Bounds;
        var sizes = workspace.Sizes();
        ILayoutContainer? parent = workspace.Source.Parent;
        int mutations = 0;
        workspace.Manager.DockFactory.DockableRemoved += (_, _) => mutations++;
        workspace.Manager.DockFactory.DockableAdded += (_, _) => mutations++;
        workspace.Manager.DockFactory.DockableClosed += (_, _) => mutations++;
        for (int repeat = 0; repeat < 2; repeat++)
        {
            workspace.StartDrag();
            bool collapses = tool || grouped;
            Assert.Equal(!collapses, workspace.SourceControl.IsEffectivelyVisible);
            Assert.False(IsPresented(Assert.IsType<TextBox>(workspace.Source.Content)));
            if (collapses)
                Assert.True(vertical ? workspace.TargetControl.Bounds.Height > originalTarget.Height + 20
                    : workspace.TargetControl.Bounds.Width > originalTarget.Width + 20);
            else Assert.Equal(originalTarget, workspace.TargetControl.Bounds);
            Assert.Same(parent, workspace.Source.Parent);
            Assert.Single(workspace.Pane.Children);
            Assert.Equal(sizes, workspace.Sizes());
            Assert.Equal(0, mutations);
            Assert.Empty(workspace.Manager.Layout.FloatingWindows);
            workspace.Cancel();
            Assert.True(workspace.SourceControl.IsEffectivelyVisible);
            Assert.Equal(originalSource, workspace.SourceControl.Bounds);
            Assert.Equal(originalTarget, workspace.TargetControl.Bounds);
            Assert.Equal(sizes, workspace.Sizes());
            Assert.Same(workspace.Source, ((ILayoutContentSelector)workspace.Pane).SelectedContent);
            Assert.True(Assert.IsType<TextBox>(workspace.Source.Content).IsEffectivelyVisible);
            Assert.Equal(0, mutations);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Multi_tab_drag_uses_native_tab_hiding_and_restores_selection(bool tool)
    {
        using Workspace workspace = new(tool, true, false, 2);
        Rect original = workspace.SourceControl.Bounds;
        Control tab = workspace.Tab;
        LayoutContent remaining = workspace.Pane.Children.OfType<LayoutContent>().Last();
        workspace.StartDrag();
        Assert.False(tab.IsVisible);
        Assert.Same(remaining, ((ILayoutContentSelector)workspace.Pane).SelectedContent);
        Assert.Equal(original, workspace.SourceControl.Bounds);
        Assert.Equal(2, workspace.Pane.ChildrenCount);
        workspace.Cancel();
        Assert.True(tab.IsVisible);
        Assert.Same(workspace.Source, ((ILayoutContentSelector)workspace.Pane).SelectedContent);
        Assert.Equal(original, workspace.SourceControl.Bounds);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Whole_tool_pane_drag_reflows_both_axes(bool vertical)
    {
        using Workspace workspace = new(true, true, vertical, 2);
        Rect original = workspace.TargetControl.Bounds;
        Control grip = workspace.SourceControl.GetVisualDescendants().OfType<Control>().Single(c => c.Name == "PART_Grip");
        workspace.StartDrag(grip);
        Assert.False(workspace.SourceControl.IsEffectivelyVisible);
        Assert.True(vertical ? workspace.TargetControl.Bounds.Height > original.Height : workspace.TargetControl.Bounds.Width > original.Width);
        Assert.Equal(2, workspace.Pane.ChildrenCount);
        workspace.Cancel();
        Assert.Equal(original, workspace.TargetControl.Bounds);
    }

    [AvaloniaFact]
    public void Application_size_changes_survive_preview_cleanup()
    {
        using Workspace workspace = new(true, false, false);
        LayoutAnchorablePane pane = (LayoutAnchorablePane)workspace.Pane;
        pane.DockWidth = new GridLength(180);
        workspace.Update();
        workspace.StartDrag();
        pane.DockWidth = new GridLength(260);
        workspace.Update();
        Assert.False(workspace.SourceControl.IsEffectivelyVisible);
        workspace.Cancel();
        Assert.Equal(new GridLength(260), pane.DockWidth);
        Assert.Equal(260, workspace.SourceControl.Bounds.Width, 1);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Detaching_the_host_clears_source_presentation(bool tool)
    {
        using Workspace workspace = new(tool, true, false);
        Control source = workspace.SourceControl;
        workspace.StartDrag();
        Assert.False(source.IsVisible);
        workspace.Window.Content = null;
        Assert.True(source.IsVisible);
        workspace.Cancel();
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        Assert.True(workspace.SourceControl.IsEffectivelyVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Drop_commits_into_the_target_after_source_preview_cleanup(bool tool)
    {
        using Workspace workspace = new(tool, true, false);
        workspace.StartDrag();
        Control area = workspace.TargetControl.GetVisualDescendants().OfType<Control>().First(c =>
            DockProperties.GetIsDockTarget(c) && ReferenceEquals(c.DataContext, workspace.Target.DockModel) && c.IsEffectivelyVisible);
        Point point = area.TranslatePoint(new Point(area.Bounds.Width / 2, area.Bounds.Height / 2), workspace.Window)!.Value;
        workspace.Window.MouseMove(point, RawInputModifiers.LeftMouseButton);
        workspace.Update();
        DockTarget target = Assert.Single(workspace.Window.GetVisualDescendants().OfType<DockTarget>());
        Control selector = Assert.IsAssignableFrom<Control>(target.GetSelector(DockOperation.Fill));
        Point drop = workspace.Window.PointToClient(selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2)));
        workspace.Window.MouseMove(drop, RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(drop, MouseButton.Left);
        workspace.Update();
        Assert.False(workspace.Native.IsDraggingDock);
        Assert.Same(workspace.Target, workspace.Source.Parent);
        Assert.Empty(workspace.Pane.Children);
        Assert.False(workspace.Source.IsFloating);
        Assert.True(Assert.IsType<TextBox>(workspace.Source.Content).IsEffectivelyVisible);
        foreach (Control child in workspace.Manager.LayoutRootPanel!.Children)
            if (child is ILayoutControl { Model: ILayoutPanelElement pane }) Assert.Equal(pane.IsVisible, child.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compact_previews_still_vacate_source_and_invalid_drop_restores_it(bool tool)
    {
        using Workspace workspace = new(tool, true, false);
        workspace.Manager.ShowDockablePreviewOnDrag = false;
        workspace.Source.CanFloat = false;
        Rect original = workspace.SourceControl.Bounds;
        workspace.StartDrag();
        Assert.False(workspace.SourceControl.IsEffectivelyVisible);
        workspace.Window.MouseMove(new Point(-50, -50), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(new Point(-50, -50), MouseButton.Left);
        workspace.Update();
        Assert.Same(workspace.Pane, workspace.Source.Parent);
        Assert.True(workspace.SourceControl.IsEffectivelyVisible);
        Assert.Equal(original, workspace.SourceControl.Bounds);
        Assert.Empty(workspace.Manager.Layout.FloatingWindows);
    }

    [AvaloniaFact]
    public void Caller_grid_definition_edits_survive_preview_cleanup()
    {
        using Workspace workspace = new(true, false, false);
        ColumnDefinition definition = workspace.Manager.LayoutRootPanel!.ColumnDefinitions[0];
        workspace.StartDrag();
        definition.Width = new GridLength(260);
        workspace.Update();
        Assert.False(workspace.SourceControl.IsEffectivelyVisible);
        workspace.Cancel();
        Assert.Equal(new GridLength(260), definition.Width);
        Assert.Equal(260, workspace.SourceControl.Bounds.Width, 1);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_content_uses_the_same_source_preview_lifetime(bool tool)
    {
        using Workspace workspace = new(tool, true, false, tool ? 2 : 1);
        if (tool) workspace.Manager.DockFactory.FloatAllDockables(workspace.Source.DockModel);
        else workspace.Source.Float();
        workspace.Update();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        host.UpdateLayout();
        ILayoutContainer? parent = workspace.Source.Parent;
        Control tab = host.GetVisualDescendants().OfType<Control>().Single(c =>
            c is LayoutDocumentTabItem document && ReferenceEquals(document.Model, workspace.Source)
            || c is LayoutAnchorableTabItem anchorable && ReferenceEquals(anchorable.Model, workspace.Source));
        Point start = tab.TranslatePoint(new Point(tab.Bounds.Width / 3, tab.Bounds.Height / 2), host)!.Value;
        host.MouseMove(start);
        host.MouseDown(start, MouseButton.Left);
        host.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        host.MouseMove(start + new Vector(0, tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
        workspace.Update(); host.UpdateLayout();
        DockControl native = Assert.Single(host.GetVisualDescendants().OfType<DockControl>());
        Assert.True(native.IsDraggingDock);
        Assert.False(IsPresented(Assert.IsType<TextBox>(workspace.Source.Content)));
        Assert.Same(parent, workspace.Source.Parent);
        host.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        host.KeyReleaseQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        host.MouseUp(default, MouseButton.Left);
        workspace.Update(); host.UpdateLayout();
        Assert.False(native.IsDraggingDock);
        Assert.True(Assert.IsType<TextBox>(workspace.Source.Content).IsEffectivelyVisible);
        Assert.Same(parent, workspace.Source.Parent);
        Assert.True(host.IsVisible);
    }

    private static bool IsPresented(Control control) => control.IsEffectivelyVisible && TopLevel.GetTopLevel(control) is not null;

    private sealed class Workspace : IDisposable
    {
        internal Window Window { get; }
        internal DockingManager Manager { get; }
        internal LayoutContent Source { get; }
        internal ILayoutContainer Pane { get; }
        internal LayoutDocumentPane Target { get; }
        internal DockControl Native => Manager.GetVisualDescendants().OfType<DockControl>().Single();
        internal Control SourceControl => Manager.GetVisualDescendants().OfType<Control>().Single(c => c is ILayoutControl view && ReferenceEquals(view.Model, Pane));
        internal Control TargetControl => Manager.GetVisualDescendants().OfType<Control>().Single(c => c is ILayoutControl view && ReferenceEquals(view.Model, Target));
        internal Control Tab => Manager.GetVisualDescendants().OfType<Control>().Single(c => c is DocumentTabStripItem or ToolTabStripItem && ReferenceEquals(c.DataContext, Source.DockModel));
        private readonly bool _tool;
        private readonly LayoutPanel _root;
        private int _drags;

        internal Workspace(bool tool, bool grouped, bool vertical, int count = 1)
        {
            _tool = tool;
            Source = tool ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
            Source.Title = "Source";
            Source.Content = new TextBox { Text = "Dragged editor" };
            Target = new LayoutDocumentPane(new LayoutDocument { Title = "Target", Content = "Target content" });
            _root = new LayoutPanel { Orientation = vertical ? Orientation.Vertical : Orientation.Horizontal };
            if (tool)
            {
                LayoutAnchorablePane tools = new((LayoutAnchorable)Source);
                if (count > 1) tools.Children.Add(new LayoutAnchorable { Title = "Remaining", Content = "Remaining content" });
                Pane = tools;
                _root.Children.Add(grouped ? new LayoutAnchorablePaneGroup(tools) : tools);
                _root.Children.Add(Target);
            }
            else
            {
                LayoutDocumentPane documents = new(Source);
                if (count > 1) documents.Children.Add(new LayoutDocument { Title = "Remaining", Content = "Remaining content" });
                Pane = documents;
                if (grouped)
                {
                    LayoutDocumentPaneGroup group = new(documents) { Orientation = _root.Orientation };
                    group.Children.Add(Target);
                    _root.Children.Add(group);
                }
                else { _root.Children.Add(documents); _root.Children.Add(Target); }
            }
            Manager = new DockingManager { Layout = new LayoutRoot { RootPanel = _root } };
            Window = new Window { Width = 1000, Height = 700, Content = Manager };
            Window.Show();
            Source.IsActive = true;
            Update();
        }

        internal ((GridLength, GridLength), double, double)[] Sizes() =>
            _root.Descendents().OfType<LayoutGroupBase>().Select(p =>
                (Lengths(p), p.DockModel.Proportion, p.DockModel.CollapsedProportion)).ToArray();

        private static (GridLength, GridLength) Lengths(LayoutGroupBase group) => group switch
        {
            LayoutPanel p => (p.DockWidth, p.DockHeight),
            LayoutDocumentPane p => (p.DockWidth, p.DockHeight),
            LayoutDocumentPaneGroup p => (p.DockWidth, p.DockHeight),
            LayoutAnchorablePane p => (p.DockWidth, p.DockHeight),
            LayoutAnchorablePaneGroup p => (p.DockWidth, p.DockHeight),
            _ => throw new InvalidOperationException()
        };

        internal void StartDrag(Control? drag = null)
        {
            Control control = drag ?? (_tool && Pane.ChildrenCount == 1
                ? SourceControl.GetVisualDescendants().OfType<Control>().Single(c => c.Name == "PART_Grip")
                : Manager.GetVisualDescendants().OfType<Control>().Single(c =>
                    c is LayoutDocumentTabItem document && ReferenceEquals(document.Model, Source)
                    || c is LayoutAnchorableTabItem tool && ReferenceEquals(tool.Model, Source)));
            // Repeated drags must not synthesize a double-click, whose native
            // document-tab command deliberately floats immediately on press.
            double x = control is LayoutDocumentTabItem ? control.Bounds.Width * (++_drags % 2 == 0 ? 0.75 : 0.25)
                : Math.Min(40, control.Bounds.Width / 2);
            Point start = control.TranslatePoint(new Point(x, control.Bounds.Height / 2), Window)!.Value;
            Window.MouseMove(start);
            Window.MouseDown(start, MouseButton.Left);
            Assert.False(Source.IsFloating, "Press unexpectedly floated the source (double-tap gesture).");
            Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            Window.MouseMove(start + new Vector(0, _tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
            Update();
            Assert.True(Native.IsDraggingDock, $"Drag did not start: {control.GetType().Name}, {control.Bounds}, start {start}, parent {Source.Parent?.GetType().Name}, visible {control.IsEffectivelyVisible}");
        }

        internal void Cancel()
        {
            Window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Window.KeyReleaseQwerty(PhysicalKey.Escape, RawInputModifiers.None);
            Window.MouseUp(default, MouseButton.Left);
            Update();
        }

        internal void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose()
        {
            Cancel();
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
