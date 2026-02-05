using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestCaseHelpers;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class ControlDetachLeakTests
{
    [ReleaseFact]
    public void DocumentControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.DocumentDock.CanCreateDocument = true;

            var control = new DocumentControl
            {
                DataContext = context.DocumentDock
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

            return new ControlDetachLeakResult(controlRef, window, context.DocumentDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "DocumentControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void ToolControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new ToolControl
            {
                DataContext = context.ToolDock
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

            return new ControlDetachLeakResult(controlRef, window, context.ToolDock, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "ToolControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void RootDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new RootDockControl
            {
                DataContext = context.Root
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

            var mainContent = FindTemplateChild<ContentControl>(control, "PART_MainContent");
            if (mainContent is not null)
            {
                RaisePointerPressed(mainContent, MouseButton.Left);
            }
            DrainDispatcher();

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);

            var controlRef = new WeakReference(control);
            control = null;

            return new RootControlDetachLeakResult(controlRef, window, context.Root, context.Factory, detached);
        });

        Assert.True(result.DetachedFromVisualTree, "RootDockControl did not detach from visual tree.");
        AssertCollected(result.ControlRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.RootKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DocumentDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new DocumentDockControl
            {
                DataContext = context.DocumentDock
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

            var wasRegistered = context.Factory.VisibleDockableControls.ContainsKey(context.DocumentDock);

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var stillRegistered = context.Factory.VisibleDockableControls.ContainsKey(context.DocumentDock);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockableDockDetachLeakResult(
                controlRef,
                window,
                context.DocumentDock,
                context.Factory,
                wasRegistered,
                stillRegistered,
                detached);
        });

        Assert.True(result.WasRegistered, "DocumentDockControl did not register with the factory.");
        Assert.False(result.StillRegistered, "DocumentDockControl remained registered after detach.");
        Assert.True(result.DetachedFromVisualTree, "DocumentDockControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void ToolDockControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new ToolDockControl
            {
                DataContext = context.ToolDock
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

            var wasRegistered = context.Factory.VisibleDockableControls.ContainsKey(context.ToolDock);

            window.Content = null;
            DrainDispatcher();
            window.UpdateLayout();
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var stillRegistered = context.Factory.VisibleDockableControls.ContainsKey(context.ToolDock);

            var controlRef = new WeakReference(control);
            control = null;

            return new DockableDockDetachLeakResult(
                controlRef,
                window,
                context.ToolDock,
                context.Factory,
                wasRegistered,
                stillRegistered,
                detached);
        });

        Assert.True(result.WasRegistered, "ToolDockControl did not register with the factory.");
        Assert.False(result.StillRegistered, "ToolDockControl remained registered after detach.");
        Assert.True(result.DetachedFromVisualTree, "ToolDockControl did not detach from visual tree.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.ControlRef);
        }
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockKeepAlive);
        GC.KeepAlive(result.FactoryKeepAlive);
    }

    [ReleaseFact]
    public void DocumentDockControl_DetachWhileWindowAlive_DoesNotLeak_InnerControls()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new DocumentDockControl
            {
                DataContext = context.DocumentDock
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var documentControl = FindVisualDescendant<DocumentControl>(control);
            var mdiControl = FindVisualDescendant<MdiDocumentControl>(control);

            var documentDetached = false;
            var mdiDetached = false;

            if (documentControl is not null)
            {
                documentControl.DetachedFromVisualTree += (_, _) => documentDetached = true;
            }

            if (mdiControl is not null)
            {
                mdiControl.DetachedFromVisualTree += (_, _) => mdiDetached = true;
            }

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var documentRef = documentControl is null ? null : new WeakReference(documentControl);
            var mdiRef = mdiControl is null ? null : new WeakReference(mdiControl);
            documentControl = null;
            mdiControl = null;

            return new DocumentDockInnerControlLeakResult(
                documentRef,
                mdiRef,
                window,
                documentDetached,
                mdiDetached);
        });

        if (result.DocumentControlRef is not null)
        {
            Assert.True(result.DocumentDetached, "DocumentControl did not detach from visual tree.");
        }

        if (result.MdiControlRef is not null)
        {
            Assert.True(result.MdiDetached, "MdiDocumentControl did not detach from visual tree.");
        }

        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            if (result.DocumentControlRef is not null)
            {
                AssertCollected(result.DocumentControlRef);
            }

            if (result.MdiControlRef is not null)
            {
                AssertCollected(result.MdiControlRef);
            }
        }

        GC.KeepAlive(result.WindowKeepAlive);
    }

    [ReleaseFact]
    public void ToolDockControl_DetachWhileWindowAlive_DoesNotLeak_InnerControls()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();

            var control = new ToolDockControl
            {
                DataContext = context.ToolDock
            };

            var window = new Window { Content = control };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            control.ApplyTemplate();
            control.UpdateLayout();
            DrainDispatcher();

            var chromeControl = FindVisualDescendant<ToolChromeControl>(control);
            var toolControl = FindVisualDescendant<ToolControl>(control);

            var chromeDetached = false;
            var toolDetached = false;

            if (chromeControl is not null)
            {
                chromeControl.DetachedFromVisualTree += (_, _) => chromeDetached = true;
            }

            if (toolControl is not null)
            {
                toolControl.DetachedFromVisualTree += (_, _) => toolDetached = true;
            }

            window.Content = null;
            DrainDispatcher();
            ClearInputState(window);
            ResetDispatcherForUnitTests();

            var chromeRef = chromeControl is null ? null : new WeakReference(chromeControl);
            var toolRef = toolControl is null ? null : new WeakReference(toolControl);
            chromeControl = null;
            toolControl = null;

            return new ToolDockInnerControlLeakResult(
                chromeRef,
                toolRef,
                window,
                chromeDetached,
                toolDetached);
        });

        if (result.ChromeControlRef is not null)
        {
            Assert.True(result.ChromeDetached, "ToolChromeControl did not detach from visual tree.");
        }

        if (result.ToolControlRef is not null)
        {
            Assert.True(result.ToolDetached, "ToolControl did not detach from visual tree.");
        }

        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            if (result.ChromeControlRef is not null)
            {
                AssertCollected(result.ChromeControlRef);
            }

            if (result.ToolControlRef is not null)
            {
                AssertCollected(result.ToolControlRef);
            }
        }

        GC.KeepAlive(result.WindowKeepAlive);
    }

    private sealed record ControlDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDock DockKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);

    private sealed record RootControlDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDock RootKeepAlive,
        IFactory FactoryKeepAlive,
        bool DetachedFromVisualTree);

    private sealed record DockableDockDetachLeakResult(
        WeakReference ControlRef,
        Window WindowKeepAlive,
        IDock DockKeepAlive,
        IFactory FactoryKeepAlive,
        bool WasRegistered,
        bool StillRegistered,
        bool DetachedFromVisualTree);

    private sealed record DocumentDockInnerControlLeakResult(
        WeakReference? DocumentControlRef,
        WeakReference? MdiControlRef,
        Window WindowKeepAlive,
        bool DocumentDetached,
        bool MdiDetached);

    private sealed record ToolDockInnerControlLeakResult(
        WeakReference? ChromeControlRef,
        WeakReference? ToolControlRef,
        Window WindowKeepAlive,
        bool ChromeDetached,
        bool ToolDetached);
}
