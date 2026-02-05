using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class MdiControlDetachLeakTests
{
    [ReleaseFact]
    public void MdiDocumentControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var dock = context.DocumentDock;

            var control = new MdiDocumentControl
            {
                DataContext = dock
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new MdiControlDetachLeakResult(controlRef, window, dock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "MdiDocumentControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void MdiDocumentWindow_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var document = context.Document;
            document.MdiState = MdiWindowState.Normal;

            var control = new MdiDocumentWindow
            {
                DataContext = document
            };
            var detached = false;
            control.DetachedFromVisualTree += (_, _) => detached = true;

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            RaisePointerPressed(control, MouseButton.Left);
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new MdiControlDetachLeakResult(controlRef, window, document, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "MdiDocumentWindow did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private sealed record MdiControlDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDockable DockKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);
}
