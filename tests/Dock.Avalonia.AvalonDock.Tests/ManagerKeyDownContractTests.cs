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
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ManagerKeyDownContractTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_Base_KeyDown_Hook_Opens_The_Active_Content_Menu(bool tool)
    {
        using Workspace workspace = new(tool);
        KeyEventArgs args = new() { RoutedEvent = InputElement.KeyDownEvent, Key = Key.F10, KeyModifiers = KeyModifiers.Shift };

        workspace.Manager.InvokeBase(args);

        Assert.True(workspace.Menu.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Model), workspace.Menu.DataContext);
        Control target = Assert.IsAssignableFrom<Control>(workspace.Menu.PlacementTarget);
        Assert.True(tool ? target is LayoutAnchorableControl : target is LayoutDocumentControl);
        Assert.Equal(PlacementMode.AnchorAndGravity, workspace.Menu.Placement);
        Assert.Equal(new Rect(), workspace.Menu.PlacementRect);
        Popup popup = Assert.IsType<Popup>(workspace.Menu.Parent);
        Assert.Equal(PlacementMode.AnchorAndGravity, popup.Placement);
        Assert.Same(target, popup.PlacementTarget);
        Assert.False(args.Handled);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Shift_F10_Opens_The_Menu_On_Key_Press(bool tool)
    {
        using Workspace workspace = new(tool);
        Assert.True(workspace.Editor.Focus());

        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        Assert.True(workspace.Menu.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Model), workspace.Menu.DataContext);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        Assert.True(workspace.Menu.IsOpen);
        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Canceling_Opening_Does_Not_Retry_On_Key_Release(bool tool)
    {
        using Workspace workspace = new(tool);
        int attempts = 0;
        workspace.Menu.Opening += (_, args) => { attempts++; args.Cancel = true; };
        Assert.True(workspace.Editor.Focus());

        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        Assert.Equal(1, attempts);
        Assert.False(workspace.Menu.IsOpen);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        Assert.Equal(1, attempts);
        Assert.False(workspace.Menu.IsOpen);
        Assert.Same(workspace.Editor, workspace.Window.FocusManager!.GetFocusedElement());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Suppressing_The_Base_Hook_Prevents_Both_Keyboard_Phases(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.Suppress = true;
        Assert.True(workspace.Editor.Focus());

        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        Assert.Equal(1, workspace.Manager.Calls);
        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_Apps_Key_Still_Works_After_A_Suppressed_Shift_F10(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.Suppress = true;
        Assert.True(workspace.Editor.Focus());
        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        Assert.False(workspace.Menu.IsOpen);

        workspace.Window.KeyPress(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, null);
        workspace.Window.KeyRelease(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, null);

        Assert.True(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Override_Can_Replace_The_Menu_Before_Calling_Base(bool tool)
    {
        using Workspace workspace = new(tool);
        ContextMenu replacement = new() { Items = { new MenuItem { Header = "Replacement" } } };
        workspace.Manager.BeforeBase = () =>
        {
            if (tool) workspace.Manager.AnchorableContextMenu = replacement;
            else workspace.Manager.DocumentContextMenu = replacement;
        };
        Assert.True(workspace.Editor.Focus());
        try
        {
            workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

            Assert.False(workspace.Menu.IsOpen);
            Assert.True(replacement.IsOpen);
            Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Model), replacement.DataContext);
        }
        finally { replacement.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Content_Owned_Menus_Keep_Their_Avalonia_Keyboard_Route(bool tool, bool cancel)
    {
        using Workspace workspace = new(tool);
        ContextMenu child = new() { Items = { new MenuItem { Header = "Child command" } } };
        child.Opening += (_, args) => args.Cancel = cancel;
        workspace.Editor.ContextMenu = child;
        Assert.True(workspace.Editor.Focus());

        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        Assert.False(child.IsOpen);
        Assert.False(workspace.Menu.IsOpen);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        Assert.Equal(!cancel, child.IsOpen);
        Assert.False(workspace.Menu.IsOpen);
        child.Close();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_Hook_Also_Accepts_The_Active_Visual_Itself(bool tool)
    {
        using Workspace workspace = new(tool);
        workspace.Manager.ActiveContent = workspace.Editor;
        KeyEventArgs args = Shortcut();
        args.Handled = true;

        workspace.Manager.InvokeBase(args);

        Assert.True(workspace.Menu.IsOpen);
        Assert.True(args.Handled);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Model), workspace.Menu.DataContext);
    }

    [AvaloniaTheory]
    [InlineData("null")]
    [InlineData("object")]
    [InlineData("unparented")]
    [InlineData("content-control")]
    public void The_Hook_Requires_A_Visual_Descendant_Of_A_Public_Content_Control(string active)
    {
        using Workspace workspace = new(false);
        Assert.True(workspace.Editor.Focus());
        workspace.Manager.ActiveContent = active switch
        {
            "null" => null,
            "object" => new object(),
            "unparented" => new Border(),
            _ => workspace.Manager.GetVisualDescendants().OfType<LayoutDocumentControl>().Single()
        };

        workspace.Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        workspace.Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaFact]
    public void A_Nonvisual_Avalonia_Object_Retains_The_Visual_Traversal_Error()
    {
        using Workspace workspace = new(false);
        workspace.Manager.ActiveContent = new AvaloniaObject();
        Assert.Throws<InvalidOperationException>(() => workspace.Manager.InvokeBase(Shortcut()));
        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(Key.F10, KeyModifiers.None)]
    [InlineData(Key.F10, KeyModifiers.Control)]
    [InlineData(Key.F9, KeyModifiers.Shift)]
    [InlineData(Key.Apps, KeyModifiers.None)]
    public void Other_Base_KeyDown_Calls_Do_Not_Open_The_Menu(Key key, KeyModifiers modifiers)
    {
        using Workspace workspace = new(false);
        workspace.Manager.InvokeBase(new KeyEventArgs { Key = key, KeyModifiers = modifiers });
        Assert.False(workspace.Menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Custom_Content_Control_Can_Open_The_Menu_With_No_LayoutItem(bool tool)
    {
        ManagerProbe manager = new();
        Button editor = new() { Content = "Custom editor" };
        TemplatedControl control = tool ? new LayoutAnchorableControl() : new LayoutDocumentControl();
        control.Template = new FuncControlTemplate((_, _) => new Border { Child = editor });
        ContextMenu menu = new() { Items = { new MenuItem { Header = "Application command" } } };
        if (tool) manager.AnchorableContextMenu = menu;
        else manager.DocumentContextMenu = menu;
        Window window = new() { Content = control, Width = 400, Height = 200 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            manager.ActiveContent = editor;

            manager.InvokeBase(Shortcut());

            Assert.True(menu.IsOpen);
            Assert.Null(menu.DataContext);
            Assert.Same(control, menu.PlacementTarget);
            menu.Close();
            Assert.Null(control.ContextMenu);
            Assert.Null(menu.PlacementTarget);
        }
        finally { menu.Close(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static KeyEventArgs Shortcut() => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = Key.F10,
        KeyModifiers = KeyModifiers.Shift
    };

    private sealed class ManagerProbe : DockingManager
    {
        public bool Suppress { get; set; }
        public int Calls { get; private set; }
        public Action? BeforeBase { get; set; }
        public void InvokeBase(KeyEventArgs args) => base.OnKeyDown(args);
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Calls++;
            BeforeBase?.Invoke();
            if (!Suppress) base.OnKeyDown(e);
        }
    }

    private sealed class Workspace : IDisposable
    {
        private readonly PlatformHotkeyConfiguration _hotkeys;
        private readonly List<KeyGesture> _contextMenuKeys;
        public ManagerProbe Manager { get; }
        public Window Window { get; }
        public Button Editor { get; } = new() { Content = "Editor" };
        public LayoutContent Model { get; }
        public ContextMenu Menu { get; } = new() { Items = { new MenuItem { Header = "Item command" } } };

        public Workspace(bool tool)
        {
            Model = tool ? new LayoutAnchorable() : new LayoutDocument();
            Model.Content = Editor;
            ILayoutPanelElement pane = tool ? new LayoutAnchorablePane((LayoutAnchorable)Model) : new LayoutDocumentPane(Model);
            Manager = new() { Layout = new() { RootPanel = new(pane) } };
            if (tool) Manager.AnchorableContextMenu = Menu;
            else Manager.DocumentContextMenu = Menu;
            Window = new() { Content = Manager, Width = 500, Height = 300 };
            _hotkeys = Application.Current!.PlatformSettings!.HotkeyConfiguration;
            _contextMenuKeys = _hotkeys.OpenContextMenu;
            _hotkeys.OpenContextMenu = new(_contextMenuKeys) { new(Key.F10, KeyModifiers.Shift) };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Model.IsActive = true;
        }

        public void Dispose()
        {
            Menu.Close();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
            _hotkeys.OpenContextMenu = _contextMenuKeys;
        }
    }
}
