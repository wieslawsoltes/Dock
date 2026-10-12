using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using Xunit;
using DropDownButton = AvalonDock.Controls.DropDownButton;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class MenuControlTests
{
    [AvaloniaTheory]
    [InlineData("Button")]
    [InlineData("Area")]
    public void A_Dropdown_Restores_A_Bound_Existing_ContextMenu_And_Its_Latest_Value(string key)
    {
        using Workspace workspace = new(key);
        ContextMenu original = new();
        ContextMenu replacement = new();
        Window source = new() { ContextMenu = original };
        using IDisposable binding = workspace.Control.Bind(Control.ContextMenuProperty, source.GetObservable(Control.ContextMenuProperty));
        workspace.Open();
        Assert.Same(original, workspace.Control.ContextMenu);
        source.ContextMenu = replacement;
        workspace.Menu.Close();
        Assert.Same(replacement, workspace.Control.ContextMenu);
        source.ContextMenu = original;
        Assert.Same(original, workspace.Control.ContextMenu);
        source.ContextMenu = null;
    }

    [AvaloniaTheory]
    [InlineData("Button")]
    [InlineData("Area")]
    public void A_Menu_Replaced_During_Opening_Does_Not_Open_A_Discarded_Popup(string key)
    {
        using Workspace workspace = new(key);
        ContextMenu replacement = CreateMenu();
        workspace.Menu.Opening += (_, _) => SetMenu(workspace.Control, replacement);
        workspace.Open();
        Assert.False(workspace.Menu.IsOpen);
        Assert.Null(workspace.Menu.DataContext);
        Assert.Null(workspace.Menu.PlacementTarget);
        Assert.False(replacement.IsOpen);
        workspace.Open();
        Assert.True(replacement.IsOpen);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Closed_Callback_Can_Open_Another_Invocation_Without_The_Previous_Callback_Closing_It(bool sameMenu)
    {
        ContextMenu first = CreateMenu();
        ContextMenu second = sameMenu ? first : CreateMenu();
        object data = new();
        ButtonProbe button = new() { DropDownContextMenu = first, DropDownContextMenuDataContext = data };
        bool reopened = false;
        first.Closed += (_, _) =>
        {
            if (reopened) return;
            reopened = true;
            button.DropDownContextMenu = second;
            button.InvokeClick();
        };
        using MenuWindow window = new(button);
        button.InvokeClick();
        first.Close();
        Assert.True(second.IsOpen);
        Assert.Same(data, second.DataContext);
        Assert.Null(button.ContextMenu);
        Assert.True(button.IsChecked);
        second.Close();
        Assert.Null(button.ContextMenu);
        Assert.Null(second.DataContext);
    }

    [AvaloniaFact]
    public void A_Failed_Opening_Releases_The_Invocation_And_Can_Be_Retried()
    {
        ContextMenu menu = CreateMenu();
        ButtonProbe button = new() { DropDownContextMenu = menu, DropDownContextMenuDataContext = new object() };
        CancelEventHandler failure = (_, _) => throw new InvalidOperationException("Opening failure");
        menu.Opening += failure;
        using MenuWindow window = new(button);
        Assert.Throws<InvalidOperationException>(button.InvokeClick);
        Assert.False(menu.IsOpen);
        Assert.Null(menu.DataContext);
        Assert.Null(menu.PlacementTarget);
        Assert.Null(button.ContextMenu);
        menu.Opening -= failure;
        button.InvokeClick();
        Assert.True(menu.IsOpen);
        Assert.True(button.IsChecked);
    }

    [AvaloniaFact]
    public void Button_Overrides_Observe_Menu_Changes_And_No_Menu_Retains_Normal_Toggle_Behavior()
    {
        ButtonProbe button = new();
        button.InvokeClick();
        Assert.True(button.IsChecked);
        button.InvokeClick();
        Assert.False(button.IsChecked);
        button.DropDownContextMenu = CreateMenu();
        Assert.Equal(1, button.MenuChanges);
        using MenuWindow window = new(button);
        button.IsEnabled = false;
        Click(button);
        Assert.False(button.DropDownContextMenu.IsOpen);
        Assert.False(button.IsChecked);
        button.IsEnabled = true;
        button.Focus();
        window.Window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        window.Window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, " ");
        Assert.True(button.DropDownContextMenu.IsOpen);
        Assert.True(button.IsChecked);
    }

    [AvaloniaFact]
    public void Icon_Hooks_And_Throwing_Selectors_Do_Not_Leave_The_Icon_Update_Guard_Armed()
    {
        MenuControlFixture resources = new();
        IDataTemplate first = (IDataTemplate)resources.Resources["Icon"]!;
        IDataTemplate second = (IDataTemplate)resources.Resources["AlternateIcon"]!;
        IconHookProbe item = new();
        item.IconTemplate = first;
        Assert.Equal(1, item.Templates);
        Assert.IsType<TextBlock>(item.Icon);
        ThrowingIconSelector selector = new() { Throw = true, Template = second };
        Assert.Throws<InvalidOperationException>(() => item.IconTemplateSelector = selector);
        Assert.Equal(1, item.Selectors);
        selector.Throw = false;
        item.Icon = "Retry";
        Assert.Equal("Retry", selector.LastItem);
        Assert.Contains("alternate-icon", Assert.IsType<TextBlock>(item.Icon).Classes);
        Assert.Same(item, selector.LastContainer);
        item.Suppress = true;
        object previous = item.Icon!;
        item.IconTemplate = null;
        Assert.Equal(2, item.Templates);
        Assert.Same(previous, item.Icon);
    }

    [AvaloniaTheory]
    [InlineData("LegacyButton", false)]
    [InlineData("LegacyButton", true)]
    [InlineData("Area", false)]
    [InlineData("Area", true)]
    public async Task A_Retained_Menu_Does_Not_Retain_An_Unused_Or_Closed_Control_And_Manager(string key, bool open)
    {
        var (menu, control, manager) = CreateRetainedMenu(key, open);
        // Avalonia's canceled hold timer retains the last press until it fires.
        // Wait on the same dispatcher timer queue: a thread-pool delay can finish
        // before due UI timers have been promoted on a busy test runner.
        TaskCompletionSource drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable timer = DispatcherTimer.RunOnce(() => drained.SetResult(),
            Application.Current!.PlatformSettings!.HoldWaitDuration + TimeSpan.FromMilliseconds(50));
        await drained.Task;
        for (int index = 0; index < 3; index++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        Assert.False(control.TryGetTarget(out _));
        Assert.False(manager.TryGetTarget(out _));
        GC.KeepAlive(menu);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ContextMenu, WeakReference<Control>, WeakReference<DockingManager>) CreateRetainedMenu(string key, bool open)
    {
        using Workspace workspace = new(key);
        // This template has no captured XAML root. An application-owned
        // compiled template can intentionally keep its declaring view alive.
        ((MenuItemEx)workspace.Menu.Items[0]!).IconTemplate =
            new FuncDataTemplate<object>(static (_, _) => new Border());
        workspace.Menu.Items.Add(new MenuItemEx { Header = new Border() });
        if (open) workspace.Open();
        workspace.Menu.Close();
        return (workspace.Menu, new(workspace.Control), new(workspace.Manager));
    }

    [AvaloniaFact]
    public void Closing_Preserves_The_Application_Icon_Template_And_Its_Resource_Scope()
    {
        using Workspace workspace = new("Button");
        MenuItemEx item = (MenuItemEx)workspace.Menu.Items[0]!;
        IDataTemplate template = item.IconTemplate!;
        workspace.Open();
        workspace.Menu.Close();
        Assert.Same(template, item.IconTemplate);
        workspace.Document.Title = "Reopened icon";
        workspace.Open();
        Assert.Same(template, item.IconTemplate);
        Assert.Equal("Reopened icon", Assert.IsType<TextBlock>(item.Icon).Text);
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Reopening_Preserves_Application_Content_And_Its_Descendant_Bindings(bool icon)
    {
        using Workspace workspace = new("LegacyButton");
        MenuItemEx item = new();
        workspace.Menu.Items.Clear();
        workspace.Menu.Items.Add(item);
        Border content = (Border)((IDataTemplate)workspace.Window.Resources["NestedContent"]!).Build(null)!;
        TextBlock text = (TextBlock)content.Child!;
        if (icon) item.Icon = content;
        else item.Header = content;
        for (int index = 0; index < 3; index++)
        {
            workspace.Document.Title = $"Title {index}";
            workspace.Open();
            Assert.Same(content, icon ? item.Icon : item.Header);
            Assert.Same(text, content.Child);
            Assert.Same(workspace.Item, text.DataContext);
            Assert.Equal(workspace.Document.Title, text.Text);
            workspace.Menu.Close();
        }
    }

    private static ContextMenu CreateMenu()
    {
        ContextMenuEx menu = new();
        menu.Items.Add(new MenuItemEx { Header = "Item" });
        return menu;
    }
    private sealed class ButtonProbe : DropDownButton
    {
        public int MenuChanges { get; private set; }
        public void InvokeClick() => OnClick();
        protected override void OnDropDownContextMenuChanged(AvaloniaPropertyChangedEventArgs e)
        {
            MenuChanges++;
            base.OnDropDownContextMenuChanged(e);
        }
    }
    private sealed class IconHookProbe : MenuItemEx
    {
        public int Templates { get; private set; }
        public int Selectors { get; private set; }
        public bool Suppress { get; set; }
        protected override void OnIconTemplateChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Templates++;
            if (!Suppress) base.OnIconTemplateChanged(e);
        }
        protected override void OnIconTemplateSelectorChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Selectors++;
            if (!Suppress) base.OnIconTemplateSelectorChanged(e);
        }
    }
    private sealed class ThrowingIconSelector : DataTemplateSelector
    {
        public bool Throw { get; set; }
        public IDataTemplate? Template { get; set; }
        public object? LastItem { get; private set; }
        public AvaloniaObject? LastContainer { get; private set; }
        public override IDataTemplate? SelectTemplate(object? item, AvaloniaObject container)
        {
            LastItem = item;
            LastContainer = container;
            if (Throw) throw new InvalidOperationException("Selector failure");
            return Template;
        }
    }
    private sealed class MenuWindow : IDisposable
    {
        public Window Window { get; }
        public MenuWindow(Control control)
        {
            Window = new() { Content = control, Width = 400, Height = 300 };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
        }
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
