using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ManagerKeyDownProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tool in new[] { false, true })
        {
            foreach (bool useVisual in new[] { false, true })
                Check($"base hook tool={tool}, visual={useVisual}", () =>
                {
                    using Workspace workspace = new(tool);
                    if (useVisual) workspace.Manager.ActiveContent = workspace.Editor;
                    KeyEventArgs args = Shortcut();
                    workspace.Manager.InvokeBase(args);
                    Require(workspace.Menu.IsOpen && !args.Handled);
                    Require(ReferenceEquals(workspace.Menu.DataContext, workspace.Manager.GetLayoutItemFromModel(workspace.Model)));
                    Require(tool ? workspace.Menu.PlacementTarget is LayoutAnchorableControl : workspace.Menu.PlacementTarget is LayoutDocumentControl);
                    Require(workspace.Menu.Parent is Popup { Placement: PlacementMode.AnchorAndGravity, PlacementRect: { } rect } && rect == default);
                });
            Check($"key press and release tool={tool}", () =>
            {
                using Workspace workspace = new(tool);
                Require(workspace.Editor.Focus());
                workspace.Press();
                Require(workspace.Menu.IsOpen);
                workspace.Release();
                Require(workspace.Menu.IsOpen);
            });
            Check($"canceled opening tool={tool}", () =>
            {
                using Workspace workspace = new(tool);
                int attempts = 0;
                workspace.Menu.Opening += (_, args) => { attempts++; args.Cancel = true; };
                Require(workspace.Editor.Focus());
                workspace.Press();
                Require(attempts == 1 && !workspace.Menu.IsOpen);
                workspace.Release();
                Require(attempts == 1 && !workspace.Menu.IsOpen);
            });
            Check($"suppressed base tool={tool}", () =>
            {
                using Workspace workspace = new(tool);
                workspace.Manager.Suppress = true;
                Require(workspace.Editor.Focus());
                workspace.Press();
                workspace.Release();
                Require(!workspace.Menu.IsOpen);
            });
            Check($"custom content without item tool={tool}", () => VerifyCustomContent(tool));
        }
        Check("nonvisual active object", () =>
        {
            using Workspace workspace = new(false);
            workspace.Manager.ActiveContent = new AvaloniaObject();
            bool threw = false;
            try { workspace.Manager.InvokeBase(Shortcut()); }
            catch (InvalidOperationException) { threw = true; }
            Require(threw && !workspace.Menu.IsOpen);
        });
        if (failures.Count != 0)
            throw new InvalidOperationException($"Packaged manager key-down: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Manager key-down: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static void VerifyCustomContent(bool tool)
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
            Require(menu.IsOpen && menu.DataContext is null && ReferenceEquals(control, menu.PlacementTarget));
            menu.Close();
            Require(control.ContextMenu is null && menu.PlacementTarget is null);
        }
        finally { menu.Close(); window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static KeyEventArgs Shortcut() => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = Key.F10,
        KeyModifiers = KeyModifiers.Shift
    };

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Manager key-down contract changed.");
    }

    private sealed class ManagerProbe : DockingManager
    {
        internal bool Suppress { get; set; }
        internal void InvokeBase(KeyEventArgs args) => base.OnKeyDown(args);
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!Suppress) base.OnKeyDown(e);
        }
    }

    private sealed class Workspace : IDisposable
    {
        private readonly PlatformHotkeyConfiguration _hotkeys;
        private readonly List<KeyGesture> _contextMenuKeys;
        internal ManagerProbe Manager { get; }
        internal Window Window { get; }
        internal Button Editor { get; } = new() { Content = "Editor" };
        internal LayoutContent Model { get; }
        internal ContextMenu Menu { get; } = new() { Items = { new MenuItem { Header = "Item command" } } };

        internal Workspace(bool tool)
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

        internal void Press() => Window.KeyPress(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);
        internal void Release() => Window.KeyRelease(Key.F10, RawInputModifiers.Shift, PhysicalKey.F10, null);

        public void Dispose()
        {
            Menu.Close();
            Window.Close();
            Dispatcher.UIThread.RunJobs();
            _hotkeys.OpenContextMenu = _contextMenuKeys;
        }
    }
}
