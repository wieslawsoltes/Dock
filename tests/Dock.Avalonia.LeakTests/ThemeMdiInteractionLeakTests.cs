using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
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
public class ThemeMdiInteractionLeakTests
{
    [ReleaseFact]
    public void MdiDocumentWindow_FluentTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunMdiDocumentWindowTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void MdiDocumentWindow_SimpleTheme_ButtonAndMenuClicks_DoNotLeak() =>
        RunMdiDocumentWindowTest(DockThemeKind.Simple);

    private static void RunMdiDocumentWindowTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var document = context.Document;
            document.MdiState = MdiWindowState.Normal;
            document.CanClose = true;
            document.CanFloat = true;

            context.DocumentDock.CascadeDocuments = new NoOpCommand();
            context.DocumentDock.TileDocumentsHorizontal = new NoOpCommand();
            context.DocumentDock.TileDocumentsVertical = new NoOpCommand();
            context.DocumentDock.RestoreDocuments = new NoOpCommand();

            var control = new MdiDocumentWindow
            {
                DataContext = document
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var minimizeButton = FindTemplateChild<Button>(control, "PART_MinimizeButton");
            var maximizeButton = FindTemplateChild<Button>(control, "PART_MaximizeRestoreButton");
            var closeButton = FindTemplateChild<Button>(control, "PART_CloseButton");
            Assert.NotNull(minimizeButton);
            Assert.NotNull(maximizeButton);
            Assert.NotNull(closeButton);

            InvokeButtonClick(minimizeButton);
            InvokeButtonClick(maximizeButton);
            InvokeButtonClick(closeButton);
            DrainDispatcher();

            var menu = control.DocumentContextMenu;
            MenuItem? menuItem = null;
            if (menu is not null)
            {
                menuItem = FindMenuItemByHeader(menu, "Cascade");
                OpenAndCloseContextMenu(control, menu, () => InvokeMenuItemClick(menuItem));
            }

            control.DocumentContextMenu = null;
            control.DataContext = null;

            var result = new MdiInteractionLeakResult(
                new WeakReference(control),
                new WeakReference(minimizeButton!),
                new WeakReference(maximizeButton!),
                new WeakReference(closeButton!),
                menuItem is null ? null : new WeakReference(menuItem),
                context.Factory,
                document);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();

            control = null;
            minimizeButton = null;
            maximizeButton = null;
            closeButton = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            var refs = new[] { result.MinimizeButtonRef, result.MaximizeButtonRef, result.CloseButtonRef }
                .Where(reference => reference is not null)
                .Cast<WeakReference>()
                .ToArray();
            if (refs.Length > 0)
            {
                AssertCollected(refs);
            }
            if (result.MenuItemRef is not null)
            {
                AssertCollected(result.MenuItemRef);
            }
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.DocumentKeepAlive);
    }

    private static MenuItem? FindMenuItemByHeader(ContextMenu menu, string header)
    {
        foreach (var visual in menu.GetVisualDescendants())
        {
            if (visual is MenuItem menuItem
                && menuItem.Header is string text
                && string.Equals(text, header, StringComparison.Ordinal))
            {
                return menuItem;
            }
        }

        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            if (item.Header is string text && string.Equals(text, header, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return menu.Items.OfType<MenuItem>().FirstOrDefault();
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

    private sealed record MdiInteractionLeakResult(
        WeakReference ControlRef,
        WeakReference MinimizeButtonRef,
        WeakReference MaximizeButtonRef,
        WeakReference CloseButtonRef,
        WeakReference? MenuItemRef,
        IFactory FactoryKeepAlive,
        IDockable DocumentKeepAlive);
}
