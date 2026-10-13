using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class PaneInputContractTests
{
    [AvaloniaTheory]
    [InlineData(false, "owner", false)]
    [InlineData(false, "owner", true)]
    [InlineData(false, "standalone", false)]
    [InlineData(false, "foreign", false)]
    [InlineData(false, "nested", false)]
    [InlineData(true, "owner", false)]
    [InlineData(true, "owner", true)]
    [InlineData(true, "standalone", false)]
    [InlineData(true, "foreign", false)]
    [InlineData(true, "nested", false)]
    public void Pane_Pointer_Activation_Requires_The_Source_Visuals_Nearest_Manager_To_Own_The_Model(
        bool tool, string host, bool handled)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutElement pane = tool
            ? new LayoutAnchorablePane((LayoutAnchorable)content)
            : new LayoutDocumentPane((LayoutDocument)content);
        DockingManager manager = new() { Layout = new() { RootPanel = new((ILayoutPanelElement)pane) } };
        Window window = new() { Width = 320, Height = 240, Content = manager };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            LayoutCachePaneControl control = manager.GetVisualDescendants().OfType<LayoutCachePaneControl>()
                .Single(view => ReferenceEquals(((ILayoutControl)view).Model, pane));
            Present(manager, new Border());
            window.UpdateLayout();
            Assert.Null(control.GetVisualParent());
            control.Template = new FuncControlTemplate<LayoutCachePaneControl>((_, _) =>
            {
                Border surface = new() { Background = Brushes.Transparent };
                if (handled) surface.PointerPressed += (_, args) => args.Handled = true;
                return surface;
            });
            DockingManager foreign = new();
            window.Content = host switch
            {
                "owner" => Present(manager, control),
                "foreign" => Present(foreign, control),
                "nested" => Present(manager, Present(foreign, control)),
                _ => control
            };
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Same(content, ((ILayoutContentSelector)pane).SelectedContent);
            foreach (MouseButton button in new[] { MouseButton.Left, MouseButton.Right })
            {
                manager.Layout.ActiveContent = null;
                window.MouseDown(new Point(10, 10), button);
                window.MouseUp(new Point(10, 10), button);
                Assert.Equal(host == "owner" && !handled, content.IsActive);
                Assert.Same(host == "owner" && !handled ? content : null, manager.Layout.ActiveContent);
            }
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Tool_Focus_Hook_Activates_The_Selected_Model_Without_Pointer_Ownership_Or_Control_Enablement_Checks(
        bool rooted, bool disabledControl)
    {
        LayoutAnchorable content = new();
        LayoutAnchorablePane pane = new(content);
        DockingManager? manager = rooted ? new() { Layout = new() { RootPanel = new(pane) } } : null;
        ToolProbe control = new(pane) { IsEnabled = !disabledControl };
        pane.SelectedContentIndex = 0;
        if (manager is not null) manager.Layout.ActiveContent = null;
        else content.IsActive = false;
        Assert.Same(content, pane.SelectedContent);

        // A derived control can forward focus arriving from a nested manager.
        // This protected hook operates on selection and does not inspect Source.
        control.FocusHook(new FocusChangedEventArgs(InputElement.GotFocusEvent) { Source = new DockingManager() });

        Assert.True(content.IsActive);
        if (manager is not null) Assert.Same(content, manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Empty_Tool_Panes_Ignore_Focus_Without_Changing_The_Managers_Active_Content()
    {
        LayoutDocument active = new();
        LayoutAnchorablePane pane = new();
        DockingManager manager = new()
        {
            Layout = new() { RootPanel = new() { Children = { new LayoutDocumentPane(active), pane } } }
        };
        active.IsActive = true;

        new ToolProbe(pane).FocusHook(new FocusChangedEventArgs(InputElement.GotFocusEvent));

        Assert.Same(active, manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData("owner", false)]
    [InlineData("owner", true)]
    [InlineData("standalone", false)]
    [InlineData("standalone", true)]
    [InlineData("foreign", false)]
    [InlineData("foreign", true)]
    [InlineData("nested", false)]
    [InlineData("nested", true)]
    [InlineData("manager-source", false)]
    [InlineData("manager-source", true)]
    [InlineData("nonvisual", false)]
    [InlineData("nonvisual", true)]
    [InlineData("null", false)]
    [InlineData("null", true)]
    [InlineData("unrooted", false)]
    [InlineData("unrooted", true)]
    public void Document_Preview_Hooks_Check_Visual_Ownership_And_Run_Even_For_Handled_Events(string host, bool handled)
    {
        LayoutDocument model = new();
        LayoutDocumentPane pane = new(model);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        Border source = new() { Background = Brushes.Transparent };
        DocumentProbe control = new() { Model = model };
        // The base model hook requires a manager. Detach after assignment to
        // test how the pointer hook handles a model that subsequently loses it.
        if (host == "unrooted")
        {
            pane.Children.Remove(model);
            Assert.Null(model.Root);
            Assert.Same(model, control.Model);
        }
        control.Template = new FuncControlTemplate<LayoutDocumentControl>((_, _) => source);
        DockingManager foreign = new();
        Control view = host switch
        {
            "standalone" => control,
            "foreign" => Present(foreign, control),
            "nested" => Present(manager, Present(foreign, control)),
            _ => Present(manager, control)
        };
        Window window = new() { Width = 320, Height = 240, Content = view };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Pointer pointer = new(1, PointerType.Mouse, true);
            foreach (bool right in new[] { false, true })
            {
                manager.Layout.ActiveContent = null;
                model.IsActive = false;
                PointerPressedEventArgs args = new(source, pointer, window, default, 0,
                    new PointerPointProperties(right ? RawInputModifiers.RightMouseButton : RawInputModifiers.LeftMouseButton,
                        right ? PointerUpdateKind.RightButtonPressed : PointerUpdateKind.LeftButtonPressed), KeyModifiers.None, 1)
                {
                    Source = host switch { "manager-source" => manager, "nonvisual" => new object(), "null" => null, _ => source },
                    Handled = handled
                };
                control.PreviewPointer(args, right);
                Assert.Equal(host == "owner", model.IsActive);
                Assert.Equal(handled, args.Handled);
            }
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Document_Pointer_Hooks_Read_The_Current_Model_When_The_Model_Change_Hook_Is_Suppressed(bool clearModel, bool handled)
    {
        LayoutDocument original = new() { Content = new object() };
        LayoutDocument replacement = new() { Content = new object() };
        DockingManager manager = new()
        {
            Layout = new() { RootPanel = new(new LayoutDocumentPane { Children = { original, replacement } }) }
        };
        Border source = new() { Background = Brushes.Transparent };
        DocumentProbe control = new() { Model = original };
        control.SuppressModelHook = true;
        control.Model = clearModel ? null : replacement;
        control.Template = new FuncControlTemplate<LayoutDocumentControl>((_, _) => source);
        Window window = new() { Width = 320, Height = 240, Content = Present(manager, control) };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Pointer pointer = new(1, PointerType.Mouse, true);
            foreach (bool right in new[] { false, true })
            {
                manager.Layout.ActiveContent = null;
                Assert.False(original.IsActive);
                Assert.False(replacement.IsActive);
                Assert.Same(clearModel ? null : replacement, control.Model);
                PointerPressedEventArgs args = new(source, pointer, window, default, 0,
                    new PointerPointProperties(right ? RawInputModifiers.RightMouseButton : RawInputModifiers.LeftMouseButton,
                        right ? PointerUpdateKind.RightButtonPressed : PointerUpdateKind.LeftButtonPressed), KeyModifiers.None, 1)
                { Handled = handled };

                control.PreviewPointer(args, right);

                Assert.False(original.IsActive);
                Assert.Equal(!clearModel, replacement.IsActive);
                Assert.Same(clearModel ? null : replacement, manager.Layout.ActiveContent);
            }
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static DockingManager Present(DockingManager manager, Control child)
    {
        manager.Template = new FuncControlTemplate<DockingManager>((_, _) => child);
        return manager;
    }

    private sealed class ToolProbe(LayoutAnchorablePane model) : LayoutAnchorablePaneControl(model)
    {
        internal void FocusHook(FocusChangedEventArgs args) => base.OnGotKeyboardFocus(args);
    }

    private sealed class DocumentProbe : LayoutDocumentControl
    {
        internal bool SuppressModelHook { get; set; }

        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (!SuppressModelHook) base.OnModelChanged(args);
        }

        internal void PreviewPointer(PointerPressedEventArgs args, bool right)
        {
            if (right) base.OnPreviewMouseRightButtonDown(args);
            else base.OnPreviewMouseLeftButtonDown(args);
        }
    }
}
