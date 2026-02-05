using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ThemeCommandInteractionLeakTests
{
    [ReleaseFact]
    public void DockCommandBarHost_FluentTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunDockCommandBarHostTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void DockCommandBarHost_SimpleTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunDockCommandBarHostTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void DocumentControl_FluentTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunDocumentControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void DocumentControl_SimpleTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunDocumentControlTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolTabStrip_FluentTheme_MenuItemClicks_DoNotLeak() =>
        RunToolTabStripTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolTabStrip_SimpleTheme_MenuItemClicks_DoNotLeak() =>
        RunToolTabStripTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolChromeControl_FluentTheme_ButtonClicks_DoNotLeak() =>
        RunToolChromeControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolChromeControl_SimpleTheme_ButtonClicks_DoNotLeak() =>
        RunToolChromeControlTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolPinnedControl_FluentTheme_MenuItemClicks_DoNotLeak() =>
        RunToolPinnedControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolPinnedControl_SimpleTheme_MenuItemClicks_DoNotLeak() =>
        RunToolPinnedControlTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolPinItemControl_FluentTheme_ButtonClick_DoNotLeak() =>
        RunToolPinItemControlButtonTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolPinItemControl_SimpleTheme_ButtonClick_DoNotLeak() =>
        RunToolPinItemControlButtonTest(DockThemeKind.Simple);

    private static void RunDockCommandBarHostTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var command = new NoOpCommand();
            var openItem = new MenuItem { Header = "Open", Command = command };
            var exitItem = new MenuItem { Header = "Exit", Command = command };
            var rootItem = new MenuItem
            {
                Header = "File",
                ItemsSource = new object[]
                {
                    openItem,
                    new Separator(),
                    exitItem
                }
            };

            var menu = new Menu
            {
                ItemsSource = new object[] { rootItem }
            };

            var toolButton = new Button { Content = "Tool", Command = command };
            var toolBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            toolBar.Children.Add(toolButton);

            var ribbonButton = new Button { Content = "Ribbon", Command = command };
            var ribbonBar = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal };
            ribbonBar.Children.Add(ribbonButton);

            var host = new DockCommandBarHost
            {
                MenuBars = new Control[] { menu },
                ToolBars = new Control[] { toolBar },
                RibbonBars = new Control[] { ribbonBar },
                IsVisible = true
            };

            var window = CreateThemedWindow(host, theme);
            ShowWindow(window);

            InvokeMenuItemClick(openItem);
            InvokeMenuItemClick(exitItem);
            InvokeButtonClick(toolButton);
            InvokeButtonClick(ribbonButton);
            DrainDispatcher();

            var result = new CommandBarLeakResult(
                new WeakReference(host),
                new WeakReference(openItem),
                new WeakReference(exitItem),
                command);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            host = null;
            menu = null;
            toolBar = null;
            ribbonBar = null;
            rootItem = null;
            openItem = null;
            exitItem = null;
            toolButton = null;
            ribbonButton = null;

            return result;
        });

        AssertCollected(result.HostRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.OpenItemRef, result.ExitItemRef);
        }
        GC.KeepAlive(result.CommandKeepAlive);
    }

    private static void RunDocumentControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Root.ActiveDockable = context.DocumentDock;
            context.Root.DefaultDockable = context.DocumentDock;
            context.Document.CanClose = true;
            context.DocumentDock.CanCloseLastDockable = true;
            context.DocumentDock.CanCreateDocument = true;
            context.DocumentDock.CreateDocument = new NoOpCommand();

            var documentControl = new DocumentControl
            {
                DataContext = context.DocumentDock
            };

            var window = CreateThemedWindow(documentControl, theme);
            ShowWindow(window);

            documentControl.ApplyTemplate();
            documentControl.UpdateLayout();
            DrainDispatcher();

            var tabStrip = FindVisualDescendant<DocumentTabStrip>(documentControl);
            Assert.NotNull(tabStrip);
            tabStrip!.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var createButton = FindVisualDescendant<Button>(
                tabStrip,
                button => string.Equals(button.Name, "PART_ButtonCreate", StringComparison.Ordinal));
            Assert.NotNull(createButton);
            ApplyNoOpCommand(createButton);
            InvokeButtonClick(createButton);

            var tabItem = tabStrip.ContainerFromIndex(0) as DocumentTabStripItem
                          ?? FindVisualDescendant<DocumentTabStripItem>(documentControl);
            Assert.NotNull(tabItem);

            var menuItem = new MenuItem { Header = "Test", Command = new NoOpCommand() };
            var menu = new ContextMenu { ItemsSource = new object[] { menuItem } };
            tabItem!.DocumentContextMenu = menu;
            OpenAndCloseContextMenu(tabItem, menu, () => InvokeMenuItemClick(menuItem));

            var closeButton = FindVisualDescendant<Button>(tabItem);
            Assert.NotNull(closeButton);
            ApplyNoOpCommand(closeButton);
            InvokeButtonClick(closeButton);
            DrainDispatcher();

            tabItem!.DocumentContextMenu = null;
            tabStrip.ItemsSource = null;
            documentControl.DataContext = null;

            var result = new DocumentControlLeakResult(
                new WeakReference(documentControl),
                new WeakReference(tabItem),
                new WeakReference(createButton),
                new WeakReference(closeButton),
                new WeakReference(menuItem),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            documentControl = null;
            tabItem = null;
            createButton = null;
            closeButton = null;
            menuItem = null;
            menu = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.TabItemRef, result.CreateButtonRef, result.CloseButtonRef, result.MenuItemRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunToolTabStripTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Root.ActiveDockable = context.ToolDock;
            context.Root.DefaultDockable = context.ToolDock;
            var secondaryTool = (Dock.Model.Avalonia.Controls.Tool)context.Factory.CreateTool();
            secondaryTool.Factory = context.Factory;
            secondaryTool.Owner = context.ToolDock;
            var visibleDockables = context.ToolDock.VisibleDockables
                                   ?? throw new InvalidOperationException("ToolDock.VisibleDockables was null.");
            visibleDockables.Add(secondaryTool);

            var tabStrip = new ToolTabStrip
            {
                DataContext = context.ToolDock,
                ItemsSource = visibleDockables
            };

            var window = CreateThemedWindow(tabStrip, theme);
            ShowWindow(window);

            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var tabItem = tabStrip.ContainerFromIndex(0) as ToolTabStripItem
                          ?? FindVisualDescendant<ToolTabStripItem>(tabStrip);
            Assert.NotNull(tabItem);
            tabItem = tabItem ?? throw new InvalidOperationException("ToolTabStrip item was not found.");

            var menuItem = new MenuItem { Header = "Test", Command = new NoOpCommand() };
            var menu = new ContextMenu { ItemsSource = new object[] { menuItem } };
            tabItem.TabContextMenu = menu;
            OpenAndCloseContextMenu(tabItem, menu, () => InvokeMenuItemClick(menuItem));
            DrainDispatcher();

            tabItem.TabContextMenu = null;
            tabStrip.ItemsSource = null;
            tabStrip.DataContext = null;

            var result = new ToolTabStripLeakResult(
                new WeakReference(tabStrip),
                new WeakReference(tabItem),
                new WeakReference(menuItem),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            tabStrip = null;
            tabItem = null;
            menuItem = null;
            menu = null;
            secondaryTool = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.TabItemRef, result.MenuItemRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunToolChromeControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var chrome = new ToolChromeControl
            {
                DataContext = context.ToolDock
            };

            var window = CreateThemedWindow(chrome, theme);
            ShowWindow(window);

            chrome.ApplyTemplate();
            chrome.UpdateLayout();
            DrainDispatcher();

            var menuButton = FindTemplateChild<Button>(chrome, "PART_MenuButton");
            var pinButton = FindTemplateChild<Button>(chrome, "PART_PinButton");
            var maximizeButton = FindTemplateChild<Button>(chrome, "PART_MaximizeRestoreButton");
            var closeButton = FindTemplateChild<Button>(chrome, "PART_CloseButton");

            ApplyNoOpCommand(menuButton);
            ApplyNoOpCommand(pinButton);
            ApplyNoOpCommand(maximizeButton);
            ApplyNoOpCommand(closeButton);

            InvokeButtonClick(menuButton);
            InvokeButtonClick(pinButton);
            InvokeButtonClick(maximizeButton);
            InvokeButtonClick(closeButton);
            DrainDispatcher();

            var result = new ToolChromeLeakResult(
                new WeakReference(chrome),
                new WeakReference(menuButton),
                new WeakReference(pinButton),
                new WeakReference(maximizeButton),
                new WeakReference(closeButton),
                context.ToolDock);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            chrome = null;
            menuButton = null;
            pinButton = null;
            maximizeButton = null;
            closeButton = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuButtonRef, result.PinButtonRef, result.MaximizeButtonRef, result.CloseButtonRef);
        }
        GC.KeepAlive(result.DockKeepAlive);
    }

    private static void RunToolPinnedControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var items = context.Factory.CreateList<IDockable>(context.Tool);

            var pinnedControl = new ToolPinnedControl
            {
                ItemsSource = items
            };

            var window = CreateThemedWindow(pinnedControl, theme);
            ShowWindow(window);

            pinnedControl.ApplyTemplate();
            pinnedControl.UpdateLayout();
            DrainDispatcher();

            var pinItem = pinnedControl.ContainerFromIndex(0) as ToolPinItemControl
                          ?? FindVisualDescendant<ToolPinItemControl>(pinnedControl);
            Assert.NotNull(pinItem);

            var menuItem = new MenuItem { Header = "Test", Command = new NoOpCommand() };
            var menu = new ContextMenu { ItemsSource = new object[] { menuItem } };
            pinItem!.PinContextMenu = menu;
            OpenAndCloseContextMenu(pinItem, menu, () => InvokeMenuItemClick(menuItem));

            var result = new ToolPinnedLeakResult(
                new WeakReference(pinnedControl),
                new WeakReference(pinItem),
                new WeakReference(menuItem),
                items);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            pinnedControl = null;
            pinItem = null;
            menuItem = null;
            menu = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.PinItemRef, result.MenuItemRef);
        }
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    private static void RunToolPinItemControlButtonTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var items = context.Factory.CreateList<IDockable>(context.Tool);

            var pinnedControl = new ToolPinnedControl
            {
                ItemsSource = items
            };

            var window = CreateThemedWindow(pinnedControl, theme);
            ShowWindow(window);

            pinnedControl.ApplyTemplate();
            pinnedControl.UpdateLayout();
            DrainDispatcher();

            var pinItem = pinnedControl.ContainerFromIndex(0) as ToolPinItemControl
                          ?? FindVisualDescendant<ToolPinItemControl>(pinnedControl);
            Assert.NotNull(pinItem);

            var button = FindVisualDescendant<Button>(pinItem!);
            Assert.NotNull(button);
            InvokeButtonClick(button);
            DrainDispatcher();

            var result = new ToolPinItemButtonLeakResult(
                new WeakReference(pinnedControl),
                new WeakReference(pinItem),
                new WeakReference(button!),
                items);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            pinnedControl = null;
            pinItem = null;
            button = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.PinItemRef, result.ButtonRef);
        }
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    private static Window CreateThemedWindow(Control content, DockThemeKind theme)
    {
        var window = new Window { Content = content };
        ApplyTheme(window, theme);
        return window;
    }

    private static void ApplyTheme(Window window, DockThemeKind theme)
    {
        switch (theme)
        {
            case DockThemeKind.Fluent:
                window.Styles.Add(new FluentTheme());
                window.Styles.Add(new DockFluentTheme());
                break;
            case DockThemeKind.Simple:
                window.Styles.Add(new DockSimpleTheme());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(theme), theme, "Unknown theme.");
        }
    }

    private enum DockThemeKind
    {
        Fluent,
        Simple
    }

    private sealed record CommandBarLeakResult(
        WeakReference HostRef,
        WeakReference OpenItemRef,
        WeakReference ExitItemRef,
        object CommandKeepAlive);

    private sealed record DocumentControlLeakResult(
        WeakReference ControlRef,
        WeakReference TabItemRef,
        WeakReference CreateButtonRef,
        WeakReference CloseButtonRef,
        WeakReference MenuItemRef,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);

    private sealed record ToolTabStripLeakResult(
        WeakReference ControlRef,
        WeakReference TabItemRef,
        WeakReference MenuItemRef,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);

    private sealed record ToolChromeLeakResult(
        WeakReference ControlRef,
        WeakReference MenuButtonRef,
        WeakReference PinButtonRef,
        WeakReference MaximizeButtonRef,
        WeakReference CloseButtonRef,
        IDock DockKeepAlive);

    private sealed record ToolPinnedLeakResult(
        WeakReference ControlRef,
        WeakReference PinItemRef,
        WeakReference MenuItemRef,
        System.Collections.Generic.IList<IDockable> ItemsKeepAlive);

    private sealed record ToolPinItemButtonLeakResult(
        WeakReference ControlRef,
        WeakReference PinItemRef,
        WeakReference ButtonRef,
        System.Collections.Generic.IList<IDockable> ItemsKeepAlive);
}
