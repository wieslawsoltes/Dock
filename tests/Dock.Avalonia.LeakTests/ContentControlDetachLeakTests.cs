using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ContentControlDetachLeakTests
{
    [ReleaseFact]
    public void DocumentContentControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new DocumentContentControl
            {
                DataContext = context.Document
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

            var wasRegistered = context.Factory.DocumentControls.ContainsKey(context.Document);

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var stillRegistered = context.Factory.DocumentControls.ContainsKey(context.Document);

            var controlRef = new WeakReference(control);
            control = null;

            return new ContentControlLeakResult(controlRef, window, context.Factory, context.Document, wasRegistered, stillRegistered, detached);
        });

        Assert.True(result.WasRegistered, "DocumentContentControl did not register with the factory.");
        Assert.False(result.StillRegistered, "DocumentContentControl remained registered after detach.");
        Assert.True(result.DetachedFromVisualTree, "DocumentContentControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.DockableKeepAlive);
    }

    [ReleaseFact]
    public void ToolContentControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new ToolContentControl
            {
                DataContext = context.Tool
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

            var wasRegistered = context.Factory.ToolControls.ContainsKey(context.Tool);

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var stillRegistered = context.Factory.ToolControls.ContainsKey(context.Tool);

            var controlRef = new WeakReference(control);
            control = null;

            return new ContentControlLeakResult(controlRef, window, context.Factory, context.Tool, wasRegistered, stillRegistered, detached);
        });

        Assert.True(result.WasRegistered, "ToolContentControl did not register with the factory.");
        Assert.False(result.StillRegistered, "ToolContentControl remained registered after detach.");
        Assert.True(result.DetachedFromVisualTree, "ToolContentControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.DockableKeepAlive);
    }

    private sealed record ContentControlLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IFactory FactoryKeepAlive,
        IDockable DockableKeepAlive,
        bool WasRegistered,
        bool StillRegistered,
        bool DetachedFromVisualTree);
}
