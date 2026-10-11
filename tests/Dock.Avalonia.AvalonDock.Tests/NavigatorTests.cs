using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class NavigatorTests
{
    [AvaloniaFact]
    public void Snapshot_Uses_Live_Containers_And_Stable_Activation_Order_While_Filtering_Content()
    {
        using Workspace workspace = new();
        workspace.Documents[0].LastActivationTimeStamp = new DateTime(2020, 1, 1);
        workspace.Documents[1].LastActivationTimeStamp = new DateTime(2020, 1, 1);
        workspace.Documents[2].LastActivationTimeStamp = new DateTime(2021, 1, 1);
        LayoutDocument disabled = new() { IsEnabled = false };
        ((LayoutDocumentPane)workspace.Documents[0].Parent!).Children.Add(disabled);
        LayoutAnchorable hidden = new();
        LayoutAnchorable disabledTool = new() { IsEnabled = false };
        ((LayoutAnchorablePane)workspace.Tools[0].Parent!).Children.Add(hidden);
        ((LayoutAnchorablePane)workspace.Tools[0].Parent!).Children.Add(disabledTool);
        hidden.Hide();
        workspace.Tools[0].ToggleAutoHide();
        NavigatorWindow navigator = workspace.Manager.CreateNavigatorWindow();
        try
        {
            Assert.Equal(new[] { workspace.Documents[2], workspace.Documents[0], workspace.Documents[1] },
                navigator.Documents!.Select(item => item.LayoutElement));
            foreach (LayoutItem item in navigator.Documents!)
                Assert.Same(item, workspace.Manager.GetLayoutItemFromModel(item.LayoutElement!));
            Assert.Equal(2, navigator.Anchorables!.Count());
            Assert.All(navigator.Anchorables!, item => Assert.Contains(item.LayoutElement, workspace.Tools));
            Assert.Same(navigator.Documents![1], navigator.SelectedDocument);
            Assert.Null(navigator.SelectedAnchorable);
            Assert.False(navigator.ShowActivated);
            Assert.False(navigator.ShowInTaskbar);
            Assert.Same(navigator, navigator.DataContext);
            using NavigatorProbe defaults = new(workspace.Manager);
            Assert.Equal("Active Files", defaults.LayoutDocumentsLabel);
            Assert.Equal("Active Tool Windows", defaults.LayoutAnchorablesLabel);
            workspace.Documents[2].Title = "Changed live title";
            Assert.Equal("Changed live title", navigator.Documents[0].Title);
            workspace.Documents[0].LastActivationTimeStamp = DateTime.MaxValue;
            Assert.Same(workspace.Documents[2], navigator.Documents[0].LayoutElement);
        }
        finally { navigator.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0, 0, false, false)]
    [InlineData(1, 0, false, false)]
    [InlineData(0, 1, false, false)]
    [InlineData(1, 1, false, false)]
    [InlineData(0, 2, false, true)]
    [InlineData(1, 2, false, true)]
    [InlineData(2, 0, true, false)]
    [InlineData(2, 2, true, false)]
    public void Initial_Selection_Matches_The_Upstream_Second_Item_Rules(int documents, int tools, bool documentSelected, bool toolSelected)
    {
        LayoutDocumentPane documentPane = new();
        LayoutAnchorablePane toolPane = new();
        for (int index = 0; index < documents; index++) documentPane.Children.Add(new LayoutDocument());
        for (int index = 0; index < tools; index++) toolPane.Children.Add(new LayoutAnchorable());
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { documentPane, toolPane } } } };
        using NavigatorProbe navigator = new(manager);
        Assert.Equal(documentSelected, navigator.SelectedDocument is not null);
        Assert.Equal(toolSelected, navigator.SelectedAnchorable is not null);
        if (documentSelected) Assert.Same(navigator.Documents![1], navigator.SelectedDocument);
        if (toolSelected) Assert.Same(navigator.Anchorables!.ElementAt(1), navigator.SelectedAnchorable);
    }

    [AvaloniaFact]
    public void Ctrl_Tab_Uses_One_Modal_Navigator_And_Release_Activates_And_Focuses_The_Selected_Document()
    {
        using Workspace workspace = new();
        Assert.True(((Control)workspace.Documents[0].Content!).Focus());
        workspace.View.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        NavigatorWindow navigator = workspace.Manager.Last!;
        Assert.True(navigator.IsVisible);
        Assert.Same(workspace.View, navigator.Owner);
        Assert.Contains(navigator, workspace.View.OwnedWindows);
        Assert.Equal(WindowStartupLocation.CenterOwner, navigator.WindowStartupLocation);
        Assert.False(workspace.Manager.GetVisualDescendants().OfType<global::Dock.Avalonia.Controls.DockControl>().Single().IsOpen);
        workspace.Manager.Open();
        Assert.Equal(1, workspace.Manager.Created);
        LayoutDocument selected = (LayoutDocument)navigator.SelectedDocument!.LayoutElement!;
        navigator.KeyReleaseQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        Assert.True(navigator.IsVisible);
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.False(navigator.IsVisible);
        Assert.DoesNotContain(navigator, workspace.View.OwnedWindows);
        Assert.Same(selected, workspace.Manager.Layout.ActiveContent);
        Assert.Same(selected.Content, workspace.View.FocusManager!.GetFocusedElement());
        workspace.Manager.Open();
        Assert.Equal(2, workspace.Manager.Created);
        Assert.NotSame(navigator, workspace.Manager.Last);
    }

    [AvaloniaFact]
    public void Focus_Moves_Into_Nonfocusable_Content_After_Native_Deferred_Presentation()
    {
        using Workspace workspace = new();
        TextBox editor = new() { Text = "Nested editor" };
        workspace.Documents[1].Content = new Border { Child = editor };
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        Assert.Same(workspace.Documents[1], navigator.SelectedDocument!.LayoutElement);
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        Assert.Same(editor, workspace.View.FocusManager!.GetFocusedElement());
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    public void Empty_And_Single_Item_Navigators_Can_Close_On_Control_Release(int documents)
    {
        using Workspace workspace = new();
        LayoutDocumentPane pane = new();
        for (int index = 0; index < documents; index++) pane.Children.Add(new LayoutDocument());
        workspace.Manager.Layout = new() { RootPanel = new(pane) };
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        Assert.Null(navigator.SelectedDocument);
        Assert.Null(navigator.SelectedAnchorable);
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.False(navigator.IsVisible);
    }

    [AvaloniaFact]
    public void Navigation_Cycles_Lists_Without_Activating_And_Ignores_Shift_Like_Upstream()
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        LayoutDocumentItem[] documents = navigator.Documents!;
        LayoutAnchorableItem[] tools = navigator.Anchorables!.ToArray();
        LayoutContent? active = workspace.Manager.Layout.ActiveContent;
        Press(navigator, PhysicalKey.ArrowDown);
        Assert.Same(documents[2], navigator.SelectedDocument);
        Press(navigator, PhysicalKey.ArrowDown);
        Assert.Same(documents[0], navigator.SelectedDocument);
        Press(navigator, PhysicalKey.ArrowUp);
        Assert.Same(documents[2], navigator.SelectedDocument);
        Press(navigator, PhysicalKey.ArrowRight);
        Assert.Null(navigator.SelectedDocument);
        Assert.Same(tools[1], navigator.SelectedAnchorable);
        Press(navigator, PhysicalKey.ArrowDown);
        Assert.Same(tools[0], navigator.SelectedAnchorable);
        Press(navigator, PhysicalKey.ArrowUp);
        Assert.Same(tools[1], navigator.SelectedAnchorable);
        Press(navigator, PhysicalKey.Tab);
        Assert.Same(documents[0], navigator.SelectedDocument);
        Press(navigator, PhysicalKey.Tab, RawInputModifiers.Control | RawInputModifiers.Shift);
        Assert.Same(documents[1], navigator.SelectedDocument);
        Press(navigator, PhysicalKey.ArrowLeft);
        Assert.Same(tools[1], navigator.SelectedAnchorable);
        Assert.Same(active, workspace.Manager.Layout.ActiveContent);
        Assert.True(navigator.IsVisible);
        navigator.KeyReleaseQwerty(PhysicalKey.ControlRight, RawInputModifiers.None);
        Assert.Same(tools[1].LayoutElement, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Pointer_Selection_Invokes_The_Current_Command(bool tool)
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        navigator.MouseMove(default);
        ListBox list = navigator.GetVisualDescendants().OfType<ListBox>().Single(value => value.Name == (tool ? "PART_AnchorableListBox" : "PART_DocumentListBox"));
        LayoutItem item = (LayoutItem)list.Items[0]!;
        int count = 0;
        using var command = ReactiveCommand.Create(() => count++, outputScheduler: ImmediateScheduler.Instance);
        item.ActivateCommand = command;
        bool closed = false;
        navigator.Closed += (_, _) => closed = true;
        ListBoxItem target = Assert.IsType<ListBoxItem>(list.ContainerFromIndex(0));
        Assert.Contains(target.GetVisualDescendants().OfType<TextBlock>(), value => value.Text == item.Title);
        Assert.Same(workspace.Manager, item.Parent);
        Click(navigator, target);
        Assert.Equal(1, count);
        Assert.False(navigator.IsVisible);
        Assert.Equal(tool, closed);
        Assert.Same(workspace.Manager, item.Parent);
        workspace.Manager.Open();
        Assert.Equal(2, workspace.Manager.Created);
        if (!closed) navigator.Close();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selection_Respects_Null_And_Disabled_Commands_And_Key_Release_Uses_A_Replacement(bool tool)
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        LayoutItem selected = tool ? navigator.Anchorables!.First() : navigator.Documents![0];
        using BehaviorSubject<bool> enabled = new(false);
        int count = 0;
        using var unavailable = ReactiveCommand.Create(() => count++, enabled, ImmediateScheduler.Instance);
        selected.ActivateCommand = null;
        SetSelection(navigator, selected);
        Assert.True(navigator.IsVisible);
        SetSelection(navigator, null);
        selected.ActivateCommand = unavailable;
        SetSelection(navigator, selected);
        Assert.True(navigator.IsVisible);
        Assert.Equal(0, count);
        using var replacement = ReactiveCommand.Create(() => count++, outputScheduler: ImmediateScheduler.Instance);
        selected.ActivateCommand = replacement;
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.Equal(1, count);
        Assert.False(navigator.IsVisible);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Content_Opens_The_Owning_Managers_Navigator(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tools[0] : workspace.Documents[0];
        content.Float();
        LayoutFloatingWindowControl host = (LayoutFloatingWindowControl)content.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
        Dispatcher.UIThread.RunJobs();
        Assert.True(((Control)content.Content!).Focus());
        host.KeyPressQwerty(PhysicalKey.Tab, RawInputModifiers.Control);
        NavigatorWindow navigator = workspace.Manager.Last!;
        Assert.NotNull(navigator);
        Assert.True(navigator.IsVisible);
        Assert.Contains(content, navigator.Documents!.Select(item => item.LayoutElement).Concat(navigator.Anchorables!.Select(item => item.LayoutElement)));
        LayoutContent selected = navigator.SelectedDocument!.LayoutElement!;
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.Same(selected, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Labels_Compile_In_Both_Xml_Namespaces_And_Updates_Reach_The_Template()
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        Assert.Equal("Recent documents", navigator.LayoutDocumentsLabel);
        navigator.Classes.Add("bound-labels");
        Assert.Equal("Tools for Recent documents", navigator.LayoutAnchorablesLabel);
        navigator.LayoutDocumentsLabel = "Editors";
        Assert.Equal("Tools for Editors", navigator.LayoutAnchorablesLabel);
        Assert.Contains(navigator.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Editors");
        Assert.Contains(navigator.GetVisualDescendants().OfType<TextBlock>(), text => text.Text == "Tools for Editors");
        Assert.False(navigator.CanResize);
        Assert.Equal(WindowDecorations.None, navigator.WindowDecorations);
    }

    [AvaloniaFact]
    public void A_Nonfocusable_List_Uses_Its_Selected_Container_For_Keyboard_Input()
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        navigator.MouseMove(default);
        ListBox list = navigator.GetVisualDescendants().OfType<ListBox>().Single(value => value.Name == "PART_DocumentListBox");
        list.Focusable = false;
        Press(navigator, PhysicalKey.ArrowDown);
        Assert.Same(list.ContainerFromItem(navigator.SelectedDocument!), navigator.FocusManager!.GetFocusedElement());
        LayoutContent selected = navigator.SelectedDocument!.LayoutElement!;
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.Same(selected, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Control_Release_Closes_Without_Executing_An_Unavailable_Command(bool disabled)
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        LayoutContent? active = workspace.Manager.Layout.ActiveContent;
        using BehaviorSubject<bool> canExecute = new(false);
        int executions = 0;
        using var command = ReactiveCommand.Create(() => executions++, canExecute, ImmediateScheduler.Instance);
        navigator.SelectedDocument!.ActivateCommand = disabled ? command : null;
        navigator.KeyReleaseQwerty(PhysicalKey.ControlLeft, RawInputModifiers.None);
        Assert.False(navigator.IsVisible);
        Assert.Equal(0, executions);
        Assert.Same(active, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Protected_Sources_Hooks_And_Empty_Lists_Are_Usable_From_A_Derived_Control()
    {
        using Workspace workspace = new();
        using NavigatorProbe navigator = new(workspace.Manager);
        navigator.Show();
        Assert.Equal(1, navigator.TemplateCalls);
        navigator.OnApplyTemplate();
        Assert.Equal(2, navigator.TemplateCalls);
        LayoutDocumentItem[] documents = navigator.Documents!;
        LayoutAnchorableItem[] tools = navigator.Anchorables!.ToArray();
        navigator.Sources(null, null);
        Assert.Null(navigator.Documents);
        Assert.Null(navigator.Anchorables);
        navigator.Down(Key.Down);
        Assert.True(navigator.IsVisible);
        navigator.Sources(documents, new List<LayoutAnchorableItem>(tools));
        Assert.Same(documents, navigator.Documents);
        navigator.Down(Key.Right);
        Assert.Same(documents[0], navigator.SelectedDocument);
        navigator.Down(Key.Right);
        Assert.Same(tools[0], navigator.SelectedAnchorable);
        Assert.True(navigator.DocumentChanges > 0);
        Assert.True(navigator.ToolChanges > 0);
        navigator.Up(Key.Escape);
        Assert.False(navigator.IsVisible);
        Assert.Same(tools[0].LayoutElement, workspace.Manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Replacement_And_Detachment_Close_The_Navigator_And_Reentrant_Or_Failing_Creation_Recovers()
    {
        using Workspace workspace = new();
        workspace.Manager.Reenter = true;
        workspace.Manager.Open();
        Assert.Equal(1, workspace.Manager.Created);
        NavigatorWindow first = workspace.Manager.Last!;
        workspace.Manager.Layout = new();
        Assert.False(first.IsVisible);
        workspace.Manager.Open();
        NavigatorWindow second = workspace.Manager.Last!;
        workspace.View.Content = null;
        Assert.False(second.IsVisible);
        workspace.View.Content = workspace.Manager;
        workspace.Manager.ThrowOnCreate = true;
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.Open());
        workspace.Manager.ThrowOnCreate = false;
        workspace.Manager.Open();
        Assert.True(workspace.Manager.Last!.IsVisible);
    }

    [AvaloniaFact]
    public void Closing_Veto_Preserves_The_Current_Navigator_Until_Retry()
    {
        using Workspace workspace = new();
        workspace.Manager.Open();
        NavigatorWindow navigator = workspace.Manager.Last!;
        EventHandler<WindowClosingEventArgs> cancel = (_, args) => args.Cancel = true;
        navigator.Closing += cancel;
        navigator.Close();
        workspace.Manager.Open();
        Assert.Same(navigator, workspace.Manager.Last);
        Assert.Equal(1, workspace.Manager.Created);
        navigator.Closing -= cancel;
        navigator.Close();
        workspace.Manager.Open();
        Assert.Equal(2, workspace.Manager.Created);
    }

    [AvaloniaFact]
    public void The_Manager_Does_Not_Retain_A_Closed_Navigator()
    {
        using Workspace workspace = new();
        WeakReference reference = OpenAndClose(workspace.Manager);
        for (int attempt = 0; attempt < 5 && reference.IsAlive; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            // Closing queues disposal of the composition target. Render that
            // batch before testing collection of its navigator presentation source.
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.IsAlive);
        GC.KeepAlive(workspace.Manager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference OpenAndClose(TrackingNavigatorManager manager)
    {
        manager.Open();
        NavigatorWindow window = manager.Last!;
        window.Close();
        manager.Last = null;
        return new(window);
    }

    private static void SetSelection(NavigatorWindow navigator, LayoutItem? item)
    {
        navigator.SelectedDocument = null;
        navigator.SelectedAnchorable = null;
        if (item is LayoutDocumentItem document) navigator.SelectedDocument = document;
        else if (item is LayoutAnchorableItem tool) navigator.SelectedAnchorable = tool;
    }

    private static void Press(Window window, PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.Control)
    {
        window.KeyPressQwerty(key, modifiers);
        window.KeyReleaseQwerty(key, modifiers);
    }

    private static void Click(Window window, Control control)
    {
        Dispatcher.UIThread.RunJobs();
        Point point = control.TranslatePoint(new(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;
        window.MouseDown(point, MouseButton.Left);
        if (window.IsVisible) window.MouseUp(point, MouseButton.Left);
    }

    private sealed class Workspace : IDisposable
    {
        internal TrackingNavigatorManager Manager { get; } = new();
        internal Window View { get; }
        internal LayoutDocument[] Documents { get; } = new[] { "A", "B", "C" }.Select(title =>
            new LayoutDocument { Title = title, Content = new TextBox { Text = title } }).ToArray();
        internal LayoutAnchorable[] Tools { get; } = new[] { "One", "Two" }.Select(title =>
            new LayoutAnchorable { Title = title, Content = new TextBox { Text = title } }).ToArray();
        internal Workspace()
        {
            LayoutDocumentPane documents = new();
            foreach (LayoutDocument document in Documents) documents.Children.Add(document);
            LayoutAnchorablePane tools = new();
            foreach (LayoutAnchorable tool in Tools) tools.Children.Add(tool);
            Manager.Layout = new() { RootPanel = new() { Children = { documents, tools } } };
            View = new() { Content = Manager, Width = 800, Height = 600 };
            View.Show();
            Dispatcher.UIThread.RunJobs();
            Documents[0].IsActive = true;
            for (int index = 0; index < Documents.Length; index++) Documents[index].LastActivationTimeStamp = new DateTime(2020, 1, 3 - index);
        }
        public void Dispose()
        {
            Manager.Last?.Close();
            Manager.Layout.FloatingWindows.Clear();
            View.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class NavigatorProbe : NavigatorWindow, IDisposable
    {
        internal int DocumentChanges { get; private set; }
        internal int ToolChanges { get; private set; }
        internal int TemplateCalls { get; private set; }
        internal NavigatorProbe(DockingManager manager) : base(manager) { }
        internal void Sources(LayoutDocumentItem[]? documents, IEnumerable<LayoutAnchorableItem>? tools) { SetDocuments(documents); SetAnchorables(tools); }
        internal void Down(Key key) => OnPreviewKeyDown(new() { Key = key });
        internal void Up(Key key) => OnPreviewKeyUp(new() { Key = key });
        public override void OnApplyTemplate() { TemplateCalls++; base.OnApplyTemplate(); }
        protected override void OnSelectedDocumentChanged(AvaloniaPropertyChangedEventArgs e) { DocumentChanges++; base.OnSelectedDocumentChanged(e); }
        protected override void OnSelectedAnchorableChanged(AvaloniaPropertyChangedEventArgs e) { ToolChanges++; base.OnSelectedAnchorableChanged(e); }
        public void Dispose() => Close();
    }
}

public class TrackingNavigatorManager : DockingManager
{
    public NavigatorWindow? Last { get; set; }
    public int Created { get; private set; }
    public bool ThrowOnCreate { get; set; }
    public bool Reenter { get; set; }
    public void Open() => ShowNavigatorWindow();
    public override NavigatorWindow CreateNavigatorWindow()
    {
        if (ThrowOnCreate) throw new InvalidOperationException("Test creation failure");
        if (Reenter) Open();
        Created++;
        return Last = new NavigatorFixture(this);
    }
}
