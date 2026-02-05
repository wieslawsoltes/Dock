using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
public class ThemeMenuInteractionLeakTests
{
    [ReleaseFact]
    public void DocumentTabStripItem_FluentTheme_ContextMenuItems_DoNotLeak() =>
        RunDocumentTabStripMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void DocumentTabStripItem_SimpleTheme_ContextMenuItems_DoNotLeak() =>
        RunDocumentTabStripMenuTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolTabStripItem_FluentTheme_ContextMenuItems_DoNotLeak() =>
        RunToolTabStripMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolTabStripItem_SimpleTheme_ContextMenuItems_DoNotLeak() =>
        RunToolTabStripMenuTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolPinItemControl_FluentTheme_ContextMenuItems_DoNotLeak() =>
        RunToolPinItemMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolPinItemControl_SimpleTheme_ContextMenuItems_DoNotLeak() =>
        RunToolPinItemMenuTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ToolChromeControl_FluentTheme_FlyoutMenuItems_DoNotLeak() =>
        RunToolChromeMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ToolChromeControl_SimpleTheme_FlyoutMenuItems_DoNotLeak() =>
        RunToolChromeMenuTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void MdiDocumentWindow_FluentTheme_ContextMenuItems_DoNotLeak() =>
        RunMdiDocumentWindowMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void MdiDocumentWindow_SimpleTheme_ContextMenuItems_DoNotLeak() =>
        RunMdiDocumentWindowMenuTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void MdiDocumentWindow_ToolWindow_FluentTheme_ToolMenuItems_DoNotLeak() =>
        RunMdiToolWindowMenuTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void MdiDocumentWindow_ToolWindow_SimpleTheme_ToolMenuItems_DoNotLeak() =>
        RunMdiToolWindowMenuTest(DockThemeKind.Simple);

    private static void RunDocumentTabStripMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            context.Root.ActiveDockable = context.DocumentDock;
            context.Root.DefaultDockable = context.DocumentDock;
            context.DocumentDock.CanCloseLastDockable = true;
            context.DocumentDock.CanDrag = true;
            context.DocumentDock.CanDrop = true;
            context.DocumentDock.CreateDocument = new NoOpCommand();
            context.Document.CanClose = true;
            context.Document.CanFloat = true;
            context.Document.CanDockAsDocument = true;

            var extra = (Dock.Model.Avalonia.Controls.Document)context.Factory.CreateDocument();
            extra.Factory = context.Factory;
            extra.Owner = context.DocumentDock;
            extra.CanClose = true;
            extra.CanFloat = true;
            extra.CanDockAsDocument = true;
            context.DocumentDock.VisibleDockables?.Add(extra);

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

            var tabItem = tabStrip.ContainerFromIndex(0) as DocumentTabStripItem
                          ?? FindVisualDescendant<DocumentTabStripItem>(documentControl);
            Assert.NotNull(tabItem);

            var menu = tabItem!.DocumentContextMenu;
            Assert.NotNull(menu);

            var menuItems = CollectMenuItems(menu!);

            OpenAndCloseContextMenu(tabItem, menu, () =>
            {
                foreach (var item in menuItems)
                {
                    InvokeMenuItemClick(item);
                }
            });

            tabItem.DocumentContextMenu = null;
            tabStrip.ItemsSource = null;
            documentControl.DataContext = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(documentControl),
                new WeakReference(tabItem),
                menuItems.Select(item => new WeakReference(item)).ToArray(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            documentControl = null;
            tabItem = null;
            menuItems = null;
            extra = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuItemRefs);
            AssertCollected(result.ItemRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunToolTabStripMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            context.Root.ActiveDockable = context.ToolDock;
            context.Root.DefaultDockable = context.ToolDock;
            context.ToolDock.CanCloseLastDockable = true;
            context.Tool.CanClose = true;
            context.Tool.CanFloat = true;
            context.Tool.CanPin = true;
            context.Tool.CanDockAsDocument = true;

            var extra = (Dock.Model.Avalonia.Controls.Tool)context.Factory.CreateTool();
            extra.Factory = context.Factory;
            extra.Owner = context.ToolDock;
            extra.CanClose = true;
            extra.CanFloat = true;
            extra.CanPin = true;
            extra.CanDockAsDocument = true;
            context.ToolDock.VisibleDockables?.Add(extra);

            var tabStrip = new ToolTabStrip
            {
                DataContext = context.ToolDock,
                ItemsSource = context.ToolDock.VisibleDockables
            };

            var window = CreateThemedWindow(tabStrip, theme);
            ShowWindow(window);

            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var tabItem = tabStrip.ContainerFromIndex(0) as ToolTabStripItem
                          ?? FindVisualDescendant<ToolTabStripItem>(tabStrip);
            Assert.NotNull(tabItem);

            var menu = tabItem!.TabContextMenu;
            Assert.NotNull(menu);

            var menuItems = CollectMenuItems(menu!);

            OpenAndCloseContextMenu(tabItem, menu, () =>
            {
                foreach (var item in menuItems)
                {
                    InvokeMenuItemClick(item);
                }
            });

            tabItem.TabContextMenu = null;
            tabStrip.ItemsSource = null;
            tabStrip.DataContext = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(tabStrip),
                new WeakReference(tabItem),
                menuItems.Select(item => new WeakReference(item)).ToArray(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            tabStrip = null;
            tabItem = null;
            menuItems = null;
            extra = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuItemRefs);
            AssertCollected(result.ItemRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunToolPinItemMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            context.ToolDock.CanCloseLastDockable = true;
            context.Tool.CanClose = true;
            context.Tool.CanFloat = true;
            context.Tool.CanPin = true;
            context.Tool.CanDockAsDocument = true;

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

            var menu = pinItem!.PinContextMenu;
            Assert.NotNull(menu);

            var menuItems = CollectMenuItems(menu!);

            OpenAndCloseContextMenu(pinItem, menu, () =>
            {
                foreach (var item in menuItems)
                {
                    InvokeMenuItemClick(item);
                }
            });

            pinItem.PinContextMenu = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(pinnedControl),
                new WeakReference(pinItem),
                menuItems.Select(item => new WeakReference(item)).ToArray(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            pinnedControl = null;
            pinItem = null;
            menuItems = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuItemRefs);
            AssertCollected(result.ItemRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunToolChromeMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            context.ToolDock.CanCloseLastDockable = true;
            context.Tool.CanClose = true;
            context.Tool.CanFloat = true;
            context.Tool.CanPin = true;
            context.Tool.CanDockAsDocument = true;

            var chrome = new ToolChromeControl
            {
                DataContext = context.ToolDock
            };

            var window = CreateThemedWindow(chrome, theme);
            ShowWindow(window);

            chrome.ApplyTemplate();
            chrome.UpdateLayout();
            DrainDispatcher();

            var flyout = chrome.ToolFlyout;
            Assert.NotNull(flyout);

            var menuItems = CollectMenuItems(flyout!);

            OpenAndCloseFlyout(chrome, flyout, () =>
            {
                foreach (var item in menuItems)
                {
                    InvokeMenuItemClick(item);
                }
            });

            chrome.ToolFlyout = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(chrome),
                new WeakReference(chrome),
                menuItems.Select(item => new WeakReference(item)).ToArray(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            chrome = null;
            menuItems = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuItemRefs);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunMdiDocumentWindowMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            var document = context.Document;
            document.MdiState = MdiWindowState.Normal;
            document.CanClose = true;
            document.CanFloat = true;

            context.DocumentDock.CanCloseLastDockable = true;
            context.DocumentDock.CanDrag = true;
            context.DocumentDock.CanDrop = true;
            context.DocumentDock.CascadeDocuments = new NoOpCommand();
            context.DocumentDock.TileDocumentsHorizontal = new NoOpCommand();
            context.DocumentDock.TileDocumentsVertical = new NoOpCommand();
            context.DocumentDock.RestoreDocuments = new NoOpCommand();

            var extra = (Dock.Model.Avalonia.Controls.Document)context.Factory.CreateDocument();
            extra.Factory = context.Factory;
            extra.Owner = context.DocumentDock;
            extra.CanClose = true;
            extra.CanFloat = true;
            context.DocumentDock.VisibleDockables?.Add(extra);

            var control = new MdiDocumentWindow
            {
                DataContext = document
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var menu = control.DocumentContextMenu;
            Assert.NotNull(menu);

            var menuItems = CollectMenuItems(menu!);

            OpenAndCloseContextMenu(control, menu, () =>
            {
                foreach (var item in menuItems)
                {
                    InvokeMenuItemClick(item);
                }
            });

            control.DocumentContextMenu = null;
            control.DataContext = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(control),
                new WeakReference(control),
                menuItems.Select(item => new WeakReference(item)).ToArray(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            control = null;
            menuItems = null;
            extra = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.MenuItemRefs);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static void RunMdiToolWindowMenuTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create(new NoOpFactory());
            context.Root.ActiveDockable = context.ToolDock;
            context.Root.DefaultDockable = context.ToolDock;
            context.ToolDock.Owner = context.Root;
            context.ToolDock.ActiveDockable = context.Tool;
            context.ToolDock.CanCloseLastDockable = true;
            context.Tool.CanClose = true;
            context.Tool.CanPin = true;
            context.Tool.CanDockAsDocument = true;
            context.Tool.CanFloat = true;

            var dockWindow = new Dock.Model.Avalonia.Core.DockWindow
            {
                Factory = context.Factory,
                Layout = context.Root
            };

            var managedDocument = new ManagedDockWindowDocument(dockWindow)
            {
                Factory = context.Factory,
                Owner = context.Root
            };

            var control = new MdiDocumentWindow
            {
                DataContext = managedDocument
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var menuButton = FindTemplateChild<Button>(control, "PART_ToolMenuButton");
            var pinButton = FindTemplateChild<Button>(control, "PART_ToolPinButton");
            var closeButton = FindTemplateChild<Button>(control, "PART_ToolCloseButton");

            if (menuButton is not null)
            {
                var flyout = menuButton.Flyout;
                if (flyout is not null)
                {
                    var menuItems = CollectMenuItems(flyout);

                    OpenAndCloseFlyout(menuButton, flyout, () =>
                    {
                        foreach (var item in menuItems)
                        {
                            InvokeMenuItemClick(item);
                        }
                    });
                }
            }

            InvokeButtonClick(pinButton);
            InvokeButtonClick(closeButton);
            DrainDispatcher();

            control.DataContext = null;
            managedDocument.Dispose();
            dockWindow.Layout = null;

            var result = new MenuInteractionLeakResult(
                new WeakReference(control),
                new WeakReference(control),
                Array.Empty<WeakReference>(),
                context.Factory,
                context.Root);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            control = null;
            menuButton = null;
            pinButton = null;
            closeButton = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.LayoutKeepAlive);
    }

    private static List<MenuItem> CollectMenuItems(ContextMenu menu)
    {
        var items = new List<MenuItem>();
        CollectMenuItems(menu.Items, items);
        return items;
    }

    private static List<MenuItem> CollectMenuItems(FlyoutBase flyout)
    {
        if (flyout is MenuFlyout menuFlyout)
        {
            var items = new List<MenuItem>();
            CollectMenuItems(menuFlyout.Items, items);
            return items;
        }

        return new List<MenuItem>();
    }

    private static void CollectMenuItems(IEnumerable? source, ICollection<MenuItem> target)
    {
        if (source is null)
        {
            return;
        }

        foreach (var entry in source)
        {
            if (entry is MenuItem menuItem)
            {
                target.Add(menuItem);
                CollectMenuItems(menuItem.Items, target);
            }
        }
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

    private sealed record MenuInteractionLeakResult(
        WeakReference ControlRef,
        WeakReference ItemRef,
        WeakReference[] MenuItemRefs,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);
}
