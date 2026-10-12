using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class PaneControlTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Panes_Expose_The_Live_Model_And_Compiled_Manager_Theme(bool tool)
    {
        using Workspace workspace = new();
        LayoutCachePaneControl control = workspace.Pane(tool);
        ILayoutElement model = ((ILayoutControl)control).Model;
        Assert.False(control.Focusable);
        Assert.Same(model, control.Tag);
        Assert.Same(tool ? workspace.Manager.AnchorablePaneControlStyle : workspace.Manager.DocumentPaneControlStyle, control.Theme);
        Assert.Same(tool ? workspace.Tools.Children : (object)workspace.Documents.Children, control.ItemsSource);
        Assert.Same(((ILayoutContentSelector)model).SelectedContent, control.SelectedItem);
        if (tool) Assert.Single(control.GetVisualDescendants().OfType<ToolControl>());
        else Assert.Single(control.GetVisualDescendants().OfType<DocumentControl>());
        ControlTheme replacement = (ControlTheme)workspace.Window.Resources[tool ? "LegacyToolPane" : "LegacyDocumentPane"]!;
        if (tool) workspace.Manager.AnchorablePaneControlStyle = replacement;
        else workspace.Manager.DocumentPaneControlStyle = replacement;
        workspace.Update();
        Assert.Same(control, workspace.Pane(tool));
        Assert.Same(replacement, control.Theme);
        Assert.Equal(tool ? 110 : 120, control.MinHeight);
        Assert.Null(control.Tag);
        if (tool) workspace.Manager.AnchorablePaneControlStyle = null;
        else workspace.Manager.DocumentPaneControlStyle = null;
        workspace.Update();
        Assert.Null(control.Theme);
        Assert.Equal(0, control.MinHeight);
        Assert.NotEmpty(control.GetVisualChildren());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Model_Selection_Is_Passive_And_Control_Selection_Uses_The_Live_Pane(bool tool)
    {
        using Workspace workspace = new();
        LayoutCachePaneControl control = workspace.Pane(tool);
        ILayoutContentSelector pane = (ILayoutContentSelector)((ILayoutControl)control).Model;
        workspace.Tools.Children[0].IsActive = true;
        LayoutContent active = workspace.Manager.Layout.ActiveContent!;
        pane.SelectedContentIndex = 1;
        workspace.Update();
        Assert.Same(pane.SelectedContent, control.SelectedItem);
        Assert.Equal(1, control.SelectedIndex);
        Assert.Same(active, workspace.Manager.Layout.ActiveContent);
        control.SelectedIndex = 0;
        workspace.Update();
        Assert.Equal(0, pane.SelectedContentIndex);
        Assert.Same(tool ? active : pane.SelectedContent, workspace.Manager.Layout.ActiveContent);
        control.SelectedIndex = -1;
        Assert.Equal(-1, pane.SelectedContentIndex);
        pane.SelectedContentIndex = 1;
        pane.SelectedContentIndex = -1;
        Assert.Equal(-1, control.SelectedIndex);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pane_Themes_And_Existing_Content_Views_Follow_Floating_And_Docking(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tools.Children[0] : workspace.Documents.Children[0];
        content.IsActive = true;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        Control view = item.View;
        if (tool) workspace.Manager.DockFactory.FloatAllDockables(content.DockModel);
        else content.Float();
        workspace.Update();
        LayoutFloatingWindowControl floating = workspace.Manager.FloatingWindows.Single();
        LayoutCachePaneControl pane = floating.GetVisualDescendants().OfType<LayoutCachePaneControl>().Single();
        Assert.Same(content.DockModel.Owner, Assert.IsAssignableFrom<LayoutElement>(((ILayoutControl)pane).Model).DockModel);
        Assert.Same(tool ? workspace.Manager.AnchorablePaneControlStyle : workspace.Manager.DocumentPaneControlStyle, pane.Theme);
        Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(content));
        Assert.Same(view, item.View);
        Assert.Contains(view, floating.GetVisualDescendants());
        content.Dock();
        workspace.Update();
        Assert.Same(view, item.View);
    }

    [AvaloniaTheory]
    [InlineData("CachePane")]
    [InlineData("LegacyCachePane")]
    public void Cache_Panes_Compile_In_Both_Namespaces(string key)
    {
        using Workspace workspace = new();
        Control control = ((IDataTemplate)workspace.Window.Resources[key]!).Build(workspace.Documents)!;
        control.DataContext = workspace.Documents;
        Assert.Same(workspace.Documents.Children, Assert.IsType<LayoutCachePaneControl>(control).ItemsSource);
    }

    [AvaloniaFact]
    public void Empty_Selection_Is_Handled_Before_It_Reaches_The_Parent()
    {
        CacheProbe control = new() { ItemsSource = new[] { "First", "Second" } };
        StackPanel parent = new(); parent.Children.Add(control);
        int notifications = 0;
        parent.AddHandler(LayoutCachePaneControl.SelectionChangedEvent, (_, _) => notifications++);
        control.SelectedIndex = 1;
        Assert.Equal(1, notifications);
        control.SelectedIndex = -1;
        Assert.Equal(2, control.Calls);
        Assert.Equal(1, notifications);
    }

    [AvaloniaFact]
    public void Tool_Pane_Constructor_Validates_And_Uses_Its_Model()
    {
        Assert.Equal("model", Assert.Throws<ArgumentNullException>(() => new LayoutAnchorablePaneControl(null!)).ParamName);
        LayoutAnchorablePane model = new(new LayoutAnchorable());
        LayoutAnchorablePaneControl control = new(model);
        Assert.Same(model, control.Model);
        Assert.Same(model.Children, control.ItemsSource);
        Assert.Same(model.SelectedContent, control.SelectedItem);
        model.Children.Add(new LayoutAnchorable());
        control.SelectedIndex = 1;
        Assert.Same(model.Children[1], model.SelectedContent);
    }

    [AvaloniaFact]
    public void A_Retained_Pane_And_Manager_Do_Not_Retain_An_Unused_Control()
    {
        using Workspace workspace = new();
        WeakReference<Control> control = CreateUnused(workspace.Tools);
        for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
        Assert.False(control.TryGetTarget(out _));
        workspace.Manager.FlowDirection = FlowDirection.RightToLeft;
        GC.KeepAlive(workspace.Tools);
    }

    [AvaloniaFact]
    public void Pane_Style_Hooks_Are_Invoked_With_The_Correct_Properties()
    {
        ManagerProbe manager = new();
        manager.DocumentPaneControlStyle = new() { TargetType = typeof(LayoutDocumentPaneControl) };
        manager.AnchorablePaneControlStyle = new() { TargetType = typeof(LayoutAnchorablePaneControl) };
        Assert.Same(DockingManager.DocumentPaneControlStyleProperty, manager.DocumentChange);
        Assert.Same(DockingManager.AnchorablePaneControlStyleProperty, manager.ToolChange);
    }

    [AvaloniaFact]
    public void A_Custom_Pane_Follows_Manager_Themes_And_Flow_Direction_After_Transfer()
    {
        using Workspace workspace = new();
        LayoutAnchorablePane model = workspace.Tools;
        LayoutAnchorablePaneControl control = new(model);
        workspace.Manager.FlowDirection = FlowDirection.RightToLeft;
        Assert.Equal(FlowDirection.RightToLeft, control.FlowDirection);
        ControlTheme next = (ControlTheme)workspace.Window.Resources["LegacyToolPane"]!;
        DockingManager manager = new() { AnchorablePaneControlStyle = next };
        manager.Layout.RootPanel.Children.Add(model);
        Assert.Same(next, control.Theme);
        Assert.Equal(FlowDirection.LeftToRight, control.FlowDirection);
        workspace.Manager.AnchorablePaneControlStyle = null;
        Assert.Same(next, control.Theme);
        manager.AnchorablePaneControlStyle = null;
        Assert.Null(control.Theme);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Removing_And_Moving_Children_Preserves_Model_Selection_Without_Reentrant_Collection_Updates(bool tool)
    {
        using Workspace workspace = new();
        LayoutCachePaneControl control = workspace.Pane(tool);
        ILayoutContentSelector pane = (ILayoutContentSelector)((ILayoutControl)control).Model;
        ILayoutGroup group = (ILayoutGroup)pane;
        pane.SelectedContentIndex = 1;
        LayoutContent selected = pane.SelectedContent!;
        LayoutContent? active = workspace.Manager.Layout.ActiveContent;
        ((ILayoutPane)pane).MoveChild(1, 0);
        Assert.Same(selected, control.SelectedItem);
        Assert.Same(active, workspace.Manager.Layout.ActiveContent);
        Assert.Equal(0, control.SelectedIndex);
        group.RemoveChild(selected);
        workspace.Update();
        Assert.Same(pane.SelectedContent, control.SelectedItem);
        Assert.Equal(pane.SelectedContentIndex, control.SelectedIndex);
        group.RemoveChildAt(0);
        workspace.Update();
        Assert.Null(control.SelectedItem);
        Assert.Equal(-1, control.SelectedIndex);
    }

    [AvaloniaFact]
    public void Custom_Tool_Pane_Input_Invokes_Protected_Hooks_And_Activates_The_Selected_Tool()
    {
        using Workspace workspace = new();
        ToolProbe control = new(workspace.Tools);
        TextBox editor = new() { Width = 120, Height = 30 };
        control.Template = new FuncControlTemplate<LayoutAnchorablePaneControl>((_, _) =>
            new Border { Background = Brushes.Transparent, Child = editor });
        workspace.Manager.Template = new FuncControlTemplate<DockingManager>((_, _) => control);
        Window window = workspace.Window;
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            foreach (MouseButton button in new[] { MouseButton.Left, MouseButton.Right })
            {
                workspace.Documents.Children[0].IsActive = true;
                window.MouseDown(new Point(5, 5), button);
                window.MouseUp(new Point(5, 5), button);
                Assert.Same(workspace.Tools.SelectedContent, workspace.Manager.Layout.ActiveContent);
            }
            Assert.Equal(1, control.LeftCalls);
            Assert.Equal(1, control.RightCalls);
            workspace.Documents.Children[0].IsActive = true;
            Assert.True(editor.Focus());
            Assert.True(control.FocusCalls > 0);
            Assert.Same(workspace.Tools.SelectedContent, workspace.Manager.Layout.ActiveContent);
        }
        finally { window.Close(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Control> CreateUnused(LayoutAnchorablePane model) => new(new LayoutAnchorablePaneControl(model));

    private sealed class CacheProbe : LayoutCachePaneControl
    {
        public int Calls { get; private set; }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e) { Calls++; base.OnSelectionChanged(e); }
    }

    private sealed class ManagerProbe : DockingManager
    {
        public AvaloniaProperty? DocumentChange { get; private set; }
        public AvaloniaProperty? ToolChange { get; private set; }
        protected override void OnDocumentPaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { DocumentChange = e.Property; base.OnDocumentPaneControlStyleChanged(e); }
        protected override void OnAnchorablePaneControlStyleChanged(AvaloniaPropertyChangedEventArgs e) { ToolChange = e.Property; base.OnAnchorablePaneControlStyleChanged(e); }
    }

    private sealed class ToolProbe(LayoutAnchorablePane model) : LayoutAnchorablePaneControl(model)
    {
        public int LeftCalls { get; private set; }
        public int RightCalls { get; private set; }
        public int FocusCalls { get; private set; }
        protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { LeftCalls++; base.OnMouseLeftButtonDown(e); }
        protected override void OnMouseRightButtonDown(PointerPressedEventArgs e) { RightCalls++; base.OnMouseRightButtonDown(e); }
        protected override void OnGotKeyboardFocus(FocusChangedEventArgs e) { FocusCalls++; base.OnGotKeyboardFocus(e); }
    }

    private sealed class Workspace : IDisposable
    {
        public PaneControlFixture Window { get; } = new();
        public DockingManager Manager => Window.FindControl<DockingManager>("Manager")!;
        public LayoutDocumentPane Documents => (LayoutDocumentPane)Manager.Layout.RootPanel.Children[0];
        public LayoutAnchorablePane Tools => (LayoutAnchorablePane)Manager.Layout.RootPanel.Children[1];
        public Workspace() { Window.Show(); Update(); }
        public LayoutCachePaneControl Pane(bool tool) => tool
            ? Manager.GetVisualDescendants().OfType<LayoutAnchorablePaneControl>().Single()
            : Manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
        public void Update()
        {
            Dispatcher.UIThread.RunJobs(); Window.UpdateLayout();
            foreach (LayoutFloatingWindowControl window in Manager.FloatingWindows) window.UpdateLayout();
        }
        public void Dispose()
        {
            foreach (LayoutFloatingWindowControl floating in Manager.FloatingWindows.ToArray()) floating.Close();
            Window.Close(); Dispatcher.UIThread.RunJobs();
        }
    }
}
