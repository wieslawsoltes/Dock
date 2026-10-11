using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using ReactiveUI.Reactive;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class NativeCommandTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Compiled_Manager_Context_Menus_Render_The_Requested_Item_And_Command(bool tool, bool floating)
    {
        using Workspace workspace = new(new ContextMenuFixture());
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        if (floating) content.Float();
        workspace.Update();
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(content)!;
        ContextMenu menu = (tool ? workspace.Manager.AnchorableContextMenu : workspace.Manager.DocumentContextMenu)!;
        int requests = 0;
        using BehaviorSubject<bool> enabled = new(true);
        using var command = ReactiveCommand.Create<object?>(parameter =>
        {
            Assert.Null(parameter);
            requests++;
        }, enabled, ImmediateScheduler.Instance);
        if (tool) ((LayoutAnchorableItem)item).HideCommand = command;
        else item.CloseCommand = command;
        Control scope = Scope(workspace, content);
        // A single floating tool has a title bar but no tab strip.
        Control target = tool && floating ? ToolButton(scope, "PART_MenuButton") : Tab(scope, content);
        MouseButton button = tool && floating ? MouseButton.Left : MouseButton.Right;
        Click(target, button);
        Assert.True(menu.IsOpen);
        Assert.Same(item, menu.DataContext);
        MenuItem entry = Assert.IsType<MenuItem>(Assert.Single(menu.Items));
        Assert.Equal(content.Title, entry.Header);
        Assert.Same(command, entry.Command);
        content.Title = "Updated title";
        Assert.Equal(content.Title, entry.Header);
        enabled.OnNext(false);
        workspace.Update();
        Assert.False(entry.IsEffectivelyEnabled);
        enabled.OnNext(true);
        workspace.Update();
        Click(entry);
        Assert.Equal(1, requests);
        Assert.NotNull(content.Root);
        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
    }

    [AvaloniaFact]
    public void Tool_Title_Auto_Hide_And_Document_Tab_Use_The_Appropriate_Manager_Menu()
    {
        using Workspace workspace = new(new ContextMenuFixture());
        ContextMenu tools = workspace.Manager.AnchorableContextMenu!;
        ContextMenu documents = workspace.Manager.DocumentContextMenu!;
        Control title = workspace.Manager.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_Title");
        Click(title, MouseButton.Right);
        Assert.True(tools.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(workspace.Tool), tools.DataContext);
        tools.Close();
        workspace.Tool.ToggleAutoHide();
        workspace.Update();
        Control pin = workspace.Manager.GetVisualDescendants().OfType<ToolPinItemControl>()
            .Single(control => ReferenceEquals(control.DataContext, workspace.Tool.DockModel));
        Click(pin, MouseButton.Right);
        Assert.True(tools.IsOpen);
        Assert.False(documents.IsOpen);
        tools.Close();
        workspace.Tool.ToggleAutoHide();
        workspace.Tool.DockAsDocument();
        workspace.Update();
        Click(Tab(workspace.Manager, workspace.Tool), MouseButton.Right);
        Assert.True(documents.IsOpen);
        Assert.False(tools.IsOpen);
        Assert.IsType<LayoutAnchorableItem>(documents.DataContext);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Manager_Menu_Replacement_And_Explicit_Null_Are_Observed(bool tool)
    {
        using Workspace workspace = new();
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        Control tab = Tab(workspace.Manager, content);
        ContextMenu original = (tool ? workspace.Manager.AnchorableContextMenu : workspace.Manager.DocumentContextMenu)!;
        Click(tab, MouseButton.Right);
        Assert.True(original.IsOpen);
        ContextMenu replacement = new() { Items = { new MenuItem { Header = "Replacement" } } };
        if (tool) workspace.Manager.AnchorableContextMenu = replacement;
        else workspace.Manager.DocumentContextMenu = replacement;
        Assert.False(original.IsOpen);
        Assert.Null(original.DataContext);
        Assert.Null(original.PlacementTarget);
        Click(tab, MouseButton.Right);
        Assert.True(replacement.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(content), replacement.DataContext);
        if (tool) workspace.Manager.AnchorableContextMenu = null;
        else workspace.Manager.DocumentContextMenu = null;
        Assert.False(replacement.IsOpen);
        Click(tab, MouseButton.Right);
        Assert.False(original.IsOpen);
        Assert.False(replacement.IsOpen);
        workspace.Manager.ClearValue(tool ? DockingManager.AnchorableContextMenuProperty : DockingManager.DocumentContextMenuProperty);
        ContextMenu restored = (tool ? workspace.Manager.AnchorableContextMenu : workspace.Manager.DocumentContextMenu)!;
        Click(tab, MouseButton.Right);
        Assert.True(restored.IsOpen);
    }

    [AvaloniaFact]
    public void Default_Context_Menus_Are_Per_Manager_And_Null_Disables_Them_Before_Attachment()
    {
        Assert.Null(new DockingManager().DocumentContextMenu);
        Assert.Null(new DockingManager().AnchorableContextMenu);
        using Workspace first = new();
        using Workspace second = new();
        Assert.NotSame(first.Manager.DocumentContextMenu, second.Manager.DocumentContextMenu);
        Assert.NotSame(first.Manager.AnchorableContextMenu, second.Manager.AnchorableContextMenu);
        using Workspace disabled = new(new DockingManager { DocumentContextMenu = null, AnchorableContextMenu = null });
        Assert.Null(disabled.Manager.DocumentContextMenu);
        Assert.Null(disabled.Manager.AnchorableContextMenu);
        Click(Tab(disabled.Manager, disabled.Document), MouseButton.Right);
        Click(ToolButton(disabled.Manager, "PART_MenuButton"));
        Assert.Null(disabled.Manager.DocumentContextMenu);
        Assert.Null(disabled.Manager.AnchorableContextMenu);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Manager_Context_Menu_Opening_Can_Be_Canceled_Without_Losing_Future_Requests(bool tool)
    {
        using Workspace workspace = new(new ContextMenuFixture());
        ContextMenu menu = (tool ? workspace.Manager.AnchorableContextMenu : workspace.Manager.DocumentContextMenu)!;
        Control target = tool ? ToolButton(workspace.Manager, "PART_MenuButton") : Tab(workspace.Manager, workspace.Document);
        MouseButton button = tool ? MouseButton.Left : MouseButton.Right;
        CancelEventHandler cancel = (_, e) => e.Cancel = true;
        menu.Opening += cancel;
        Click(target, button);
        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
        Assert.Null(target.ContextMenu);
        menu.Opening -= cancel;
        Click(target, button);
        Assert.True(menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Keyboard_Context_Request_Uses_The_Active_Item_And_Respects_Child_Menus(bool tool)
    {
        using Workspace workspace = new(new ContextMenuFixture());
        LayoutContent content = tool ? workspace.Tool : workspace.Document;
        Button child = new() { Content = "Application control" };
        content.Content = child;
        content.IsActive = true;
        workspace.Update();
        Assert.True(child.Focus());
        OpenKeyboardMenu(workspace.Window);
        ContextMenu menu = (tool ? workspace.Manager.AnchorableContextMenu : workspace.Manager.DocumentContextMenu)!;
        Assert.True(menu.IsOpen);
        Assert.Same(workspace.Manager.GetLayoutItemFromModel(content), menu.DataContext);
        menu.Close();
        ContextMenu childMenu = new() { Items = { new MenuItem { Header = "Application action" } } };
        child.ContextMenu = childMenu;
        Assert.True(child.Focus());
        OpenKeyboardMenu(workspace.Window);
        Assert.True(childMenu.IsOpen);
        Assert.False(menu.IsOpen);
        childMenu.Close();
        childMenu.Opening += (_, e) => e.Cancel = true;
        Assert.True(child.Focus());
        OpenKeyboardMenu(workspace.Window);
        Assert.False(childMenu.IsOpen);
        Assert.False(menu.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Manager_Context_Menus_Close_And_Clear_Their_Item_When_Layout_Or_View_Is_Detached(bool replaceLayout)
    {
        using Workspace workspace = new(new ContextMenuFixture());
        ContextMenu menu = workspace.Manager.DocumentContextMenu!;
        Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
        Assert.True(menu.IsOpen);
        if (replaceLayout) workspace.Manager.Layout = new LayoutRoot();
        else workspace.Window.Content = null;
        workspace.Update();
        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
        if (!replaceLayout)
        {
            workspace.Window.Content = workspace.Manager;
            workspace.Update();
            Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
            Assert.True(menu.IsOpen);
        }
    }

    [AvaloniaFact]
    public void Replacing_A_Manager_Menu_During_Opening_Does_Not_Open_The_Obsolete_Menu()
    {
        using Workspace workspace = new(new ContextMenuFixture());
        ContextMenu original = workspace.Manager.DocumentContextMenu!;
        ContextMenu replacement = new() { Items = { new MenuItem { Header = "Replacement" } } };
        original.Opening += (_, _) => workspace.Manager.DocumentContextMenu = replacement;
        Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
        Assert.False(original.IsOpen);
        Assert.Null(original.DataContext);
        Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
        Assert.True(replacement.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData("document", false)]
    [InlineData("document", true)]
    [InlineData("tool", false)]
    [InlineData("tool", true)]
    [InlineData("floating", false)]
    [InlineData("keyboard", true)]
    [InlineData("submenu", false)]
    [InlineData("submenu", true)]
    public async Task Retained_Custom_Context_Menu_Does_Not_Retain_A_Closed_Manager(string origin, bool closeFirst)
    {
        ContextMenu retained = new() { Items = { new MenuItem { Header = "Retained menu" } } };
        WeakReference<DockingManager> reference = OpenCustomMenuAndRelease(retained, origin, closeFirst);
        await Task.Delay(Application.Current!.PlatformSettings!.HoldWaitDuration + TimeSpan.FromMilliseconds(50));
        Dispatcher.UIThread.RunJobs();
        for (int i = 0; i < 20; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            // A render commit can enqueue cleanup after the tick returns.
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (!IsManagerAlive(reference)) break;
            // Submenu disposal and canceled hold continuations can complete on
            // a later dispatcher turn under load. Keep the collection check bounded.
            await Task.Delay(10);
        }
        Assert.False(reference.TryGetTarget(out _));
        GC.KeepAlive(retained);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsManagerAlive(WeakReference<DockingManager> reference) => reference.TryGetTarget(out _);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<DockingManager> OpenCustomMenuAndRelease(ContextMenu menu, string origin, bool closeFirst)
    {
        using Workspace workspace = new(new DockingManager { DocumentContextMenu = menu, AnchorableContextMenu = menu });
        if (origin == "keyboard")
        {
            Button child = new() { Content = "Keyboard target" };
            workspace.Document.Content = child;
            workspace.Update();
            Assert.True(child.Focus());
            OpenKeyboardMenu(workspace.Window);
        }
        else if (origin is "tool" or "floating")
        {
            if (origin == "floating") workspace.Tool.Float();
            workspace.Update();
            Click(ToolButton(Scope(workspace, workspace.Tool), "PART_MenuButton"));
        }
        else
        {
            if (origin == "submenu") ((MenuItem)menu.Items[0]!).Items.Add(new MenuItem { Header = "Nested action" });
            Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
            if (origin == "submenu")
            {
                MenuItem entry = (MenuItem)menu.Items[0]!;
                Click(entry);
                Assert.True(entry.IsSubMenuOpen);
                Assert.NotNull(TopLevel.GetTopLevel((MenuItem)entry.Items[0]!));
            }
        }
        Assert.True(menu.IsOpen);
        Assert.NotNull(TopLevel.GetTopLevel(menu));
        if (closeFirst) menu.Close();
        return new(workspace.Manager);
    }

    [AvaloniaFact]
    public void Custom_Submenus_Reopen_With_Compiled_Bindings_And_Theme_Changes_Intact()
    {
        ContextMenuFixture fixture = new();
        using Workspace workspace = new(fixture);
        ContextMenu menu = (ContextMenu)fixture.Resources["NestedMenu"]!;
        workspace.Manager.DocumentContextMenu = menu;
        MenuItem parent = (MenuItem)menu.Items[0]!;
        MenuItem action = (MenuItem)parent.Items[0]!;
        LayoutItem item = workspace.Manager.GetLayoutItemFromModel(workspace.Document)!;
        int requests = 0;
        using var command = ReactiveCommand.Create(() => requests++, outputScheduler: ImmediateScheduler.Instance);
        item.FloatCommand = command;
        for (int i = 1; i <= 3; i++)
        {
            ControlTheme theme = new()
            {
                TargetType = typeof(MenuItem),
                BasedOn = (ControlTheme)workspace.Manager.FindResource(typeof(MenuItem))!,
                Setters = { new Setter(TemplatedControl.FontSizeProperty, 18d + i) }
            };
            action.Theme = theme;
            workspace.Document.Title = $"Menu pass {i}";
            Click(Tab(workspace.Manager, workspace.Document), MouseButton.Right);
            Click(parent);
            Assert.True(parent.IsSubMenuOpen);
            Assert.Equal(workspace.Document.Title, action.Header);
            Assert.Same(command, action.Command);
            Assert.Same(theme, action.Theme);
            Assert.Equal(18d + i, action.FontSize);
            Click(action);
            Assert.Equal(i, requests);
            Assert.False(menu.IsOpen);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Keyboard_Focus_Returns_After_The_Menu_Closes_Or_Opening_Is_Canceled(bool cancel)
    {
        using Workspace workspace = new(new ContextMenuFixture());
        Button child = new() { Content = "Application focus" };
        workspace.Document.Content = child;
        workspace.Update();
        ContextMenu menu = workspace.Manager.DocumentContextMenu!;
        menu.Opening += (_, e) => e.Cancel = cancel;
        Assert.True(child.Focus());
        OpenKeyboardMenu(workspace.Window);
        Assert.Equal(!cancel, menu.IsOpen);
        if (!cancel) menu.Close();
        Assert.Same(child, workspace.Window.FocusManager!.GetFocusedElement());
    }

    private static void OpenKeyboardMenu(Window window)
    {
        window.KeyPress(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, null);
        window.KeyRelease(Key.Apps, RawInputModifiers.None, PhysicalKey.ContextMenu, null);
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
