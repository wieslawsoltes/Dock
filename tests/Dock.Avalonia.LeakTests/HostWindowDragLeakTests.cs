using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class HostWindowDragLeakTests
{
    [ReleaseFact]
    public void HostWindow_TitleBarDragBegin_CallsFactory_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var factory = new TestFactory();
            var root = (IRootDock)factory.CreateRootDock();
            root.Factory = factory;
            root.VisibleDockables = factory.CreateList<IDockable>();

            var dockWindow = new DockWindow
            {
                Factory = factory,
                Layout = root,
                Title = "Host"
            };

            var window = new HostWindow
            {
                Window = dockWindow,
                ToolChromeControlsWholeWindow = true,
                Content = new Border()
            };
            dockWindow.Host = window;

            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            window.ApplyTemplate();
            window.UpdateLayout();
            DrainDispatcher();

            var titleBar = FindTemplateChild<HostWindowTitleBar>(window, "PART_TitleBar")
                           ?? FindVisualDescendant<HostWindowTitleBar>(window);
            Assert.NotNull(titleBar);

            var background = titleBar is null
                ? null
                : FindTemplateChild<Control>(titleBar, "PART_Background");
            Assert.NotNull(background);

            if (background is not null)
            {
                RaisePointerPressed(background, MouseButton.Left);
            }
            DrainDispatcher();

            window.Window = null;
            dockWindow.Host = null;
            ClearFactoryCaches(factory);

            var result = new HostWindowDragLeakResult(
                new WeakReference(window),
                titleBar is null ? null : new WeakReference(titleBar),
                dockWindow,
                factory,
                factory.MoveDragBeginCalled);

            CleanupWindow(window);
            window = null;
            titleBar = null;

            return result;
        });

        Assert.True(result.MoveDragBeginCalled, "Factory OnWindowMoveDragBegin was not called.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.WindowRef);
            if (result.TitleBarRef is not null)
            {
                AssertCollected(result.TitleBarRef);
            }
        }
        GC.KeepAlive(result.DockWindowKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private sealed record HostWindowDragLeakResult(
        WeakReference WindowRef,
        WeakReference? TitleBarRef,
        DockWindow DockWindowKeepAlive,
        TestFactory FactoryKeepAlive,
        bool MoveDragBeginCalled);

    private sealed class TestFactory : Factory
    {
        public bool MoveDragBeginCalled { get; private set; }

        public override bool OnWindowMoveDragBegin(IDockWindow? window)
        {
            MoveDragBeginCalled = true;
            return false;
        }
    }
}
