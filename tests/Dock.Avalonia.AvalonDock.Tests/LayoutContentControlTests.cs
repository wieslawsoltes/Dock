using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutContentControlTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Model_Redirection_From_Container_Or_Enablement_Changes_Keeps_The_Final_Subscription(bool tool, bool fromContainer)
    {
        LayoutContent first = CreateModel(tool);
        first.IsEnabled = false;
        LayoutContent replacement = CreateModel(tool);
        DockingManager manager = CreateManager(first, replacement);
        TemplatedControl control = CreateControl(tool);
        control.PropertyChanged += (_, change) =>
        {
            if (ReferenceEquals(Model(control), first) &&
                (fromContainer ? change.Property.Name == nameof(LayoutDocumentControl.LayoutItem) : change.Property == InputElement.IsEnabledProperty))
                SetModel(control, replacement);
        };
        SetModel(control, first);
        Assert.Same(replacement, Model(control));
        Assert.Same(manager.GetLayoutItemFromModel(replacement), Item(control));
        Assert.True(control.IsEnabled);
        replacement.IsEnabled = false;
        Assert.False(control.IsEnabled);
        first.IsEnabled = true;
        Assert.False(control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Model_Replacement_Resolves_Existing_Containers_And_Stops_Observing_The_Old_Model(bool tool)
    {
        LayoutContent first = CreateModel(tool);
        LayoutContent second = CreateModel(tool);
        DockingManager manager = CreateManager(first, second);
        TemplatedControl control = CreateControl(tool);
        Assert.Null(Model(control));
        Assert.Null(Item(control));
        Assert.Equal(!tool, control.Focusable);
        first.IsEnabled = false;
        SetModel(control, first);
        Assert.Same(manager.GetLayoutItemFromModel(first), Item(control));
        Assert.False(control.IsEnabled);
        SetModel(control, second);
        Assert.Same(manager.GetLayoutItemFromModel(second), Item(control));
        Assert.True(control.IsEnabled);
        first.IsEnabled = true;
        first.IsEnabled = false;
        Assert.True(control.IsEnabled);
        second.IsEnabled = false;
        Assert.False(control.IsEnabled);
        SetModel(control, null);
        Assert.Null(Item(control));
        Assert.True(control.IsEnabled);
        second.IsEnabled = true;
        second.IsEnabled = false;
        Assert.True(control.IsEnabled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unattached_Model_Resolves_Its_Container_When_Added_To_A_Manager(bool tool)
    {
        LayoutContent model = CreateModel(tool);
        TemplatedControl control = CreateControl(tool);
        SetModel(control, model);
        Assert.Null(Item(control));
        DockingManager manager = CreateManager(model);
        Assert.Same(manager.GetLayoutItemFromModel(model), Item(control));
        Assert.Same(model, Model(control));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Controls_Keep_The_Assigned_Model_And_Release_A_Detached_Container(bool tool)
    {
        LayoutContent model = CreateModel(tool);
        DockingManager manager = CreateManager(model);
        TemplatedControl control = CreateControl(tool);
        SetModel(control, model);
        Assert.NotNull(Item(control));
        model.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.Same(model, Model(control));
        Assert.Null(Item(control));
        Assert.Null(manager.GetLayoutItemFromModel(model));
    }

    [AvaloniaFact]
    public void Document_Model_Hook_And_Protected_Container_Setter_Are_Usable_By_Derived_Controls()
    {
        LayoutDocument model = new();
        DockingManager manager = CreateManager(model);
        DocumentProbe control = new() { Model = model };
        Assert.Same(model, Assert.Single(control.Changes).NewValue);
        Assert.Same(manager.GetLayoutItemFromModel(model), control.LayoutItem);
        Assert.True(LayoutDocumentControl.LayoutItemProperty.IsReadOnly);
        control.SetContainer(null);
        Assert.Null(control.LayoutItem);
        LayoutDocumentItem replacement = new();
        control.SetContainer(replacement);
        Assert.Same(replacement, control.GetValue(LayoutDocumentControl.LayoutItemProperty));
        control.Model = null;
        Assert.Null(control.LayoutItem);
        DocumentProbe suppressing = new() { SuppressModelHook = true, Model = model };
        Assert.Same(model, suppressing.Model);
        Assert.Null(suppressing.LayoutItem);
    }

    [AvaloniaFact]
    public void Anchorable_Model_Hook_And_Protected_Container_Setter_Are_Usable_By_Derived_Controls()
    {
        LayoutAnchorable model = new();
        DockingManager manager = CreateManager(model);
        AnchorableProbe control = new() { Model = model };
        Assert.Same(model, Assert.Single(control.Changes).NewValue);
        Assert.Same(manager.GetLayoutItemFromModel(model), control.LayoutItem);
        Assert.True(LayoutAnchorableControl.LayoutItemProperty.IsReadOnly);
        control.SetContainer(null);
        Assert.Null(control.LayoutItem);
        LayoutAnchorableItem replacement = new();
        control.SetContainer(replacement);
        Assert.Same(replacement, control.GetValue(LayoutAnchorableControl.LayoutItemProperty));
        control.Model = null;
        Assert.Null(control.LayoutItem);
        AnchorableProbe suppressing = new() { SuppressModelHook = true, Model = model };
        Assert.Same(model, suppressing.Model);
        Assert.Null(suppressing.LayoutItem);
    }

    [AvaloniaTheory]
    [InlineData("Document", false)]
    [InlineData("LegacyDocument", false)]
    [InlineData("Tool", true)]
    [InlineData("LegacyTool", true)]
    public void Both_Xml_Namespaces_Compile_Models_And_ReadOnly_Containers_In_Custom_Themes(string key, bool tool)
    {
        ContentControlFixture window = new();
        LayoutContent model = CreateModel(tool);
        model.Title = "Original title";
        model.Content = new TextBox { Text = "Editor" };
        DockingManager manager = CreateManager(model);
        TemplatedControl control = Assert.IsAssignableFrom<TemplatedControl>(((IDataTemplate)window.Resources[key]!).Build(model));
        control.DataContext = model;
        control.Theme = (ControlTheme)window.Resources[tool ? "ToolTheme" : "DocumentTheme"]!;
        window.Content = control;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Same(model, Model(control));
            LayoutItem item = manager.GetLayoutItemFromModel(model)!;
            Assert.Same(item, Item(control));
            Assert.Contains(item.View, control.GetVisualDescendants());
            TextBlock title = control.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains("container-title"));
            Assert.Equal(model.Title, title.Text);
            model.Title = "Updated title";
            Assert.Equal(model.Title, title.Text);
            Assert.Same(model.Content, item.View.Child);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public void Disabling_Active_Content_Selects_The_First_Enabled_Sibling_Or_Clears_Selection(bool tool, bool enabledSibling)
    {
        LayoutContent disabled = CreateModel(tool);
        disabled.IsEnabled = false;
        LayoutContent next = CreateModel(tool);
        next.IsEnabled = enabledSibling;
        LayoutContent active = CreateModel(tool);
        DockingManager manager = CreateManager(disabled, next, active);
        TemplatedControl control = CreateControl(tool);
        SetModel(control, active);
        active.IsActive = true;
        active.IsEnabled = false;
        Assert.False(control.IsEnabled);
        ILayoutContentSelector pane = (ILayoutContentSelector)active.Parent!;
        Assert.Equal(enabledSibling ? 1 : -1, pane.SelectedContentIndex);
        Assert.Same(enabledSibling ? next : null, pane.SelectedContent);
        GC.KeepAlive(manager);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disabling_Inactive_Content_Does_Not_Change_Selection(bool tool)
    {
        LayoutContent first = CreateModel(tool);
        LayoutContent second = CreateModel(tool);
        DockingManager manager = CreateManager(first, second);
        TemplatedControl control = CreateControl(tool);
        SetModel(control, first);
        second.IsActive = true;
        first.IsEnabled = false;
        Assert.Same(second, ((ILayoutContentSelector)second.Parent!).SelectedContent);
        Assert.Same(second, manager.Layout.ActiveContent);
        Assert.False(control.IsEnabled);
    }

    [AvaloniaFact]
    public void Native_Docked_And_Floating_Content_Uses_Public_Controls_And_Preserves_The_Editor_View()
    {
        using Workspace workspace = new();
        LayoutDocumentControl document = workspace.DocumentControl;
        LayoutAnchorableControl tool = workspace.ToolControl;
        LayoutItem item = document.LayoutItem!;
        ContentPresenter view = item.View;
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Document), item);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Tool), tool.LayoutItem);
        Assert.Contains(view, document.GetVisualDescendants());
        workspace.Editor.Focus();
        workspace.Editor.CaretIndex = workspace.Editor.Text!.Length;
        workspace.Window.KeyTextInput(" changed");
        string? edited = workspace.Editor.Text;
        workspace.Document.Float();
        workspace.Update();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        LayoutDocumentControl floating = Assert.Single(host.GetVisualDescendants().OfType<LayoutDocumentControl>());
        Assert.Same(workspace.Document, floating.Model);
        Assert.Same(item, floating.LayoutItem);
        Assert.Contains(view, floating.GetVisualDescendants());
        workspace.Document.Dock();
        workspace.Update();
        Assert.Same(item, workspace.DocumentControl.LayoutItem);
        Assert.Contains(view, workspace.DocumentControl.GetVisualDescendants());
        Assert.Same(workspace.Editor, view.Child);
        Assert.Equal(edited, workspace.Editor.Text);
    }

    [AvaloniaFact]
    public void A_Tool_Docked_As_A_Document_Uses_The_Document_Control_With_Its_Anchorable_Container()
    {
        using Workspace workspace = new();
        LayoutItem item = workspace.ToolControl.LayoutItem!;
        ContentPresenter view = item.View;
        workspace.Tool.DockAsDocument();
        workspace.Tool.IsActive = true;
        workspace.Update();
        LayoutDocumentControl control = workspace.Manager.GetVisualDescendants().OfType<LayoutDocumentControl>().Single(value => ReferenceEquals(value.Model, workspace.Tool));
        Assert.Same(item, control.LayoutItem);
        Assert.IsType<LayoutAnchorableItem>(control.LayoutItem);
        Assert.Contains(view, control.GetVisualDescendants());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Closing_Content_Clears_A_Retained_Generated_Control(bool tool)
    {
        using Workspace workspace = new();
        if (!tool)
            foreach (LayoutDocument other in workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Where(value => !ReferenceEquals(value, workspace.Document)).ToArray()) other.Close();
        LayoutContent model = tool ? workspace.Tool : workspace.Document;
        TemplatedControl control = tool ? workspace.ToolControl : workspace.DocumentControl;
        LayoutItem item = Item(control)!;
        model.CanClose = true;
        model.Close();
        workspace.Update();
        Assert.Null(Model(control));
        Assert.Null(Item(control));
        Assert.Null(control.DataContext);
        Assert.Null(item.View.Content);
        Assert.Null(item.View.Child);
    }

    [AvaloniaFact]
    public void Xml_Replacement_Clears_Generated_Controls_And_Presents_The_Restored_Models()
    {
        using Workspace workspace = new();
        LayoutDocumentControl previousDocument = workspace.DocumentControl;
        LayoutAnchorableControl previousTool = workspace.ToolControl;
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter text = new();
        serializer.Serialize(text);
        serializer.Deserialize(new StringReader(text.ToString()));
        workspace.Update();
        Assert.Null(previousDocument.Model);
        Assert.Null(previousDocument.LayoutItem);
        Assert.Null(previousTool.Model);
        Assert.Null(previousTool.LayoutItem);
        LayoutDocumentControl current = Assert.Single(workspace.Manager.GetVisualDescendants().OfType<LayoutDocumentControl>());
        Assert.NotSame(workspace.Document, current.Model);
        Assert.Same(workspace.Manager.Layout, current.Model!.Root);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(current.Model), current.LayoutItem);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retaining_The_Model_And_Container_Does_Not_Retain_An_Unused_Control(bool tool)
    {
        LayoutContent model = CreateModel(tool);
        DockingManager manager = CreateManager(model);
        LayoutItem item = manager.GetLayoutItemFromModel(model)!;
        WeakReference<TemplatedControl> control = CreateUnusedControl(model, tool);
        Collect();
        Assert.False(control.TryGetTarget(out _));
        model.IsEnabled = false;
        Assert.Same(model, item.LayoutElement);
        GC.KeepAlive(manager);
        GC.KeepAlive(model);
        GC.KeepAlive(item);
    }

    [AvaloniaTheory]
    [InlineData(MouseButton.Left)]
    [InlineData(MouseButton.Right)]
    public void Native_Document_Pointer_Input_Activates_The_Public_Model(MouseButton button)
    {
        using Workspace workspace = new();
        workspace.Tool.IsActive = true;
        workspace.Update();
        Click(workspace.Editor, button);
        Assert.Same(workspace.Document, workspace.Manager.Layout.ActiveContent);
        Assert.True(workspace.Document.IsSelected);
    }

    [AvaloniaFact]
    public void Document_Preview_Hooks_Run_Before_The_Editor_Handles_Pointer_Input()
    {
        LayoutDocument model = new() { Content = new TextBox { Text = "Editor" } };
        DockingManager manager = CreateManager(model);
        DocumentProbe control = new() { Model = model };
        Window window = new() { Width = 400, Height = 300, Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(manager.GetLayoutItemFromModel(model)!.View, control.GetVisualDescendants());
            Click((Control)model.Content, MouseButton.Left);
            Click((Control)model.Content, MouseButton.Right);
            Assert.Equal(1, control.LeftPresses);
            Assert.Equal(1, control.RightPresses);
            Assert.True(control.FocusChanges > 0);
            Assert.True(model.IsActive);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void Anchorable_Focus_Hook_Receives_Real_Editor_Focus()
    {
        LayoutAnchorable model = new() { Content = new TextBox { Text = "Tool" } };
        DockingManager manager = CreateManager(model);
        AnchorableProbe control = new() { Model = model };
        Window window = new() { Width = 400, Height = 300, Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(control.Focusable);
            Assert.Contains(manager.GetLayoutItemFromModel(model)!.View, control.GetVisualDescendants());
            Assert.True(((Control)model.Content).Focus());
            Assert.True(control.FocusChanges > 0);
            Assert.True(model.IsActive);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Focus_Returning_From_A_Floating_Host_Does_Not_Activate_An_Unrelated_Model(bool tool)
    {
        using Workspace workspace = new();
        workspace.Document.Float();
        workspace.Update();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        LayoutContent model = CreateModel(tool);
        DockingManager manager = CreateManager(model);
        manager.Layout.ActiveContent = null;
        Control editor = new TextBox();
        FocusChangedEventArgs args = new(InputElement.GotFocusEvent) { OldFocusedElement = host, NewFocusedElement = editor, Source = editor };
        if (tool) new AnchorableProbe { Model = (LayoutAnchorable)model }.FocusHook(args);
        else new DocumentProbe { Model = model }.FocusHook(args);
        Assert.False(model.IsActive);
        Assert.Null(manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Keyboard_Focus_Inside_A_Nested_Manager_Activates_Both_Content_Models(bool tool)
    {
        TextBox editor = new() { Text = "Nested editor" };
        LayoutDocument inner = new() { Content = editor };
        DockingManager innerManager = CreateManager(inner);
        LayoutContent outer = CreateModel(tool);
        outer.Content = innerManager;
        DockingManager outerManager = CreateManager(outer);
        Control control = tool ? new AnchorableProbe { Model = (LayoutAnchorable)outer } : new DocumentProbe { Model = outer };
        Window window = new() { Content = control, Width = 600, Height = 400 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            outerManager.Layout.ActiveContent = null;
            Assert.True(editor.Focus(NavigationMethod.Tab));
            Assert.True(outer.IsActive);
            Assert.Same(outer, outerManager.Layout.ActiveContent);
            Assert.True(inner.IsActive);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Explicit_Focus_Hook_Call_Activates_The_Model_Even_When_The_Control_Is_Disabled(bool tool)
    {
        LayoutContent model = CreateModel(tool);
        DockingManager manager = CreateManager(model);
        manager.Layout.ActiveContent = null;
        model.IsEnabled = false;
        Control editor = new TextBox();
        FocusChangedEventArgs args = new(InputElement.GotFocusEvent) { NewFocusedElement = editor, Source = editor };
        if (tool) new AnchorableProbe { Model = (LayoutAnchorable)model }.FocusHook(args);
        else new DocumentProbe { Model = model }.FocusHook(args);
        Assert.True(model.IsActive);
        Assert.Same(model, manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Focus_Hooks_Read_The_Current_Model_When_A_Derived_Model_Hook_Suppresses_Base(bool tool, bool clearModel)
    {
        LayoutContent original = CreateModel(tool);
        LayoutContent replacement = CreateModel(tool);
        DockingManager manager = CreateManager(original, replacement);
        LayoutItem originalItem = manager.GetLayoutItemFromModel(original)!;
        manager.Layout.ActiveContent = null;
        FocusChangedEventArgs args = new(InputElement.GotFocusEvent);
        if (tool)
        {
            AnchorableProbe control = new() { Model = (LayoutAnchorable)original };
            control.SuppressModelHook = true;
            control.Model = clearModel ? null : (LayoutAnchorable)replacement;
            Assert.Same(originalItem, control.LayoutItem);
            control.FocusHook(args);
        }
        else
        {
            DocumentProbe control = new() { Model = original };
            control.SuppressModelHook = true;
            control.Model = clearModel ? null : replacement;
            Assert.Same(originalItem, control.LayoutItem);
            control.FocusHook(args);
        }
        Assert.False(original.IsActive);
        Assert.Equal(!clearModel, replacement.IsActive);
        Assert.Same(clearModel ? null : replacement, manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void AutoHide_And_Floating_Tools_Keep_The_Anchorable_Control_And_Existing_View()
    {
        using Workspace workspace = new();
        LayoutItem item = workspace.ToolControl.LayoutItem!;
        ContentPresenter view = item.View;
        workspace.Tool.ToggleAutoHide();
        workspace.Update();
        Click(Assert.Single(workspace.Manager.GetVisualDescendants().OfType<global::Dock.Avalonia.Controls.ToolPinItemControl>()), MouseButton.Left);
        Assert.True(workspace.Tool.IsAutoHidden);
        Assert.Same(item, workspace.ToolControl.LayoutItem);
        Assert.Contains(view, workspace.ToolControl.GetVisualDescendants());
        workspace.Tool.ToggleAutoHide();
        workspace.Tool.Float();
        workspace.Update();
        LayoutFloatingWindowControl host = Assert.Single(workspace.Manager.FloatingWindows);
        LayoutAnchorableControl floating = Assert.Single(host.GetVisualDescendants().OfType<LayoutAnchorableControl>());
        Assert.Same(workspace.Tool, floating.Model);
        Assert.Same(item, floating.LayoutItem);
        Assert.Contains(view, floating.GetVisualDescendants());
        workspace.Tool.Dock();
        workspace.Update();
        Assert.Same(item, workspace.ToolControl.LayoutItem);
        Assert.Contains(view, workspace.ToolControl.GetVisualDescendants());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Retaining_A_Closed_Generated_Control_Does_Not_Retain_Its_Model_Or_Manager(bool tool)
    {
        var (control, model, manager) = CreateClosedControl(tool);
        Collect();
        Assert.Null(Model(control));
        Assert.Null(Item(control));
        Assert.Null(control.Parent);
        Assert.Null(control.GetVisualParent());
        Assert.False(model.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(control);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (TemplatedControl, WeakReference<LayoutContent>, WeakReference<DockingManager>) CreateClosedControl(bool tool)
    {
        using Workspace workspace = new();
        if (!tool)
            foreach (LayoutDocument other in workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Where(value => !ReferenceEquals(value, workspace.Document)).ToArray()) other.Close();
        LayoutContent model = tool ? workspace.Tool : workspace.Document;
        TemplatedControl control = tool ? workspace.ToolControl : workspace.DocumentControl;
        model.Content = null;
        model.CanClose = true;
        model.Close();
        workspace.Update();
        return (control, new(model), new(workspace.Manager));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<TemplatedControl> CreateUnusedControl(LayoutContent model, bool tool)
    {
        TemplatedControl control = CreateControl(tool);
        SetModel(control, model);
        return new(control);
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

    private static void Click(Control control, MouseButton button)
    {
        TopLevel host = TopLevel.GetTopLevel(control)!;
        host.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), host)!.Value;
        host.MouseMove(point);
        host.MouseDown(point, button);
        host.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private static LayoutContent CreateModel(bool tool) => tool
        ? new LayoutAnchorable { CanClose = true, Content = new object() }
        : new LayoutDocument { Content = new object() };
    private static TemplatedControl CreateControl(bool tool) => tool ? new LayoutAnchorableControl() : new LayoutDocumentControl();
    private static LayoutContent? Model(TemplatedControl control) => control is LayoutDocumentControl document ? document.Model : ((LayoutAnchorableControl)control).Model;
    private static LayoutItem? Item(TemplatedControl control) => control is LayoutDocumentControl document ? document.LayoutItem : ((LayoutAnchorableControl)control).LayoutItem;
    private static void SetModel(TemplatedControl control, LayoutContent? model)
    {
        if (control is LayoutDocumentControl document) document.Model = model;
        else ((LayoutAnchorableControl)control).Model = (LayoutAnchorable?)model;
    }

    private static DockingManager CreateManager(params LayoutContent[] contents)
    {
        LayoutDocumentPane documents = new();
        LayoutAnchorablePane tools = new();
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { tools, documents } } } };
        foreach (LayoutContent content in contents)
        {
            if (content is LayoutAnchorable tool) tools.Children.Add(tool);
            else documents.Children.Add(content);
        }
        return manager;
    }

    private sealed class DocumentProbe : LayoutDocumentControl
    {
        public List<AvaloniaPropertyChangedEventArgs> Changes { get; } = new();
        public bool SuppressModelHook { get; set; }
        public int LeftPresses { get; private set; }
        public int RightPresses { get; private set; }
        public int FocusChanges { get; private set; }
        public void SetContainer(LayoutItem? item) => SetLayoutItem(item);
        public void FocusHook(FocusChangedEventArgs args) => OnPreviewGotKeyboardFocus(args);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Changes.Add(e);
            if (!SuppressModelHook) base.OnModelChanged(e);
        }
        protected override void OnPreviewGotKeyboardFocus(FocusChangedEventArgs e) { FocusChanges++; base.OnPreviewGotKeyboardFocus(e); }
        protected override void OnPreviewMouseLeftButtonDown(PointerPressedEventArgs e) { LeftPresses++; base.OnPreviewMouseLeftButtonDown(e); }
        protected override void OnPreviewMouseRightButtonDown(PointerPressedEventArgs e) { RightPresses++; base.OnPreviewMouseRightButtonDown(e); }
    }

    private sealed class AnchorableProbe : LayoutAnchorableControl
    {
        public List<AvaloniaPropertyChangedEventArgs> Changes { get; } = new();
        public bool SuppressModelHook { get; set; }
        public int FocusChanges { get; private set; }
        public void SetContainer(LayoutItem? item) => SetLayoutItem(item);
        public void FocusHook(FocusChangedEventArgs args) => OnGotKeyboardFocus(args);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Changes.Add(e);
            if (!SuppressModelHook) base.OnModelChanged(e);
        }
        protected override void OnGotKeyboardFocus(FocusChangedEventArgs e) { FocusChanges++; base.OnGotKeyboardFocus(e); }
    }

    private sealed class Workspace : IDisposable
    {
        public XamlFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutDocument Document { get; }
        public LayoutAnchorable Tool { get; }
        public TextBox Editor { get; }
        public LayoutDocumentControl DocumentControl => Manager.GetVisualDescendants().OfType<LayoutDocumentControl>().Single(control => ReferenceEquals(control.Model, Document));
        public LayoutAnchorableControl ToolControl => Manager.GetVisualDescendants().OfType<LayoutAnchorableControl>().Single(control => ReferenceEquals(control.Model, Tool));
        public Workspace()
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Document = Manager.Layout.Descendents().OfType<LayoutDocument>().Single(model => model.ContentId == "one");
            Tool = Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
            Editor = Window.FindControl<TextBox>("Editor")!;
            Window.Show();
            Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Manager.Layout.FloatingWindows.Clear(); Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
