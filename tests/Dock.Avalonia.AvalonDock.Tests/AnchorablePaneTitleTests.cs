using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
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
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AnchorablePaneTitleTests
{
    [AvaloniaFact]
    public void Model_Changes_Resolve_The_Existing_ReadOnly_Container_And_Notify_Derived_Titles()
    {
        using Workspace workspace = new();
        TitleProbe title = new();
        Assert.Null(title.Model);
        Assert.Null(title.LayoutItem);
        Assert.False(title.Focusable);
        Assert.True(title.IsHitTestVisible);
        Assert.True(AnchorablePaneTitle.LayoutItemProperty.IsReadOnly);
        title.Model = workspace.First;
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), title.LayoutItem);
        title.Model = workspace.Second;
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Second), title.LayoutItem);
        Assert.Equal(new object?[] { workspace.First, workspace.Second }, title.Models);
        LayoutAnchorableItem custom = new();
        title.AssignContainer(custom);
        Assert.Same(custom, title.GetValue(AnchorablePaneTitle.LayoutItemProperty));
        title.Model = null;
        Assert.Null(title.LayoutItem);
        TitleProbe suppressing = new() { SuppressModelHook = true, Model = workspace.First };
        Assert.Same(workspace.First, suppressing.Model);
        Assert.Null(suppressing.LayoutItem);
    }

    [AvaloniaFact]
    public void A_Preassigned_Model_Resolves_On_Attachment_And_An_Explicit_Model_Survives_Closure()
    {
        LayoutAnchorable model = new() { CanClose = true };
        AnchorablePaneTitle title = new() { Model = model };
        Assert.Null(title.LayoutItem);
        LayoutAnchorablePane pane = new(model);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        Window window = new() { Content = title };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(manager.GetLayoutItemFromModel(model), title.LayoutItem);
            model.Close();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(model, title.Model);
            Assert.Null(title.LayoutItem);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("Title")]
    [InlineData("LegacyTitle")]
    public void Both_Xml_Namespaces_Compile_A_Title_With_The_Manager_Template(string key)
    {
        using Workspace workspace = new();
        ContentControlFixture resources = new();
        AnchorablePaneTitle title = Assert.IsType<AnchorablePaneTitle>(((IDataTemplate)resources.Resources[key]!).Build(workspace.First));
        title.DataContext = workspace.First;
        resources.Content = title;
        try
        {
            resources.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(workspace.First, title.Model);
            Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), title.LayoutItem);
            Assert.Equal("First", Assert.Single(title.GetVisualDescendants().OfType<TextBlock>()).Text);
            workspace.First.Title = "Updated";
            Assert.Equal("Updated", Assert.Single(title.GetVisualDescendants().OfType<TextBlock>()).Text);
        }
        finally { resources.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Title_Tracks_Selection_Metadata_And_The_Current_Container(bool floating)
    {
        using Workspace workspace = new();
        if (floating) workspace.FloatGroup();
        Control scope = workspace.Scope;
        AnchorablePaneTitle first = Title(scope);
        Assert.Same(workspace.First, first.Model);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.First), first.LayoutItem);
        Assert.Contains(first.GetVisualAncestors(), visual => visual is ToolChromeControl);
        workspace.Second.IsActive = true;
        workspace.Update();
        AnchorablePaneTitle selected = Title(scope);
        Assert.Same(workspace.Second, selected.Model);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Second), selected.LayoutItem);
        workspace.Second.Title = "Native title change";
        Assert.Equal("Native title change", Assert.Single(selected.GetVisualDescendants().OfType<TextBlock>()).Text);
    }

    [AvaloniaFact]
    public void AutoHide_Preview_Uses_The_Public_Title_And_Existing_Container()
    {
        using Workspace workspace = new();
        workspace.Second.Close();
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.First)!;
        workspace.First.ToggleAutoHide();
        workspace.Update();
        Click(Assert.Single(workspace.Manager.GetVisualDescendants().OfType<ToolPinItemControl>()));
        AnchorablePaneTitle title = Title(workspace.Manager);
        Assert.Same(workspace.First, title.Model);
        Assert.Same(item, title.LayoutItem);
    }

    [AvaloniaFact]
    public void Xml_Replacement_Releases_The_Old_Title_And_Presents_The_Restored_Tool()
    {
        using Workspace workspace = new();
        AnchorablePaneTitle previous = Title(workspace.Manager);
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter text = new();
        serializer.Serialize(text);
        serializer.Deserialize(new StringReader(text.ToString()));
        workspace.Update();
        Assert.Null(previous.Model);
        Assert.Null(previous.LayoutItem);
        AnchorablePaneTitle current = Title(workspace.Manager);
        Assert.NotSame(workspace.First, current.Model);
        Assert.Equal(workspace.First.ContentId, current.Model!.ContentId);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(current.Model), current.LayoutItem);
    }

    [AvaloniaFact]
    public void Clicking_A_Custom_Title_Activates_Its_Model_Through_The_Protected_Input_Hooks()
    {
        using Workspace workspace = new(custom: true);
        workspace.Document.IsActive = true;
        Click(workspace.Custom);
        Assert.Equal(1, workspace.Custom.Presses);
        Assert.Equal(1, workspace.Custom.Releases);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
        Assert.True(workspace.First.IsSelected);
    }

    [AvaloniaFact]
    public void A_Custom_Title_Theme_Can_Compile_A_Binding_To_The_ReadOnly_Container()
    {
        using Workspace workspace = new(custom: true);
        ContentControlFixture resources = new();
        workspace.Custom.Theme = (ControlTheme)resources.Resources["TitleTheme"]!;
        workspace.Update();
        Assert.Equal("First", Assert.Single(workspace.Custom.GetVisualDescendants().OfType<TextBlock>()).Text);
        workspace.Custom.Model = workspace.Second;
        workspace.Second.Title = "Changed second";
        Assert.Equal("Changed second", Assert.Single(workspace.Custom.GetVisualDescendants().OfType<TextBlock>()).Text);
    }

    [AvaloniaFact]
    public void An_Interactive_Title_Child_Keeps_Its_Input_Without_Floating_The_Pane()
    {
        using Workspace workspace = new(custom: true);
        ContentControlFixture resources = new();
        workspace.Manager.AnchorableTitleTemplate = (IDataTemplate)resources.Resources["InteractiveTitle"]!;
        workspace.Update();
        Button button = Assert.Single(workspace.Custom.GetVisualDescendants().OfType<Button>());
        int clicked = 0;
        button.Click += (_, _) => clicked++;
        Click(button);
        Assert.Equal(1, clicked);
        Point start = Center(button, workspace.Window);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(350, 0), RawInputModifiers.LeftMouseButton);
        Assert.Empty(workspace.Manager.FloatingWindows);
        workspace.Window.MouseUp(start + new Vector(350, 0), MouseButton.Left);
    }

    [AvaloniaFact]
    public void A_Custom_Title_Inside_Another_Manager_Activates_And_Floats_Its_Own_Model()
    {
        using Workspace workspace = new(custom: true);
        DockingManager other = new()
        {
            Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Content = workspace.Custom })) }
        };
        workspace.Panel.Children.Remove(workspace.Custom);
        DockPanel.SetDock(other, global::Avalonia.Controls.Dock.Top);
        other.Height = 100;
        workspace.Panel.Children.Insert(0, other);
        workspace.Document.IsActive = true;
        workspace.Update();
        Click(workspace.Custom);
        Assert.Same(workspace.First, workspace.Manager.Layout.ActiveContent);
        PressAndLeave(workspace);
        LayoutAnchorableFloatingWindow floating = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Same(workspace.First, Assert.Single(floating.Descendents().OfType<LayoutAnchorable>()));
        Assert.False(workspace.Second.IsFloating);
        Assert.Empty(other.FloatingWindows);
        Assert.Single(workspace.Manager.FloatingWindows).MouseUp(new Point(5, 5), MouseButton.Left);
    }

    [AvaloniaFact]
    public void A_Custom_AutoHidden_Title_Floats_Only_The_Represented_Tool()
    {
        using Workspace workspace = new(custom: true);
        workspace.First.ToggleAutoHide();
        workspace.Second.ToggleAutoHide();
        workspace.Update();
        Assert.True(workspace.Second.IsAutoHidden);
        PressAndLeave(workspace);
        LayoutAnchorableFloatingWindow floating = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Same(workspace.First, Assert.Single(floating.Descendents().OfType<LayoutAnchorable>()));
        Assert.True(workspace.Second.IsAutoHidden);
        Assert.Single(workspace.Manager.FloatingWindows).MouseUp(new Point(5, 5), MouseButton.Left);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Leaving_A_Custom_Title_Floats_Its_Visual_Pane_Or_Only_Its_Model_And_Uses_Native_Drag_Cancellation(bool cancel, bool paneTitle)
    {
        using Workspace workspace = new(custom: true, paneTitle: paneTitle);
        int began = 0;
        int ended = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = cancel; };
        workspace.Manager.DockFactory.WindowMoveDragEnd += (_, _) => ended++;
        PressAndLeave(workspace);
        LayoutAnchorableFloatingWindow floating = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        Assert.Equal(paneTitle ? new[] { workspace.First, workspace.Second } : new[] { workspace.First },
            floating.Descendents().OfType<LayoutAnchorable>());
        Assert.Equal(paneTitle, workspace.Second.IsFloating);
        LayoutAnchorableFloatingWindowControl host = Assert.IsType<LayoutAnchorableFloatingWindowControl>(Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Equal(1, began);
        Assert.Equal(!cancel, host.IsDragging);
        host.MouseUp(new Point(5, 5), MouseButton.Left);
        workspace.Update();
        Assert.False(host.IsDragging);
        Assert.Equal(cancel ? 0 : 1, ended);
        Assert.True(workspace.Custom.Exits > 0);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Custom_Title_Starts_The_Existing_SinglePane_Window_Drag_Without_Creating_A_Window(bool cancel)
    {
        using Workspace workspace = new(custom: true);
        workspace.FloatGroup();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = cancel; };
        Point point = Center(workspace.Custom, workspace.Window);
        workspace.Window.MouseMove(point);
        workspace.Window.MouseDown(point, MouseButton.Left);
        Assert.Equal(1, began);
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Equal(!cancel, host.IsDragging);
        host.MouseUp(new Point(5, 5), MouseButton.Left);
        Assert.False(host.IsDragging);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Floating_Grip_Does_Not_Start_Duplicate_Drags_Through_The_Title(bool cancel)
    {
        using Workspace workspace = new();
        workspace.FloatGroup();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, args) => { began++; args.Cancel = cancel; };
        Point point = Center(Title(host), host);
        host.MouseMove(point);
        host.MouseDown(point, MouseButton.Left);
        host.MouseMove(point + new Vector(30, 0), RawInputModifiers.LeftMouseButton);
        Assert.Equal(1, began);
        Assert.Equal(!cancel, host.IsDragging);
        host.MouseUp(point + new Vector(30, 0), MouseButton.Left);
        Assert.False(host.IsDragging);
    }

    [AvaloniaTheory]
    [InlineData("float")]
    [InlineData("drag")]
    [InlineData("enabled")]
    public void Custom_Title_Drag_Respects_Capabilities_And_Enablement(string restriction)
    {
        using Workspace workspace = new(custom: true);
        if (restriction == "float") workspace.First.CanFloat = false;
        else if (restriction == "drag") workspace.First.DockModel.CanDrag = false;
        else workspace.Custom.IsEnabled = false;
        PressAndLeave(workspace);
        Assert.Empty(workspace.Manager.FloatingWindows);
        Assert.Same(workspace.Pane, workspace.First.Parent);
    }

    [AvaloniaFact]
    public void A_Refused_Pane_Float_Does_Not_Start_Dragging_Its_Existing_MultiPane_Window()
    {
        using Workspace workspace = new(custom: true, paneTitle: true);
        workspace.FloatGroup();
        LayoutAnchorableFloatingWindow floating = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(workspace.Manager.Layout.FloatingWindows));
        floating.RootPanel!.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable { Title = "Another pane" }));
        workspace.Second.CanFloat = false;
        workspace.Update();
        Assert.False(floating.IsSinglePane);
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        int began = 0;
        workspace.Manager.DockFactory.WindowMoveDragBegin += (_, _) => began++;
        PressAndLeave(workspace);
        Assert.Same(host, Assert.Single(workspace.Manager.FloatingWindows));
        Assert.Equal(0, began);
        Assert.False(host.IsDragging);
        workspace.Window.MouseUp(Center(workspace.Custom, workspace.Window) + new Vector(350, 0), MouseButton.Left);
    }

    [AvaloniaTheory]
    [InlineData("release")]
    [InlineData("model")]
    [InlineData("detach")]
    [InlineData("move")]
    [InlineData("disabled")]
    public void A_Canceled_Title_Press_Does_Not_Float_A_Pane_Later(string cancel)
    {
        using Workspace workspace = new(custom: true);
        Point start = Center(workspace.Custom, workspace.Window);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        if (cancel == "release") workspace.Window.MouseUp(start, MouseButton.Left);
        else if (cancel == "model") workspace.Custom.Model = workspace.Second;
        else if (cancel == "detach")
        {
            workspace.Panel.Children.Remove(workspace.Custom);
            workspace.Panel.Children.Insert(0, workspace.Custom);
            workspace.Update();
        }
        else if (cancel == "disabled") workspace.Custom.IsEnabled = false;
        else workspace.Window.MouseMove(start + new Vector(1, 0));
        workspace.Window.MouseMove(start + new Vector(350, 0), RawInputModifiers.LeftMouseButton);
        Assert.Empty(workspace.Manager.FloatingWindows);
        workspace.Window.MouseUp(start + new Vector(350, 0), MouseButton.Left);
    }

    [AvaloniaFact]
    public void An_Unused_Title_Is_Not_Retained_By_Its_Model_Or_Container()
    {
        using Workspace workspace = new();
        WeakReference<AnchorablePaneTitle> title = CreateUnusedTitle(workspace.First);
        Collect();
        Assert.False(title.TryGetTarget(out _));
        workspace.First.Title = "Still live";
        Assert.NotNull(workspace.Manager.GetLayoutItemFromModel(workspace.First));
    }

    [AvaloniaFact]
    public void A_Retained_Closed_Title_Does_Not_Retain_Its_Model_Or_Manager()
    {
        var (title, model, manager) = CreateClosedTitle();
        Collect();
        Assert.Null(title.Model);
        Assert.Null(title.LayoutItem);
        Assert.Null(title.Parent);
        Assert.Null(title.GetVisualParent());
        Assert.Null(title.DataContext);
        Assert.False(model.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(title);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<AnchorablePaneTitle> CreateUnusedTitle(LayoutAnchorable model) => new(new AnchorablePaneTitle { Model = model });

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (AnchorablePaneTitle, WeakReference<LayoutAnchorable>, WeakReference<DockingManager>) CreateClosedTitle()
    {
        using Workspace workspace = new();
        workspace.Second.Close();
        AnchorablePaneTitle title = Title(workspace.Manager);
        workspace.First.Content = null;
        workspace.First.Close();
        workspace.Update();
        return (title, new(workspace.First), new(workspace.Manager));
    }

    private static void Collect()
    {
        for (int index = 0; index < 3; index++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static void PressAndLeave(Workspace workspace)
    {
        Point start = Center(workspace.Custom, workspace.Window);
        workspace.Window.MouseMove(start);
        workspace.Window.MouseDown(start, MouseButton.Left);
        workspace.Window.MouseMove(start + new Vector(350, 0), RawInputModifiers.LeftMouseButton);
        workspace.Update();
    }

    private static AnchorablePaneTitle Title(Control scope) => scope.GetVisualDescendants().OfType<AnchorablePaneTitle>()
        .Single(title => title.IsEffectivelyVisible && title.Model is not null);
    private static Point Center(Control control, TopLevel top) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), top)!.Value;
    private static void Click(Control control)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        Point point = Center(control, top);
        top.MouseMove(point);
        top.MouseDown(point, MouseButton.Left);
        top.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class TitleProbe : AnchorablePaneTitle
    {
        public List<object?> Models { get; } = new();
        public bool SuppressModelHook { get; init; }
        public int Presses { get; private set; }
        public int Releases { get; private set; }
        public int Exits { get; private set; }
        public void AssignContainer(LayoutItem? item) => SetLayoutItem(item);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Models.Add(e.NewValue);
            if (!SuppressModelHook) base.OnModelChanged(e);
        }
        protected override void OnMouseLeftButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseLeftButtonDown(e); }
        protected override void OnMouseLeftButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnMouseLeftButtonUp(e); }
        protected override void OnMouseLeave(PointerEventArgs e) { Exits++; base.OnMouseLeave(e); }
    }

    private sealed class Workspace : IDisposable
    {
        public Window Window { get; } = new() { Width = 900, Height = 500 };
        public DockPanel Panel { get; } = new();
        public DockingManager Manager { get; }
        public LayoutAnchorablePane Pane { get; }
        public LayoutAnchorable First { get; } = new() { Title = "First", ContentId = "first", CanClose = true, Content = "First tool" };
        public LayoutAnchorable Second { get; } = new() { Title = "Second", ContentId = "second", CanClose = true, Content = "Second tool" };
        public LayoutDocument Document { get; } = new() { Title = "Document", Content = "Document content" };
        public TitleProbe Custom { get; } = new() { Width = 250, Height = 40, HorizontalAlignment = HorizontalAlignment.Left };
        public Control Scope => Manager.FloatingWindows.FirstOrDefault() as Control ?? Manager;

        public Workspace(bool custom = false, bool paneTitle = false)
        {
            Pane = new(First) { DockWidth = new GridLength(350) };
            Pane.Children.Add(Second);
            Manager = new() { Layout = new() { RootPanel = new() { Children = { Pane, new LayoutDocumentPane(Document) } } } };
            First.IsActive = true;
            if (custom)
            {
                Custom.Model = First;
                Control host = paneTitle ? new LayoutAnchorablePaneControl(Pane)
                {
                    Height = 40,
                    Template = new FuncControlTemplate<LayoutAnchorablePaneControl>((_, _) => Custom)
                } : Custom;
                DockPanel.SetDock(host, global::Avalonia.Controls.Dock.Top);
                Panel.Children.Add(host);
            }
            Panel.Children.Add(Manager);
            Window.Content = Panel;
            Window.Show();
            Update();
        }

        public void FloatGroup() { Manager.DockFactory.FloatAllDockables(First.DockModel); Update(); }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); Scope.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
