using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;
using DropDownButton = AvalonDock.Controls.DropDownButton;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class MenuControlTests
{
    [AvaloniaTheory]
    [InlineData("Button")]
    [InlineData("LegacyButton")]
    [InlineData("Area")]
    [InlineData("LegacyArea")]
    public void Both_Xml_Namespaces_Open_Compiled_Menus_And_Invoke_The_Existing_Item_Command(string key)
    {
        using Workspace workspace = new(key);
        int opening = 0;
        workspace.Menu.Opening += (_, _) => opening++;
        workspace.Open();
        Assert.True(workspace.Menu.IsOpen);
        Assert.Equal(1, opening);
        Assert.Same(workspace.Item, workspace.Menu.DataContext);
        Assert.Same(workspace.Control, workspace.Menu.PlacementTarget);
        Assert.Equal(workspace.Control is DropDownButton ? PlacementMode.Bottom : PlacementMode.Pointer, workspace.Menu.Placement);
        if (workspace.Control is DropDownButton button) Assert.True(button.IsChecked);
        MenuItemEx item = Assert.IsType<MenuItemEx>(Assert.Single(workspace.Menu.Items));
        Assert.Same(workspace.Item.ActivateCommand, item.Command);
        Click(item);
        Assert.Same(workspace.Document, workspace.Manager.Layout.ActiveContent);
        Assert.False(workspace.Menu.IsOpen);
        Assert.Null(workspace.Menu.DataContext);
        Assert.Null(workspace.Menu.PlacementTarget);
        Assert.Null(workspace.Control.ContextMenu);
        if (workspace.Control is DropDownButton closed) Assert.False(closed.IsChecked);
    }

    [AvaloniaFact]
    public void An_Open_ContextMenuEx_Refreshes_A_Compiled_Snapshot_Source_And_Generates_MenuItemEx()
    {
        using Workspace workspace = new("Button");
        SnapshotMenuSource source = new();
        ContextMenuEx menu = (ContextMenuEx)workspace.Window.Resources["SnapshotMenu"]!;
        menu.DataContext = source;
        menu.Open(workspace.Control);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new[] { "First" }, menu.ItemsSource!.Cast<string>());
        Assert.IsType<MenuItemEx>(Assert.Single(menu.GetRealizedContainers()));
        menu.Close();
        source.Values.Add("Second");
        Assert.Equal(new[] { "First" }, menu.ItemsSource!.Cast<string>());
        menu.Open(workspace.Control);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(new[] { "First", "Second" }, menu.ItemsSource!.Cast<string>());
        Assert.Equal(2, menu.GetRealizedContainers().Count());
        menu.Close();
    }

    [AvaloniaFact]
    public void Icon_Templates_Inherit_Item_Data_And_A_Selector_Takes_Precedence_Including_Null()
    {
        using Workspace workspace = new("Button");
        workspace.Open();
        MenuItemEx item = (MenuItemEx)workspace.Menu.Items[0]!;
        TextBlock icon = Assert.IsType<TextBlock>(item.Icon);
        Assert.Same(workspace.Item, icon.DataContext);
        Assert.Equal(workspace.Document.Title, icon.Text);
        workspace.Document.Title = "Changed";
        Assert.Equal("Changed", icon.Text);
        IconSelector selector = new();
        item.IconTemplateSelector = selector;
        Assert.Same(icon, selector.LastItem);
        Assert.Same(item, selector.LastContainer);
        Assert.Same(icon, item.Icon);
        item.IconTemplate = (IDataTemplate)workspace.Window.Resources["AlternateIcon"]!;
        Assert.Same(icon, item.Icon);
        item.IconTemplateSelector = null;
        TextBlock alternate = Assert.IsType<TextBlock>(item.Icon);
        Assert.NotSame(icon, alternate);
        Assert.Equal("Icon: Changed", alternate.Text);
        item.Icon = null;
        Assert.Null(item.Icon);
    }

    [AvaloniaTheory]
    [InlineData("Button")]
    [InlineData("Area")]
    public void Cancellation_Replacement_And_View_Detachment_Clear_Menu_State(string key)
    {
        using Workspace workspace = new(key);
        bool cancel = true;
        workspace.Menu.Opening += (_, args) => args.Cancel = cancel;
        workspace.Open();
        Assert.False(workspace.Menu.IsOpen);
        Assert.Null(workspace.Menu.DataContext);
        if (workspace.Control is DropDownButton canceled) Assert.False(canceled.IsChecked);
        cancel = false;
        workspace.Open();
        Assert.True(workspace.Menu.IsOpen);
        ContextMenuEx replacement = new();
        replacement.Items.Add(new MenuItemEx { Header = "Replacement" });
        SetMenu(workspace.Control, replacement);
        Assert.False(workspace.Menu.IsOpen);
        Assert.Null(workspace.Menu.DataContext);
        workspace.Open();
        Assert.True(replacement.IsOpen);
        workspace.Window.Content = null;
        Assert.False(replacement.IsOpen);
        Assert.Null(replacement.PlacementTarget);
        Assert.Null(replacement.DataContext);
        workspace.Window.Content = workspace.Control;
        Dispatcher.UIThread.RunJobs();
        workspace.Open();
        Assert.True(replacement.IsOpen);
    }

    [AvaloniaFact]
    public void Area_Hooks_Use_Press_Bubbling_And_Release_Tunneling_And_Respect_Child_Menus()
    {
        AreaProbe area = new() { Width = 200, Height = 50, Content = new Border { Background = Brushes.Transparent } };
        ContextMenuEx own = new();
        own.Items.Add(new MenuItem { Header = "Own" });
        area.DropDownContextMenu = own;
        Window window = new() { Content = area, Width = 400, Height = 300 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Click(area, MouseButton.Right);
            Assert.Equal(1, area.Presses);
            Assert.Equal(1, area.Releases);
            Assert.True(own.IsOpen);
            own.Close();
            Control child = (Control)area.Content!;
            ContextMenu childMenu = new();
            childMenu.Items.Add(new MenuItem { Header = "Child" });
            child.ContextMenu = childMenu;
            Click(child, MouseButton.Right);
            Assert.True(childMenu.IsOpen);
            Assert.False(own.IsOpen);
            childMenu.Close();
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ContextMenu_Container_And_Opening_Hooks_Are_Overridable_Without_Replacing_Explicit_Items()
    {
        MenuProbe menu = new();
        MenuItem explicitItem = new() { Header = "Explicit" };
        Separator separator = new();
        menu.Items.Add("Generated");
        menu.Items.Add(explicitItem);
        menu.Items.Add(separator);
        Button target = new() { Content = "Target" };
        Window window = new() { Content = target };
        try
        {
            window.Show();
            menu.Open(target);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, menu.Openings);
            Assert.Equal(1, menu.Containers);
            Assert.IsType<IconProbe>(menu.ContainerFromIndex(0));
            Assert.Same(explicitItem, menu.ContainerFromIndex(1));
            Assert.Same(separator, menu.ContainerFromIndex(2));
        }
        finally { menu.Close(); window.Close(); }
    }

    private static void SetMenu(Control control, ContextMenu menu)
    {
        if (control is DropDownButton button) button.DropDownContextMenu = menu;
        else ((DropDownControlArea)control).DropDownContextMenu = menu;
    }

    private static void Click(Control control, MouseButton button = MouseButton.Left)
    {
        TopLevel top = TopLevel.GetTopLevel(control)!;
        Point point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), top)!.Value;
        top.MouseMove(point);
        top.MouseDown(point, button);
        top.MouseUp(point, button);
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class AreaProbe : DropDownControlArea
    {
        public int Presses { get; private set; }
        public int Releases { get; private set; }
        protected override void OnMouseRightButtonDown(PointerPressedEventArgs e) { Presses++; base.OnMouseRightButtonDown(e); }
        protected override void OnPreviewMouseRightButtonUp(PointerReleasedEventArgs e) { Releases++; base.OnPreviewMouseRightButtonUp(e); }
    }
    private sealed class IconProbe : MenuItemEx { }
    private sealed class MenuProbe : ContextMenuEx
    {
        public int Openings { get; private set; }
        public int Containers { get; private set; }
        protected override void OnOpened(RoutedEventArgs e) { Openings++; base.OnOpened(e); }
        protected override AvaloniaObject GetContainerForItemOverride() { Containers++; return new IconProbe(); }
    }
    private sealed class IconSelector : DataTemplateSelector
    {
        public object? LastItem { get; private set; }
        public AvaloniaObject? LastContainer { get; private set; }
        public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
        {
            LastItem = item;
            LastContainer = container;
            return null;
        }
    }
    private sealed class Workspace : IDisposable
    {
        public MenuControlFixture Window { get; } = new();
        public LayoutDocument Document { get; } = new() { Title = "Document" };
        public DockingManager Manager { get; }
        public LayoutItem Item { get; }
        public Control Control { get; }
        public ContextMenu Menu { get; }

        public Workspace(string key)
        {
            Manager = new() { Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(Document)) } };
            Item = Manager.GetLayoutItemFromModel(Document)!;
            Control = ((IDataTemplate)Window.Resources[key]!).Build(Item)!;
            Control.DataContext = Item;
            Menu = Control is DropDownButton button ? button.DropDownContextMenu! : ((DropDownControlArea)Control).DropDownContextMenu!;
            Window.Content = Control;
            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }
        public void Open() => Click(Control, Control is DropDownButton ? MouseButton.Left : MouseButton.Right);
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
