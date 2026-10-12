using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DocumentMenuTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unprefixed_Menu_Uses_Sorted_Public_Models_And_Native_Activation(bool floating)
    {
        using Workspace workspace = new(floating);
        MenuItem[] entries = workspace.Open();
        Assert.Equal(floating ? new[] { workspace.First } : new[] { workspace.First, workspace.Last }, entries.Select(Model));
        Assert.Equal("Menu: Alpha", Text(entries[0], "menu-header").Text);
        Assert.Same(workspace.First, Text(entries[0], "menu-header").DataContext);
        Click(entries[0]);
        workspace.Update();
        Assert.True(workspace.First.IsActive);
        Assert.Same(workspace.First.Content, workspace.Manager.ActiveContent);
        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Header_Template_And_Metadata_Changes_Refresh_Open_And_Reopened_Menus(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Open();
        workspace.First.DockModel.Title = "Changed";
        workspace.Update();
        Assert.Equal("Menu: Changed", Text(workspace.Entries[0], "menu-header").Text);
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("AlternateHeader");
        workspace.Update();
        Assert.Equal("ID: first", Text(workspace.Entries[0], "alternate-header").Text);
        workspace.Menu.Hide();
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("MenuHeader");
        Assert.Equal("Menu: Changed", Text(workspace.Open()[0], "menu-header").Text);
    }

    [AvaloniaFact]
    public void Menu_Header_Falls_Back_To_Document_Header_And_Selectors_Have_Precedence()
    {
        using Workspace workspace = new();
        workspace.Manager.DocumentHeaderTemplate = workspace.Template("AlternateHeader");
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = null;
        Assert.Same(workspace.Manager.DocumentHeaderTemplate, workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Equal("ID: first", Text(workspace.Open()[0], "alternate-header").Text);
        HeaderKindSelector selector = new() { DocumentTemplate = workspace.Template("MenuHeader") };
        workspace.Manager.DocumentHeaderTemplateSelector = selector;
        workspace.Update();
        Assert.Same(selector, workspace.Manager.DocumentPaneMenuItemHeaderTemplateSelector);
        Assert.Null(workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Equal("Menu: Alpha", Text(workspace.Entries[0], "menu-header").Text);
        HeaderKindSelector menuSelector = new() { DocumentTemplate = workspace.Template("AlternateHeader") };
        workspace.Manager.DocumentPaneMenuItemHeaderTemplateSelector = menuSelector;
        workspace.Manager.DocumentHeaderTemplateSelector = new HeaderKindSelector();
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("MenuHeader");
        workspace.Update();
        Assert.Same(menuSelector, workspace.Manager.DocumentPaneMenuItemHeaderTemplateSelector);
        Assert.Null(workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Equal("ID: first", Text(workspace.Entries[0], "alternate-header").Text);
        Assert.Contains(menuSelector.Calls, call => ReferenceEquals(call.Item, workspace.First));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Default_Icons_Observe_Layout_And_Container_Icon_Sources(bool floating)
    {
        using Workspace workspace = new(floating);
        IImage icon = new DrawingImage { Drawing = new GeometryDrawing { Brush = Brushes.Red, Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) } };
        workspace.First.IconSource = icon;
        Image image = workspace.Open()[0].GetVisualDescendants().OfType<Image>().Single(value => value.Classes.Contains("avalondock-menu-icon"));
        Assert.Same(icon, image.Source);
        Assert.True(image.IsVisible);
        workspace.Manager.GetLayoutItemFromModel(workspace.First)!.IconSource = null;
        workspace.Update();
        Assert.Null(image.Source);
        Assert.False(image.IsVisible);
        workspace.Manager.GetLayoutItemFromModel(workspace.First)!.IconSource = icon;
        workspace.Update();
        Assert.Same(icon, image.Source);
    }

    [AvaloniaFact]
    public void Icon_Selector_Receives_The_Current_Icon_And_Menu_Item_Without_Clearing_Explicit_Template()
    {
        using Workspace workspace = new();
        workspace.Manager.IconContentTemplate = workspace.Template("Icon");
        MenuItem entry = workspace.Open()[0];
        Assert.Equal("Icon: first", Text(entry, "custom-icon").Text);
        object previous = Assert.IsType<MenuItemEx>(entry).Icon!;
        IconSelector selector = new() { Template = workspace.Template("AlternateHeader") };
        workspace.Manager.IconContentTemplateSelector = selector;
        workspace.Update();
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, previous) && ReferenceEquals(call.Container, entry));
        Assert.Same(workspace.Template("Icon"), workspace.Manager.IconContentTemplate);
        Assert.Equal("ID: first", Text(entry, "alternate-header").Text);
        workspace.Manager.IconContentTemplateSelector = null;
        workspace.Update();
        Assert.Equal("Icon: first", Text(entry, "custom-icon").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Null_Icon_Selector_Result_Preserves_The_Current_Icon_Across_Template_Changes(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Manager.IconContentTemplate = workspace.Template("Icon");
        MenuItemEx entry = Assert.IsType<MenuItemEx>(workspace.Open()[0]);
        Control icon = Assert.IsAssignableFrom<Control>(entry.Icon);
        IconSelector selector = new();
        workspace.Manager.IconContentTemplateSelector = selector;
        workspace.Update();
        Assert.Same(icon, entry.Icon);
        Assert.Same(workspace.First, icon.DataContext);
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, icon) && ReferenceEquals(call.Container, entry));
        workspace.Manager.IconContentTemplate = workspace.Template("AlternateHeader");
        workspace.Update();
        Assert.Same(icon, entry.Icon);
        Assert.Equal("Icon: first", Text(entry, "custom-icon").Text);
        workspace.Menu.Hide();
        workspace.Open();
        Assert.Same(icon, workspace.Entries[0].Icon);
        workspace.Manager.IconContentTemplateSelector = null;
        workspace.Update();
        Assert.NotSame(icon, entry.Icon);
        Assert.Equal("ID: first", Text(entry, "alternate-header").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Initially_Empty_Icon_Is_Passed_To_The_Selector_And_Template_Data_Is_Inherited(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Manager.IconContentTemplate = null;
        IconSelector selector = new() { Template = workspace.Template("Icon") };
        workspace.Manager.IconContentTemplateSelector = selector;
        MenuItemEx entry = Assert.IsType<MenuItemEx>(workspace.Open()[0]);
        Assert.Contains(selector.Calls, call => call.Item is null && ReferenceEquals(call.Container, entry));
        Assert.Equal("Icon: first", Text(entry, "custom-icon").Text);
        Assert.Same(workspace.First, Text(entry, "custom-icon").DataContext);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Null_Menu_Header_Falls_Back_To_The_Document_Header_Even_With_A_Menu_Selector(bool floating)
    {
        using Workspace workspace = new(floating);
        workspace.Manager.DocumentHeaderTemplate = workspace.Template("AlternateHeader");
        HeaderKindSelector selector = new() { DocumentTemplate = workspace.Template("MenuHeader") };
        workspace.Manager.DocumentPaneMenuItemHeaderTemplateSelector = selector;
        Assert.Same(workspace.Manager.DocumentHeaderTemplate, workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        MenuItem entry = workspace.Open()[0];
        Assert.Equal("ID: first", Text(entry, "alternate-header").Text);
        Assert.Empty(selector.Calls);
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("MenuHeader");
        workspace.Update();
        Assert.Null(workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Equal("Menu: Alpha", Text(entry, "menu-header").Text);
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = null;
        workspace.Update();
        Assert.Same(workspace.Manager.DocumentHeaderTemplate, workspace.Manager.DocumentPaneMenuItemHeaderTemplate);
        Assert.Equal("ID: first", Text(entry, "alternate-header").Text);
        workspace.Manager.DocumentHeaderTemplate = null;
        workspace.Update();
        Assert.Equal("ID: first", Text(entry, "alternate-header").Text);
        workspace.Manager.CoerceValue(DockingManager.DocumentPaneMenuItemHeaderTemplateProperty);
        workspace.Update();
        Assert.Equal("Menu: Alpha", Text(entry, "menu-header").Text);
    }

    private sealed class IconSelector : DataTemplateSelector
    {
        internal IDataTemplate? Template { get; init; }
        internal List<(object? Item, AvaloniaObject Container)> Calls { get; } = new();
        public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
        {
            Calls.Add((item, container));
            return Template;
        }
    }

    [AvaloniaFact]
    public void Generated_Icons_Follow_Entries_After_The_Open_Menu_Rebuilds_Its_Containers()
    {
        using Workspace workspace = new();
        workspace.Manager.IconContentTemplate = workspace.Template("Icon");
        workspace.Open();
        LayoutDocumentPane pane = Assert.IsType<LayoutDocumentPane>(workspace.First.Parent);
        LayoutDocument added = new() { ContentId = "added", Title = "A new document", Content = new TextBlock() };
        pane.Children.Add(added);
        workspace.Update();
        foreach (MenuItem entry in workspace.Entries)
        {
            Assert.IsType<MenuItemEx>(entry);
            Assert.Equal("Icon: " + Model(entry).ContentId, Text(entry, "custom-icon").Text);
            Assert.Same(Model(entry), Text(entry, "custom-icon").DataContext);
        }
        pane.Children.Remove(added);
        workspace.Update();
        foreach (MenuItem entry in workspace.Entries)
            Assert.Equal("Icon: " + Model(entry).ContentId, Text(entry, "custom-icon").Text);
        workspace.Menu.Hide();
        workspace.Open();
        foreach (MenuItem entry in workspace.Entries)
            Assert.Equal("Icon: " + Model(entry).ContentId, Text(entry, "custom-icon").Text);
    }

    [AvaloniaFact]
    public void Menu_Uses_Replaced_Activation_Command_And_Live_Enablement()
    {
        using Workspace workspace = new();
        MenuItem entry = workspace.Open()[0];
        int calls = 0;
        using BehaviorSubject<bool> enabled = new(false);
        using var command = ReactiveCommand.Create(() => calls++, enabled, outputScheduler: ImmediateScheduler.Instance);
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.First)!;
        item.ActivateCommand = command;
        workspace.Update();
        Assert.Same(command, entry.Command);
        Assert.False(entry.IsEffectivelyEnabled);
        enabled.OnNext(true);
        workspace.Update();
        Assert.True(entry.IsEffectivelyEnabled);
        workspace.First.IsEnabled = false;
        workspace.Update();
        Assert.False(entry.IsEffectivelyEnabled);
        workspace.First.IsEnabled = true;
        workspace.Update();
        Click(entry);
        Assert.Equal(1, calls);
    }

    [AvaloniaFact]
    public void Collection_Changes_Refresh_Sorted_Menu_Without_Reordering_Native_Tabs()
    {
        using Workspace workspace = new();
        workspace.Open();
        LayoutDocument middle = new() { Title = "Middle", ContentId = "middle" };
        workspace.Pane.Children.Insert(0, middle);
        workspace.Update();
        Assert.Equal(new[] { workspace.First, middle, workspace.Last }, workspace.Entries.Select(Model));
        Assert.Equal(new[] { middle, workspace.Last, workspace.First }, workspace.Pane.Children);
        workspace.Pane.Children.Remove(workspace.First);
        workspace.Update();
        Assert.Equal(new[] { middle, workspace.Last }, workspace.Entries.Select(Model));
    }

    [AvaloniaFact]
    public void Tools_Docked_As_Documents_Keep_Their_Public_Model_In_Menu_Selectors()
    {
        using Workspace workspace = new();
        LayoutAnchorable tool = workspace.Manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
        tool.Title = "Beta";
        tool.DockAsDocument();
        HeaderKindSelector selector = new()
        {
            DocumentTemplate = workspace.Template("MenuHeader"), ToolTemplate = workspace.Template("AlternateHeader")
        };
        workspace.Manager.DocumentPaneMenuItemHeaderTemplateSelector = selector;
        MenuItem[] entries = workspace.Open();
        Assert.Equal(new LayoutContent[] { workspace.First, tool, workspace.Last }, entries.Select(Model));
        Assert.Equal("ID: tool", Text(entries[1], "alternate-header").Text);
        Assert.Contains(selector.Calls, call => ReferenceEquals(call.Item, tool));
        Click(entries[1]);
        Assert.True(tool.IsActive);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void XML_Replacement_Closes_The_Old_Menu_And_Uses_New_Models(bool floating)
    {
        using Workspace workspace = new(floating);
        MenuFlyout oldMenu = workspace.Menu;
        TextBlock oldHeader = Text(workspace.Open()[0], "menu-header");
        ContentPresenter oldPresenter = Presenter(oldHeader);
        XmlLayoutSerializer serializer = new(workspace.Manager);
        using StringWriter writer = new();
        serializer.Serialize(writer);
        serializer.Deserialize(new StringReader(writer.ToString()));
        workspace.Update();
        Assert.False(oldMenu.IsOpen);
        Assert.Null(oldPresenter.Content);
        Assert.Null(oldPresenter.Child);
        LayoutDocument restored = workspace.Manager.Layout.Descendents().OfType<LayoutDocument>().Single(content => content.ContentId == "first");
        Assert.NotSame(workspace.First, restored);
        Control scope = floating ? (HostWindow)restored.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : workspace.Manager;
        Button button = MenuButton(scope);
        Click(button);
        MenuFlyout menu = (MenuFlyout)button.Flyout!;
        Assert.True(menu.IsOpen);
        Assert.Contains(menu.Popup.Child!.GetVisualDescendants().OfType<TextBlock>(), text => text.Classes.Contains("menu-header") && ReferenceEquals(text.DataContext, restored));
        menu.Hide();
    }

    [AvaloniaFact]
    public void View_Detachment_Closes_And_Reattachment_Reopens_The_Document_Menu()
    {
        using Workspace workspace = new();
        workspace.Open();
        MenuFlyout menu = workspace.Menu;
        workspace.Window.Content = null;
        Dispatcher.UIThread.RunJobs();
        Assert.False(menu.IsOpen);
        workspace.Manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("AlternateHeader");
        workspace.Window.Content = workspace.Manager;
        workspace.Update();
        Assert.Equal("ID: first", Text(workspace.Open()[0], "alternate-header").Text);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Removed_Menu_Header_And_Icon_Release_Their_Model_And_Manager(bool renderBeforeRemoval)
    {
        var result = RemoveMenuViews(renderBeforeRemoval);
        for (int i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        foreach (ContentPresenter view in result.Views)
        {
            Assert.Null(view.Content);
            Assert.Null(view.Child);
            Assert.Null(view.DataContext);
            Assert.Null(view.Parent);
            Assert.Null(view.TemplatedParent);
            Assert.Null(TopLevel.GetTopLevel(view));
        }
        Assert.False(result.Model.TryGetTarget(out _));
        Assert.False(result.Manager.TryGetTarget(out _));
        GC.KeepAlive(result.Views);
    }

    [AvaloniaFact]
    public void Public_Menu_Header_Change_Callbacks_Are_Invoked()
    {
        TrackingManager manager = new();
        using Workspace workspace = new();
        manager.DocumentPaneMenuItemHeaderTemplate = workspace.Template("MenuHeader");
        manager.DocumentPaneMenuItemHeaderTemplateSelector = new HeaderKindSelector();
        Assert.True(manager.TemplateChanges > 0);
        Assert.Equal(1, manager.SelectorChanges);
        Assert.Null(manager.DocumentPaneMenuItemHeaderTemplate);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContentPresenter[] Views, WeakReference<LayoutDocument> Model, WeakReference<DockingManager> Manager) RemoveMenuViews(bool renderBeforeRemoval)
    {
        using Workspace workspace = new();
        workspace.Manager.IconContentTemplate = workspace.Template("Icon");
        workspace.Menu.ShowAt(workspace.Button);
        workspace.Update();
        MenuItem item = workspace.Entries[0];
        ContentPresenter[] views = { Presenter(Text(item, "menu-header")), Presenter(Text(item, "custom-icon")) };
        if (renderBeforeRemoval) TopLevel.GetTopLevel(item)!.CaptureRenderedFrame()?.Dispose();
        workspace.Pane.Children.Remove(workspace.First);
        workspace.Update();
        workspace.Window.CaptureRenderedFrame()?.Dispose();
        return (views, new(workspace.First), new(workspace.Manager));
    }

    private sealed class TrackingManager : DockingManager
    {
        public int TemplateChanges { get; private set; }
        public int SelectorChanges { get; private set; }
        protected override void OnDocumentPaneMenuItemHeaderTemplateChanged(AvaloniaPropertyChangedEventArgs e) => TemplateChanges++;
        protected override void OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnDocumentPaneMenuItemHeaderTemplateSelectorChanged(e);
            SelectorChanges++;
        }
    }

    private static ContentPresenter Presenter(Control child) => child.GetVisualAncestors().OfType<ContentPresenter>().First(presenter => ReferenceEquals(presenter.Child, child));
    private static Button MenuButton(Control scope) => scope.GetVisualDescendants().OfType<Button>().Single(button => button.Classes.Contains("avalondock-document-menu"));

    private static LayoutContent Model(MenuItem item) => (LayoutContent)item.DataContext!;
    private static TextBlock Text(Control scope, string name) => scope.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Classes.Contains(name));

    private static void Click(Control control)
    {
        TopLevel host = TopLevel.GetTopLevel(control)!;
        host.UpdateLayout();
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), host)!.Value;
        host.MouseMove(point);
        IInputElement? hit = host.InputHitTest(point);
        Assert.True(ReferenceEquals(hit, control) || hit is Visual visual && visual.GetVisualAncestors().Contains(control));
        host.MouseDown(point, MouseButton.Left);
        host.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class Workspace : IDisposable
    {
        public DocumentMenuFixture Window { get; } = new();
        public DockingManager Manager { get; }
        public LayoutDocumentPane Pane { get; }
        public LayoutDocument First { get; }
        public LayoutDocument Last { get; }
        public Control Scope { get; }
        public MenuFlyout Menu => (MenuFlyout)Button.Flyout!;
        public Button Button => MenuButton(Scope);
        public MenuItem[] Entries
        {
            get
            {
                ItemsControl presenter = (ItemsControl)Menu.Popup.Child!;
                return Enumerable.Range(0, presenter.ItemCount).Select(index => (MenuItem)presenter.ContainerFromIndex(index)!).ToArray();
            }
        }

        public Workspace(bool floating = false)
        {
            Manager = Window.FindControl<DockingManager>("Manager")!;
            Pane = Manager.Layout.Descendents().OfType<LayoutDocumentPane>().Single();
            Last = (LayoutDocument)Pane.Children[0];
            First = (LayoutDocument)Pane.Children[1];
            Window.Show();
            Update();
            if (floating)
            {
                First.Float();
            }
            Scope = floating ? (HostWindow)First.FindParent<LayoutFloatingWindow>()!.DockWindow.Host! : Manager;
            Update();
        }

        public IDataTemplate Template(string key) => (IDataTemplate)Window.Resources[key]!;
        public MenuItem[] Open()
        {
            Click(Button);
            Update();
            Assert.True(Menu.IsOpen);
            return Entries;
        }
        public void Update()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            if (Scope is HostWindow host) host.UpdateLayout();
            if (Scope is not null && Scope.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => button.Classes.Contains("avalondock-document-menu"))?.Flyout is MenuFlyout { IsOpen: true } menu)
                TopLevel.GetTopLevel(menu.Popup.Child)?.UpdateLayout();
        }
        public void Dispose()
        {
            if (Scope.GetVisualDescendants().OfType<Button>().FirstOrDefault(button => button.Classes.Contains("avalondock-document-menu"))?.Flyout is MenuFlyout menu)
                menu.Hide();
            Manager.Layout.FloatingWindows.Clear();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
