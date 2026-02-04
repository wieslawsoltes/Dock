using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ToolChromeControlLeakTests
{
    [ReleaseFact]
    public void ToolChromeControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var chromeControl = new ToolChromeControl
            {
                DataContext = context.ToolDock
            };

            var window = new HostWindow { Content = chromeControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            chromeControl.ApplyTemplate();
            chromeControl.UpdateLayout();
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(chromeControl);
            chromeControl = null;

            return new ToolChromeDetachLeakResult(controlRef, window, context.ToolDock);
        });

        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
    }

    private sealed record ToolChromeDetachLeakResult(
        WeakReference ControlRef,
        HostWindow WindowKeepAlive,
        Dock.Model.Core.IDock DockKeepAlive);
}
