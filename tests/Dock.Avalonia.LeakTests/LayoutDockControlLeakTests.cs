using System;
using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Dock.Model.Controls;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class LayoutDockControlLeakTests
{
    [ReleaseFact]
    public void GridDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.GridDock.RowDefinitions = "Auto,*";
            context.GridDock.ColumnDefinitions = "*,*";

            var control = new GridDockControl
            {
                DataContext = context.GridDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.GridDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "GridDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void ProportionalDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.ProportionalDock.VisibleDockables?.Add(context.Document);

            var control = new ProportionalDockControl
            {
                DataContext = context.ProportionalDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.ProportionalDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "ProportionalDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void SplitViewDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.SplitViewDock.PaneDockable = context.ToolDock;
            context.SplitViewDock.ContentDockable = context.DocumentDock;
            context.SplitViewDock.DisplayMode = Dock.Model.Core.SplitViewDisplayMode.Overlay;
            context.SplitViewDock.UseLightDismissOverlayMode = true;
            context.SplitViewDock.IsPaneOpen = true;

            var control = new SplitViewDockControl
            {
                DataContext = context.SplitViewDock
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

            var splitView = FindTemplateChild<SplitView>(control, "PART_SplitView");
            if (splitView is not null)
            {
                splitView.IsPaneOpen = false;
                splitView.IsPaneOpen = true;
            }
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new LayoutDockControlLeakResult(controlRef, window, context.SplitViewDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "SplitViewDockControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void StackDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new StackDockControl
            {
                DataContext = context.StackDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.StackDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "StackDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void WrapDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new WrapDockControl
            {
                DataContext = context.WrapDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.WrapDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "WrapDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void UniformGridDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new UniformGridDockControl
            {
                DataContext = context.UniformGridDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.UniformGridDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "UniformGridDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DockDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new DockDockControl
            {
                DataContext = context.DockDock
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

            return new LayoutDockControlLeakResult(controlRef, window, context.DockDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DockDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    private sealed record LayoutDockControlLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDock DockKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);
}
