using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using IDock = Dock.Model.Core.IDock;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Model_And_ReadOnly_Item_Properties_Use_The_Existing_Container_And_Protected_Hook(bool tool)
    {
        using Workspace workspace = new(tool);
        TemplatedControl tab = workspace.Tab;
        Assert.False(tab.Focusable);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), Item(tab));
        Assert.True(tool ? LayoutAnchorableTabItem.LayoutItemProperty.IsReadOnly : LayoutDocumentTabItem.LayoutItemProperty.IsReadOnly);
        SetModel(tab, workspace.Second);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Second), Item(tab));
        LayoutDocumentItem custom = new();
        if (tab is DocumentProbe document) { document.AssignItem(custom); Assert.Equal(2, document.Models.Count); }
        else { ToolProbe anchorable = (ToolProbe)tab; anchorable.AssignItem(custom); Assert.Equal(2, anchorable.Models.Count); }
        Assert.Same(custom, Item(tab));
        SetModel(tab, null);
        Assert.Null(Item(tab));
        TemplatedControl suppressed = tool ? new ToolProbe { SuppressModel = true } : new DocumentProbe { SuppressModel = true };
        SetModel(suppressed, workspace.First);
        Assert.Null(Item(suppressed));
    }

    [AvaloniaTheory]
    [InlineData("DocumentTab", false)]
    [InlineData("LegacyDocumentTab", false)]
    [InlineData("ToolTab", true)]
    [InlineData("LegacyToolTab", true)]
    public void Both_Namespaces_Compile_Headers_With_Live_Manager_Templates(string key, bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.DocumentHeaderTemplate = (IDataTemplate)workspace.Window.Resources["DocumentHeader"]!;
        workspace.Manager.AnchorableHeaderTemplate = (IDataTemplate)workspace.Window.Resources["ToolHeader"]!;
        Control tab = ((IDataTemplate)workspace.Window.Resources[key]!).Build(workspace.First)!;
        tab.DataContext = workspace.First;
        workspace.Tabs.Children.Clear();
        workspace.Tabs.Children.Add(tab);
        workspace.Update();
        Assert.Same(workspace.First, Model(tab));
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), Item(tab));
        TextBlock header = Assert.Single(tab.GetVisualDescendants().OfType<TextBlock>());
        Assert.Equal((tool ? "Tool: " : "Document: ") + workspace.First.Title, header.Text);
        workspace.First.Title = "Changed";
        Assert.EndsWith("Changed", header.Text);
        if (tool) workspace.Manager.AnchorableHeaderTemplate = null;
        else workspace.Manager.DocumentHeaderTemplate = null;
        workspace.Update();
        Assert.Equal("Changed", Assert.Single(tab.GetVisualDescendants().OfType<TextBlock>()).Text);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Native_Docked_And_Floating_Tabs_Expose_Public_Headers_And_Current_Items(bool tool, bool floating)
    {
        using Workspace workspace = new(tool);
        if (floating)
        {
            if (tool) workspace.Manager.DockFactory.FloatAllDockables(workspace.First.DockModel);
            else workspace.First.Float();
            workspace.Update();
        }
        Control scope = floating ? workspace.Manager.FloatingWindows.Single() : workspace.Manager;
        Control header = NativeTab(scope, workspace.First, tool);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), Item(header));
        Assert.Contains(header.GetVisualAncestors(), parent => tool ? parent is ToolTabStripItem : parent is DocumentTabStripItem);
        workspace.First.Title = "Native header";
        Assert.Equal("Native header", Assert.Single(header.GetVisualDescendants().OfType<TextBlock>()).Text);
        Click(header);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Custom_Document_Presses_And_Tool_Releases_Activate_And_Invoke_Protected_Input_Hooks(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Second.IsActive = true;
        Point point = Center(workspace.Tab);
        workspace.Window.MouseMove(point);
        workspace.Window.MouseDown(point, MouseButton.Left);
        Assert.Same(tool ? workspace.Second : workspace.First, workspace.Manager.Layout.ActiveContent);
        workspace.Window.MouseUp(point, MouseButton.Left);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
        workspace.Window.MouseMove(new Point(799, 449));
        if (workspace.Tab is DocumentProbe document)
        {
            Assert.Equal(1, document.Presses); Assert.Equal(1, document.Releases); Assert.Equal(1, document.Downs);
            Assert.True(document.Enters > 0 && document.Exits > 0 && document.Moves > 0);
        }
        else
        {
            ToolProbe anchorable = (ToolProbe)workspace.Tab;
            Assert.Equal(1, anchorable.Presses); Assert.Equal(1, anchorable.Releases);
            Assert.True(anchorable.Enters > 0 && anchorable.Exits > 0 && anchorable.Moves > 0);
        }
    }

    [AvaloniaFact]
    public void Custom_Document_Middle_Click_Uses_The_Current_Command_And_Respects_Enablement()
    {
        using Workspace workspace = new(false);
        int closed = 0;
        using BehaviorSubject<bool> enabled = new(true);
        using ReactiveCommand<Unit, Unit> command = ReactiveCommand.Create(() => { closed++; }, enabled, ImmediateScheduler.Instance);
        Item(workspace.Tab)!.CloseCommand = command;
        Click(workspace.Tab, MouseButton.Middle);
        Assert.Equal(1, closed);
        enabled.OnNext(false);
        Dispatcher.UIThread.RunJobs();
        Click(workspace.Tab, MouseButton.Middle);
        Assert.Equal(1, closed);
        Assert.Same(workspace.Pane, workspace.First.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Interactive_Children_Do_Not_Activate_Or_Drag_Custom_Tabs(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Tab.Theme = (ControlTheme)workspace.Window.Resources[tool ? "InteractiveTool" : "InteractiveDocument"]!;
        workspace.Second.IsActive = true;
        workspace.Update();
        Click(Assert.Single(workspace.Tab.GetVisualDescendants().OfType<Button>()));
        Assert.Same(workspace.Second, workspace.Manager.Layout.ActiveContent);
        Assert.Empty(workspace.Manager.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dragging_Custom_Tabs_Reorders_The_Live_Pane_Without_Floating(bool tool)
    {
        using Workspace workspace = new(tool);
        Point start = Center(workspace.Tab);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(Center(workspace.Other), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseUp(Center(workspace.Other), MouseButton.Left);
        Assert.Equal(new[] { workspace.Second, workspace.First }, workspace.Pane.Children.Cast<LayoutContent>());
        Assert.Equal(new[] { workspace.Second.DockModel, workspace.First.DockModel }, ((IDock)workspace.First.DockModel.Owner!).VisibleDockables!);
        Assert.Empty(workspace.Manager.FloatingWindows);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dragging_Outside_A_Custom_Tab_Row_Floats_Only_That_Content_Through_Native_Window_Drag(bool tool)
    {
        using Workspace workspace = new(tool);
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, e) => { began++; e.Cancel = true; };
        Point start = Center(workspace.Tab);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
        workspace.Window.MouseMove(start + new Vector(0, 90), RawInputModifiers.LeftMouseButton);
        workspace.Update();
        Assert.True(workspace.First.IsFloating);
        Assert.False(workspace.Second.IsFloating);
        Assert.Equal(1, began);
        Assert.False(workspace.Manager.FloatingWindows.Single().IsDragging);
        workspace.Window.MouseUp(start + new Vector(0, 90), MouseButton.Left);
    }

    private static Control NativeTab(Control scope, LayoutContent model, bool tool) => scope.GetVisualDescendants().OfType<Control>()
        .Single(control => (tool ? control is LayoutAnchorableTabItem : control is LayoutDocumentTabItem) && ReferenceEquals(Model(control), model));
    private static LayoutContent? Model(Control control) => control is LayoutDocumentTabItem document ? document.Model : ((LayoutAnchorableTabItem)control).Model;
    private static LayoutItem? Item(Control control) => control is LayoutDocumentTabItem document ? document.LayoutItem : ((LayoutAnchorableTabItem)control).LayoutItem;
    private static void SetModel(Control control, LayoutContent? model)
    {
        if (control is LayoutDocumentTabItem document) document.Model = model;
        else ((LayoutAnchorableTabItem)control).Model = model;
    }
    private static Point Center(Control control) => control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), TopLevel.GetTopLevel(control)!)!.Value;
    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        Point point = Center(control);
        top.MouseMove(point); top.MouseDown(point, button); top.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class DocumentProbe : LayoutDocumentTabItem
    {
        public bool SuppressModel { get; set; }
        public List<LayoutContent?> Models { get; } = new();
        public int Presses { get; private set; }
        public int Releases { get; private set; }
        public int Moves { get; private set; }
        public int Exits { get; private set; }
        public int Enters { get; private set; }
        public int Downs { get; private set; }
        public void AssignItem(LayoutItem? item) => SetLayoutItem(item);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e) { Models.Add(Model); if (!SuppressModel) base.OnModelChanged(e); }
        protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
        protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
        protected override void OnMouseMove(PointerEventArgs e) { Moves++; base.OnMouseMove(e); }
        protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
        protected override void OnMouseEnter(PointerEventArgs e) { Enters++; base.OnMouseEnter(e); }
        protected override void OnMouseDown(PointerPressedEventArgs e) { Downs++; base.OnMouseDown(e); }
    }
    private sealed class ToolProbe : LayoutAnchorableTabItem
    {
        public bool SuppressModel { get; set; }
        public List<LayoutContent?> Models { get; } = new();
        public int Presses { get; private set; }
        public int Releases { get; private set; }
        public int Moves { get; private set; }
        public int Exits { get; private set; }
        public int Enters { get; private set; }
        public void AssignItem(LayoutItem? item) => SetLayoutItem(item);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e) { Models.Add(Model); if (!SuppressModel) base.OnModelChanged(e); }
        protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
        protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
        protected override void OnMouseMove(PointerEventArgs e) { Moves++; base.OnMouseMove(e); }
        protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
        protected override void OnMouseEnter(PointerEventArgs e) { Enters++; base.OnMouseEnter(e); }
    }
    private sealed class Workspace : IDisposable
    {
        public TabControlFixture Window { get; } = new();
        public StackPanel Tabs { get; } = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, Height = 40 };
        public DockingManager Manager { get; }
        public ILayoutContainer Pane { get; }
        public LayoutContent First { get; }
        public LayoutContent Second { get; }
        public TemplatedControl Tab { get; }
        public TemplatedControl Other { get; }
        public Workspace(bool tool)
        {
            First = tool ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
            Second = tool ? new LayoutAnchorable { CanClose = true } : new LayoutDocument();
            First.Title = "First"; Second.Title = "Second";
            First.ContentId = "first"; Second.ContentId = "second";
            First.Content = "First content"; Second.Content = "Second content";
            LayoutPanel root = new();
            if (tool)
            {
                LayoutAnchorablePane tools = new((LayoutAnchorable)First);
                tools.Children.Add((LayoutAnchorable)Second);
                Pane = tools;
                root.Children.Add(tools);
                root.Children.Add(new LayoutDocumentPane(new LayoutDocument { Title = "Document" }));
            }
            else
            {
                LayoutDocumentPane documents = new(First);
                documents.Children.Add(Second);
                Pane = documents;
                root.Children.Add(documents);
            }
            Manager = new() { Layout = new() { RootPanel = root } };
            Tab = tool ? new ToolProbe() : new DocumentProbe();
            Other = tool ? new ToolProbe() : new DocumentProbe();
            SetModel(Tab, First); SetModel(Other, Second);
            Tab.Width = Other.Width = 180;
            Tab.Height = Other.Height = 40;
            Tabs.Children.Add(Tab); Tabs.Children.Add(Other);
            DockPanel panel = new();
            DockPanel.SetDock(Tabs, global::Avalonia.Controls.Dock.Top);
            panel.Children.Add(Tabs); panel.Children.Add(Manager);
            Window.Content = panel;
            Window.Show();
            Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); foreach (LayoutFloatingWindowControl host in Manager.FloatingWindows) host.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
