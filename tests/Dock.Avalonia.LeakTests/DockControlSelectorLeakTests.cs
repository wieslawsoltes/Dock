using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Selectors;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlSelectorLeakTests
{
    [ReleaseFact]
    public void DockControl_ShowHideSelector_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dockControl = new DockControl
            {
                Factory = context.Factory,
                Layout = context.Root,
                InitializeFactory = true,
                InitializeLayout = true
            };
            var window = new Window { Content = dockControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);

            dockControl.ShowSelector(DockSelectorMode.Documents);
            DrainDispatcher();
            dockControl.HideSelector();
            DrainDispatcher();

            var result = new SelectorLeakResult(
                new WeakReference(dockControl),
                new WeakReference(context.Root));

            CleanupWindow(window);
            return result;
        });

        AssertCollected(result.ControlRef, result.LayoutRef);
    }
}
