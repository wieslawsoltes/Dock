using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls.Overlays;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ThemeOverlayInteractionLeakTests
{
    [ReleaseFact]
    public void DialogShellControl_FluentTheme_CloseButtonClick_DoesNotLeak() =>
        RunDialogShellControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void DialogShellControl_SimpleTheme_CloseButtonClick_DoesNotLeak() =>
        RunDialogShellControlTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void ConfirmationDialogControl_FluentTheme_ButtonClicks_DoNotLeak() =>
        RunConfirmationDialogControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void ConfirmationDialogControl_SimpleTheme_ButtonClicks_DoNotLeak() =>
        RunConfirmationDialogControlTest(DockThemeKind.Simple);

    [ReleaseFact]
    public void BusyOverlayControl_FluentTheme_ReloadClick_DoesNotLeak() =>
        RunBusyOverlayControlTest(DockThemeKind.Fluent);

    [ReleaseFact]
    public void BusyOverlayControl_SimpleTheme_ReloadClick_DoesNotLeak() =>
        RunBusyOverlayControlTest(DockThemeKind.Simple);

    private static void RunDialogShellControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var control = new DialogShellControl
            {
                Title = "Dialog",
                Content = new TextBlock { Text = "Body" },
                CloseCommand = new NoOpCommand()
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var closeButton = FindVisualDescendant<Button>(control);
            if (closeButton is not null)
            {
                InvokeButtonClick(closeButton);
                DrainDispatcher();
            }
            else
            {
                Assert.True(theme == DockThemeKind.Simple, "Close button not found for Fluent theme.");
            }

            var result = new OverlayButtonLeakResult(
                new WeakReference(control),
                new WeakReference(closeButton));

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            control = null;
            closeButton = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ButtonRef);
        }
    }

    private static void RunConfirmationDialogControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var control = new ConfirmationDialogControl
            {
                Title = "Confirm",
                Message = "Are you sure?",
                ConfirmText = "Yes",
                CancelText = "No",
                ConfirmCommand = new NoOpCommand(),
                CancelCommand = new NoOpCommand()
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var buttons = GetVisualDescendants<Button>(control);
            if (buttons.Count == 0)
            {
                Assert.True(theme == DockThemeKind.Simple, "Confirmation buttons not found for Fluent theme.");
            }
            else
            {
                foreach (var button in buttons)
                {
                    InvokeButtonClick(button);
                }
                DrainDispatcher();
            }

            var result = new OverlayButtonsLeakResult(
                new WeakReference(control),
                buttons.Select(button => new WeakReference(button)).ToArray());

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            control = null;
            buttons = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ButtonRefs);
        }
    }

    private static void RunBusyOverlayControlTest(DockThemeKind theme)
    {
        var result = RunInSession(() =>
        {
            var busyService = new StubBusyService
            {
                IsReloadVisible = true
            };
            busyService.SetReloadHandler(() => System.Threading.Tasks.Task.CompletedTask);
            var globalService = new StubGlobalBusyService();

            var control = new BusyOverlayControl
            {
                BusyService = busyService,
                GlobalBusyService = globalService,
                ShowReloadButton = true
            };

            var window = CreateThemedWindow(control, theme);
            ShowWindow(window);

            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var reloadButton = FindVisualDescendant<Button>(
                control,
                button => ReferenceEquals(button.Command, control.ReloadCommand));
            if (reloadButton is not null)
            {
                InvokeButtonClick(reloadButton);
                DrainDispatcher();
            }
            else
            {
                Assert.True(theme == DockThemeKind.Simple, "Reload button not found for Fluent theme.");
            }

            var result = new OverlayButtonKeepAliveLeakResult(
                new WeakReference(control),
                new WeakReference(reloadButton),
                busyService,
                globalService);

            CleanupWindow(window);
            ResetDispatcherForUnitTests();

            control = null;
            reloadButton = null;

            return result;
        });

        AssertCollected(result.ControlRef);
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ButtonRef);
        }
        GC.KeepAlive(result.BusyServiceKeepAlive);
        GC.KeepAlive(result.GlobalServiceKeepAlive);
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

    private static System.Collections.Generic.List<T> GetVisualDescendants<T>(Control root) where T : Control
    {
        var results = new System.Collections.Generic.List<T>();
        foreach (var visual in root.GetVisualDescendants())
        {
            if (visual is T control)
            {
                results.Add(control);
            }
        }
        return results;
    }

    private enum DockThemeKind
    {
        Fluent,
        Simple
    }

    private sealed record OverlayButtonLeakResult(
        WeakReference ControlRef,
        WeakReference ButtonRef);

    private sealed record OverlayButtonsLeakResult(
        WeakReference ControlRef,
        WeakReference[] ButtonRefs);

    private sealed record OverlayButtonKeepAliveLeakResult(
        WeakReference ControlRef,
        WeakReference ButtonRef,
        StubBusyService BusyServiceKeepAlive,
        StubGlobalBusyService GlobalServiceKeepAlive);
}
