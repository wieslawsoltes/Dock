using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Xunit;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.AvalonDock.Tests;

public class GridControlTests
{
    [AvaloniaFact]
    public void Generated_Grids_Expose_Existing_Models_And_Compile_Styles_In_Both_Namespaces()
    {
        using Workspace workspace = new();
        LayoutPanelControl root = Assert.IsType<LayoutPanelControl>(workspace.Manager.LayoutRootPanel);
        LayoutGridControl<ILayoutPanelElement> panelBase = root;
        Assert.Same(workspace.Manager.Layout.RootPanel, panelBase.Model);
        Assert.Contains(root, workspace.Manager.GetVisualDescendants());
        Assert.Same(workspace.Documents, workspace.DocumentGrid.Model);
        Assert.Same(workspace.Tools, workspace.ToolGrid.Model);
        foreach (Grid grid in new Grid[] { root, workspace.DocumentGrid, workspace.ToolGrid })
        {
            LayoutElement model = Assert.IsAssignableFrom<LayoutElement>(((ILayoutControl)grid).Model);
            Assert.Same(model, grid.Tag);
            Assert.Same(model.DockModel, grid.DataContext);
            Assert.Equal(Brushes.Transparent, grid.Background);
            Assert.Equal(3, grid.Children.Count);
            Assert.IsAssignableFrom<ILayoutControl>(grid.Children[0]);
            Assert.IsType<LayoutGridResizerControl>(grid.Children[1]);
            Assert.IsAssignableFrom<ILayoutControl>(grid.Children[2]);
            Assert.All(grid.Children, child => Assert.Same(grid, child.GetVisualParent()));
            bool horizontal = ((ILayoutOrientableGroup)model).Orientation == Orientation.Horizontal;
            Assert.Equal(horizontal ? 3 : 0, grid.ColumnDefinitions.Count);
            Assert.Equal(horizontal ? 0 : 3, grid.RowDefinitions.Count);
        }
        Assert.Equal(2, root.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Count());
        Assert.Equal(2, root.GetVisualDescendants().OfType<LayoutAnchorablePaneControl>().Count());
    }

    [AvaloniaFact]
    public void Public_Definitions_Use_Live_Lengths_Minima_And_Splitter_Thickness()
    {
        using Workspace workspace = new();
        LayoutPanelControl root = workspace.Manager.LayoutRootPanel!;
        Assert.Same(workspace.DocumentGrid, root.Children[0]);
        Assert.Same(workspace.ToolGrid, root.Children[2]);
        Assert.Equal(new GridLength(400), root.ColumnDefinitions[2].Width);
        workspace.Tools.DockWidth = new GridLength(350);
        workspace.Tools.DockMinWidth = 140;
        workspace.Manager.GridSplitterWidth = 12;
        workspace.Update();
        Assert.Equal(new GridLength(350), root.ColumnDefinitions[2].Width);
        Assert.Equal(140, root.ColumnDefinitions[2].MinWidth);
        Assert.Equal(new GridLength(12), root.ColumnDefinitions[1].Width);
        Assert.Equal(12, Assert.IsType<LayoutGridResizerControl>(root.Children[1]).Thickness);
        Assert.All(root.Children.Select((child, index) => (child, index)), pair => Assert.Equal(pair.index, Grid.GetColumn(pair.child)));
        Assert.Same(root, workspace.Manager.DockFactory.VisibleDockableControls[workspace.Manager.Layout.RootPanel.DockModel]);
        Assert.Same(workspace.ToolGrid, workspace.Manager.DockFactory.VisibleDockableControls[workspace.Tools.DockModel]);
    }

    [AvaloniaFact]
    public void Hidden_Children_Keep_Zero_Length_Slots_And_Only_The_Visible_Splitter_Gap()
    {
        using Workspace workspace = new();
        LayoutAnchorablePane empty = new();
        workspace.Tools.Children.Insert(1, empty);
        workspace.Update();
        LayoutAnchorablePaneGroupControl grid = workspace.ToolGrid;
        Assert.Equal(5, grid.Children.Count);
        Assert.Equal(5, grid.RowDefinitions.Count);
        Assert.Same(empty, Assert.IsType<LayoutAnchorablePaneControl>(grid.Children[2]).Model);
        Assert.Equal(new GridLength(6), grid.RowDefinitions[1].Height);
        Assert.Equal(new GridLength(0), grid.RowDefinitions[2].Height);
        Assert.Equal(0, grid.RowDefinitions[2].MinHeight);
        Assert.Equal(new GridLength(0), grid.RowDefinitions[3].Height);
        Assert.False(grid.Children[2].IsVisible);
        Assert.False(grid.Children[3].IsVisible);
        empty.Children.Add(new LayoutAnchorable { Title = "Shown" });
        workspace.Update();
        Assert.True(grid.Children[2].IsVisible);
        Assert.Equal(GridLength.Star, grid.RowDefinitions[2].Height);
        Assert.Equal(new GridLength(6), grid.RowDefinitions[3].Height);
    }

    [AvaloniaFact]
    public void Reordering_And_Changing_Axis_Preserve_The_Public_Pane_Controls()
    {
        using Workspace workspace = new();
        LayoutDocumentPaneGroupControl grid = workspace.DocumentGrid;
        Control first = grid.Children[0], second = grid.Children[2];
        workspace.Documents.Children.Move(0, 1);
        workspace.Update();
        Assert.Same(second, grid.Children[0]);
        Assert.Same(first, grid.Children[2]);
        workspace.Documents.Orientation = Orientation.Vertical;
        workspace.Update();
        Assert.Same(second, grid.Children[0]);
        Assert.Same(first, grid.Children[2]);
        Assert.Empty(grid.ColumnDefinitions);
        Assert.Equal(3, grid.RowDefinitions.Count);
        Assert.Equal(2, Grid.GetRow(first));
        Assert.Equal(0, Grid.GetColumn(first));
    }

    [AvaloniaFact]
    public void Caller_Edits_To_Public_Definitions_Participate_In_Actual_Grid_Layout()
    {
        using Workspace workspace = new();
        LayoutPanelControl root = workspace.Manager.LayoutRootPanel!;
        root.ColumnDefinitions[2].Width = new GridLength(280);
        workspace.Update();
        Assert.InRange(workspace.ToolGrid.Bounds.Width, 279, 281);
        Assert.Equal(new GridLength(400), workspace.Tools.DockWidth);
        workspace.Tools.DockWidth = new GridLength(310);
        workspace.Update();
        Assert.InRange(workspace.ToolGrid.Bounds.Width, 309, 311);
    }

    [AvaloniaFact]
    public void Groups_Normalize_Only_Their_Split_Axis_And_Panels_Preserve_Explicit_Lengths()
    {
        using Workspace workspace = new();
        Assert.Equal(GridLength.Star, workspace.Documents.DockWidth);
        Assert.Equal(new GridLength(400), workspace.Tools.DockWidth);
        Assert.Equal(GridLength.Star, workspace.FirstDocument.DockWidth);
        Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.SecondDocument.DockWidth);
        Assert.Equal(new GridLength(125), workspace.FirstDocument.DockHeight);
        Assert.Equal(GridLength.Star, workspace.Top.DockHeight);
        Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.Bottom.DockHeight);
        Assert.Equal(new GridLength(140), workspace.Top.DockWidth);
        Assert.Equal(new GridLength(160), workspace.Bottom.DockWidth);
        AssertRatio(workspace.DocumentGrid, workspace.FirstDocument, workspace.SecondDocument, horizontal: true);
        AssertRatio(workspace.ToolGrid, workspace.Top, workspace.Bottom, horizontal: false);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Live_Pixel_Changes_Persist_Until_Children_Are_Rebuilt(bool tool)
    {
        using Workspace workspace = new();
        if (tool) workspace.Top.DockHeight = new GridLength(200);
        else workspace.FirstDocument.DockWidth = new GridLength(240);
        workspace.Update();
        workspace.Window.Height += 80;
        workspace.Window.Width += 80;
        workspace.Update();
        LayoutElement pane = tool ? workspace.Top : workspace.FirstDocument;
        Control native = workspace.PaneControl(pane);
        Assert.InRange(tool ? native.Bounds.Height : native.Bounds.Width, tool ? 199 : 239, tool ? 201 : 241);
        // A new hidden child also participates in the upstream length policy.
        if (tool)
        {
            LayoutAnchorablePane empty = new() { DockHeight = GridLength.Auto };
            workspace.Tools.Children.Add(empty);
            workspace.Update();
            Assert.False(empty.IsVisible);
            Assert.Equal(GridLength.Star, empty.DockHeight);
            Assert.Equal(GridLength.Star, workspace.Top.DockHeight);
        }
        else
        {
            LayoutDocumentPane added = new(new LayoutDocument { Title = "Added" }) { DockWidth = GridLength.Auto };
            workspace.Documents.Children.Add(added);
            workspace.Update();
            Assert.Equal(GridLength.Star, added.DockWidth);
            Assert.Equal(GridLength.Star, workspace.FirstDocument.DockWidth);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Orientation_Is_Live_And_Rebuilds_The_New_Axis_Without_Activating_Content(bool tool)
    {
        using Workspace workspace = new();
        workspace.FirstDocument.Children[0].IsActive = true;
        LayoutContent active = workspace.Manager.Layout.ActiveContent!;
        if (tool)
        {
            LayoutAnchorablePaneGroupControl grid = workspace.ToolGrid;
            workspace.Tools.Orientation = Orientation.Horizontal;
            workspace.Update();
            Assert.Same(grid, workspace.ToolGrid);
            Assert.Equal(Orientation.Horizontal, grid.Orientation);
            Assert.Equal(GridLength.Star, workspace.Top.DockWidth);
            Assert.Equal(GridLength.Star, workspace.Bottom.DockWidth);
            Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.Bottom.DockHeight);
        }
        else
        {
            LayoutDocumentPaneGroupControl grid = workspace.DocumentGrid;
            workspace.Documents.Orientation = Orientation.Vertical;
            workspace.Update();
            Assert.Same(grid, workspace.DocumentGrid);
            Assert.Equal(Orientation.Vertical, grid.Orientation);
            Assert.Equal(GridLength.Star, workspace.FirstDocument.DockHeight);
            Assert.Equal(GridLength.Star, workspace.SecondDocument.DockHeight);
            Assert.Equal(new GridLength(2, GridUnitType.Star), workspace.SecondDocument.DockWidth);
        }
        Assert.Same(active, workspace.Manager.Layout.ActiveContent);
        workspace.Manager.FlowDirection = FlowDirection.RightToLeft;
        workspace.Update();
        Assert.Equal(FlowDirection.LeftToRight, workspace.Manager.LayoutRootPanel!.FlowDirection);
        Assert.Equal(FlowDirection.LeftToRight, workspace.DocumentGrid.FlowDirection);
        Assert.Equal(FlowDirection.LeftToRight, workspace.ToolGrid.FlowDirection);
    }

    [AvaloniaFact]
    public void Root_Panel_Property_Controls_The_Displayed_Grid_And_Reuses_The_Content_View()
    {
        Assert.Null(new DockingManager().LayoutRootPanel);
        using Workspace workspace = new();
        LayoutPanelControl root = workspace.Manager.LayoutRootPanel!;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.FirstDocument.Children[0])!;
        Control view = item.View;
        workspace.Manager.SetValue(DockingManager.LayoutRootPanelProperty, null);
        workspace.Update();
        Assert.DoesNotContain(root, workspace.Manager.GetVisualDescendants());
        Assert.Empty(root.Children);
        workspace.Manager.LayoutRootPanel = root;
        workspace.Update();
        Assert.Contains(root, workspace.Manager.GetVisualDescendants());
        Assert.Contains(view, root.GetVisualDescendants());
        Assert.Same(view, item.View);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Replacing_The_Root_Or_Layout_Replaces_The_Displayed_Model(int mode)
    {
        using Workspace workspace = new();
        LayoutPanelControl previous = workspace.Manager.LayoutRootPanel!;
        if (mode == 2)
        {
            XmlLayoutSerializer serializer = new(workspace.Manager);
            using StringWriter xml = new();
            serializer.Serialize(xml);
            serializer.Deserialize(new StringReader(xml.ToString()));
        }
        else
        {
            LayoutPanel replacement = new(new LayoutDocumentPane(new LayoutDocument { Title = "Replacement" }));
            if (mode == 0) workspace.Manager.Layout.RootPanel = replacement;
            else workspace.Manager.Layout = new LayoutRoot { RootPanel = replacement };
        }
        workspace.Update();
        LayoutPanelControl current = workspace.Manager.LayoutRootPanel!;
        Assert.NotSame(previous, current);
        Assert.Same(workspace.Manager.Layout.RootPanel, current.Model);
        Assert.Contains(current, workspace.Manager.GetVisualDescendants());
        Assert.DoesNotContain(previous, workspace.Manager.GetVisualDescendants());
        Assert.Empty(previous.Children);
        Assert.Null(previous.GetVisualParent());
        Assert.Null(previous.Parent);
    }

    [AvaloniaFact]
    public void Root_Change_Hook_Observes_Load_Clear_And_Restore()
    {
        ManagerProbe manager = new();
        Window window = new() { Content = manager };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            LayoutPanelControl root = manager.LayoutRootPanel!;
            Assert.Same(root, manager.LastPanel);
            int loaded = manager.Changes;
            manager.LayoutRootPanel = null;
            Assert.Null(manager.LastPanel);
            manager.LayoutRootPanel = root;
            Assert.Same(root, manager.LastPanel);
            Assert.Equal(loaded + 2, manager.Changes);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void View_Reattachment_Releases_The_Old_Presenter_And_Reuses_The_Root_Control()
    {
        using Workspace workspace = new();
        DockingManager manager = workspace.Manager;
        LayoutPanelControl root = manager.LayoutRootPanel!;
        workspace.Window.Content = null;
        workspace.Update();
        Assert.Empty(root.Children);
        Assert.Null(root.GetVisualParent());
        workspace.Window.Content = manager;
        workspace.Update();
        Assert.Same(root, manager.LayoutRootPanel);
        Assert.Contains(root, manager.GetVisualDescendants());
        Assert.Equal(2, root.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Count());
    }

    [AvaloniaFact]
    public void Floating_Tool_Groups_Use_The_Same_Public_Model_And_Existing_Views()
    {
        using Workspace workspace = new();
        LayoutAnchorablePaneGroup model = workspace.Tools;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Top.Children[0])!;
        Control view = item.View;
        workspace.Manager.DockFactory.FloatDockable(model.DockModel);
        workspace.Update();
        LayoutFloatingWindowControl floating = Assert.Single(workspace.Manager.FloatingWindows);
        LayoutAnchorablePaneGroupControl grid = floating.GetVisualDescendants().OfType<LayoutAnchorablePaneGroupControl>()
            .Single(control => ReferenceEquals(control.Model, model));
        Assert.Equal(model.Orientation, grid.Orientation);
        Assert.Contains(view, grid.GetVisualDescendants());
        Assert.Same(view, item.View);
    }

    [AvaloniaFact]
    public void Retained_Old_Models_And_Manager_Do_Not_Retain_Discarded_Grids()
    {
        var result = ReplaceAndRelease();
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
        Assert.False(result.Root.TryGetTarget(out _));
        Assert.False(result.Group.TryGetTarget(out _));
        GC.KeepAlive(result.Manager);
        GC.KeepAlive(result.Model);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (DockingManager Manager, LayoutRoot Model, WeakReference<Control> Root, WeakReference<Control> Group) ReplaceAndRelease()
    {
        // Retain data, not application controls with their own last-rendered
        // composition subtrees, so this checks the adapter's subscriptions.
        LayoutAnchorablePaneGroup group = new(new LayoutAnchorablePane(new LayoutAnchorable { Title = "Tool", Content = "Data" }));
        DockingManager manager = new()
        {
            Layout = new LayoutRoot { RootPanel = new LayoutPanel { Children = { new LayoutDocumentPane(), group } } }
        };
        Window window = new() { Content = manager, Width = 800, Height = 500 };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var result = (manager, manager.Layout,
                new WeakReference<Control>(manager.LayoutRootPanel!),
                new WeakReference<Control>(manager.GetVisualDescendants().OfType<LayoutAnchorablePaneGroupControl>().Single()));
            manager.Layout = new LayoutRoot();
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            window.CaptureRenderedFrame()?.Dispose();
            return result;
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void AssertRatio(Control grid, LayoutElement first, LayoutElement second, bool horizontal)
    {
        Control[] panes = grid.GetVisualDescendants().OfType<Control>()
            .Where(control => control is DocumentDockControl or ToolDockControl).ToArray();
        Control a = panes.Single(control => ReferenceEquals(control.DataContext, first.DockModel));
        Control b = panes.Single(control => ReferenceEquals(control.DataContext, second.DockModel));
        double extentA = horizontal ? a.Bounds.Width : a.Bounds.Height;
        double extentB = horizontal ? b.Bounds.Width : b.Bounds.Height;
        Assert.InRange(extentB, extentA * 2 - 2, extentA * 2 + 2);
    }

    private sealed class ManagerProbe : DockingManager
    {
        public int Changes { get; private set; }
        public LayoutPanelControl? LastPanel { get; private set; }
        protected override void OnLayoutRootPanelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Changes++; LastPanel = LayoutRootPanel; base.OnLayoutRootPanelChanged(e);
        }
    }

    private sealed class Workspace : IDisposable
    {
        public GridControlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutDocumentPaneGroup Documents => (LayoutDocumentPaneGroup)Manager.Layout.RootPanel.Children[0];
        public LayoutAnchorablePaneGroup Tools => (LayoutAnchorablePaneGroup)Manager.Layout.RootPanel.Children[1];
        public LayoutDocumentPane FirstDocument => (LayoutDocumentPane)Documents.Children[0];
        public LayoutDocumentPane SecondDocument => (LayoutDocumentPane)Documents.Children[1];
        public LayoutAnchorablePane Top => (LayoutAnchorablePane)Tools.Children[0];
        public LayoutAnchorablePane Bottom => (LayoutAnchorablePane)Tools.Children[1];
        public LayoutDocumentPaneGroupControl DocumentGrid => Manager.GetVisualDescendants().OfType<LayoutDocumentPaneGroupControl>().Single();
        public LayoutAnchorablePaneGroupControl ToolGrid => Manager.GetVisualDescendants().OfType<LayoutAnchorablePaneGroupControl>().Single();
        public Workspace() { Manager = Window.FindControl<DockingManager>("Manager")!; Window.Show(); Update(); }
        public Control PaneControl(LayoutElement model) => Manager.GetVisualDescendants().OfType<Control>()
            .Single(control => control is DocumentDockControl or ToolDockControl && ReferenceEquals(control.DataContext, model.DockModel));
        public void Update()
        {
            Dispatcher.UIThread.RunJobs(); Window.UpdateLayout();
            foreach (LayoutFloatingWindowControl window in Manager.FloatingWindows) window.UpdateLayout();
        }
        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs();
        }
    }
}
