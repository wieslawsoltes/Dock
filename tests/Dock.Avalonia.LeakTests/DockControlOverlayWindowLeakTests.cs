using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlOverlayWindowLeakTests
{
    [ReleaseFact]
    public void DockControl_DragPreviewWindow_Closes_DoesNotLeak()
    {
        var previousOpacity = DockSettings.DragPreviewOpacity;
        DockSettings.DragPreviewOpacity = 0.9;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Document.CanDrag = true;
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;

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
                var tabItem = GetDocumentTabItem(dockControl, out var tabStrip);

                var startPoint = new Point(tabItem!.Bounds.Width / 2, tabItem.Bounds.Height / 2);
                var outsideInItems = new Point(tabStrip!.Bounds.Width + 100, tabStrip.Bounds.Height + 100);

                BeginDockDrag(tabItem, tabStrip, startPoint, outsideInItems);
                DrainDispatcher();

                var dragStarted = dockControl.IsDraggingDock || dockControl.IsOpen;
                Assert.True(dragStarted, "Dock drag did not start.");

                var previewWindows = FindOpenWindows<DragPreviewWindow>();
                var previewRefs = previewWindows.Select(window => new WeakReference(window)).ToArray();

                var dockPoint = new Point(dockControl.Bounds.Width / 2, dockControl.Bounds.Height / 2);
                EndDockDrag(dockControl, dockPoint);
                DrainDispatcher();

                var remainingPreviewWindows = FindOpenWindows<DragPreviewWindow>();
                var previewClosed = remainingPreviewWindows.Count == 0
                                    || remainingPreviewWindows.All(previewWindow => !previewWindow.IsVisible);

                foreach (var previewWindow in remainingPreviewWindows)
                {
                    if (previewWindow.IsVisible)
                    {
                        previewWindow.Close();
                    }
                }

                previewWindows.Clear();
                remainingPreviewWindows.Clear();

                CleanupWindow(window);

                return new DragPreviewWindowLeakResult(
                    previewRefs,
                    previewClosed,
                    window,
                    context.Factory,
                    context.Root);
            });

            Assert.True(result.PreviewClosed, "DragPreviewWindow did not close after drag.");
            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                if (result.PreviewWindowRefs.Length > 0)
                {
                    AssertCollected(result.PreviewWindowRefs);
                }
            }
            GC.KeepAlive(result.WindowKeepAlive);
            GC.KeepAlive(result.FactoryKeepAlive);
            GC.KeepAlive(result.LayoutKeepAlive);
        }
        finally
        {
            DockSettings.DragPreviewOpacity = previousOpacity;
        }
    }

    [ReleaseFact]
    public void DockControl_FloatingAdornerWindows_Close_DoesNotLeak()
    {
        var previousAdorner = DockSettings.UseFloatingDockAdorner;
        var previousMode = DockSettings.FloatingWindowHostMode;
        DockSettings.UseFloatingDockAdorner = true;
        DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Document.CanDrag = true;
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;

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
                var tabItem = GetDocumentTabItem(dockControl, out var tabStrip);

                var startPoint = new Point(tabItem!.Bounds.Width / 2, tabItem.Bounds.Height / 2);
                var outsideInItems = new Point(tabStrip!.Bounds.Width + 100, tabStrip.Bounds.Height + 100);

                BeginDockDrag(tabItem, tabStrip, startPoint, outsideInItems);
                DrainDispatcher();

                var dragStarted = dockControl.IsDraggingDock || dockControl.IsOpen;
                Assert.True(dragStarted, "Dock drag did not start.");

                var dockPoint = new Point(dockControl.Bounds.Width / 2, dockControl.Bounds.Height / 2);
                RaisePointerMoved(dockControl, dockPoint, leftPressed: true);
                DrainDispatcher();

                var adornerWindows = FindOpenWindows<DockAdornerWindow>();
                var adornerRefs = adornerWindows.Select(window => new WeakReference(window)).ToArray();

                RaisePointerReleased(dockControl, dockPoint, MouseButton.Left);
                DrainDispatcher();

                var remainingAdornerWindows = FindOpenWindows<DockAdornerWindow>();
                var allClosed = remainingAdornerWindows.Count == 0
                                || remainingAdornerWindows.All(adornerWindow => !adornerWindow.IsVisible);

                foreach (var adornerWindow in remainingAdornerWindows)
                {
                    if (adornerWindow.IsVisible)
                    {
                        adornerWindow.Close();
                    }
                }

                adornerWindows.Clear();
                remainingAdornerWindows.Clear();

                CleanupWindow(window);

                return new DockAdornerWindowLeakResult(
                    adornerRefs,
                    allClosed,
                    window,
                    context.Factory,
                    context.Root);
            });

            Assert.True(result.AllClosed, "DockAdornerWindow did not close after drag.");
            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                if (result.AdornerRefs.Length > 0)
                {
                    AssertCollected(result.AdornerRefs);
                }
            }
            GC.KeepAlive(result.WindowKeepAlive);
            GC.KeepAlive(result.FactoryKeepAlive);
            GC.KeepAlive(result.LayoutKeepAlive);
        }
        finally
        {
            DockSettings.UseFloatingDockAdorner = previousAdorner;
            DockSettings.FloatingWindowHostMode = previousMode;
        }
    }

    [ReleaseFact]
    public void DockControl_ManagedDragPreviewOverlay_Clears_DoesNotLeak()
    {
        var previousMode = DockSettings.FloatingWindowHostMode;
        DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Root.FloatingWindowHostMode = DockFloatingWindowHostMode.Managed;
                context.Document.CanDrag = true;
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;

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
                var managedLayer = FindVisualDescendant<ManagedWindowLayer>(dockControl);
                Assert.NotNull(managedLayer);
                var tabItem = GetDocumentTabItem(dockControl, out var tabStrip);

                var startPoint = new Point(tabItem!.Bounds.Width / 2, tabItem.Bounds.Height / 2);
                var outsideInItems = new Point(tabStrip!.Bounds.Width + 100, tabStrip.Bounds.Height + 100);

                BeginDockDrag(tabItem, tabStrip, startPoint, outsideInItems);
                DrainDispatcher();

                var dragStarted = dockControl.IsDraggingDock || dockControl.IsOpen;
                Assert.True(dragStarted, "Dock drag did not start.");

                var previewControl = managedLayer is null
                    ? null
                    : FindVisualDescendant<DragPreviewControl>(managedLayer);

                Assert.NotNull(previewControl);

                var dockPoint = new Point(dockControl.Bounds.Width / 2, dockControl.Bounds.Height / 2);
                EndDockDrag(dockControl, dockPoint);
                DrainDispatcher();

                var previewCleared = managedLayer is null
                    ? true
                    : FindVisualDescendant<DragPreviewControl>(managedLayer) is null;

                var previewRef = previewControl is null ? null : new WeakReference(previewControl);
                previewControl = null;

                CleanupWindow(window);

                return new ManagedDragPreviewLeakResult(
                    previewRef,
                    previewCleared,
                    window,
                    context.Factory,
                    context.Root);
            });

            Assert.True(result.PreviewCleared, "Managed drag preview overlay was not cleared.");
            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                if (result.PreviewRef is not null)
                {
                    AssertCollected(result.PreviewRef);
                }
            }
            GC.KeepAlive(result.WindowKeepAlive);
            GC.KeepAlive(result.FactoryKeepAlive);
            GC.KeepAlive(result.LayoutKeepAlive);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = previousMode;
        }
    }

    private static List<T> FindOpenWindows<T>() where T : Window
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime.Windows.OfType<T>().ToList();
        }

        return new List<T>();
    }

    private static T? FindOpenWindow<T>() where T : Window
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            return lifetime.Windows.OfType<T>().FirstOrDefault();
        }

        return null;
    }

    private static DocumentTabStripItem GetDocumentTabItem(DockControl dockControl, out DocumentTabStrip tabStrip)
    {
        dockControl.ApplyTemplate();
        dockControl.UpdateLayout();
        DrainDispatcher();

        var documentDockControl = FindVisualDescendant<DocumentDockControl>(dockControl);
        if (documentDockControl is not null)
        {
            documentDockControl.ApplyTemplate();
            documentDockControl.UpdateLayout();
            DrainDispatcher();
        }

        var documentControl = FindVisualDescendant<DocumentControl>(dockControl);
        if (documentControl is not null)
        {
            documentControl.ApplyTemplate();
            documentControl.UpdateLayout();
            DrainDispatcher();
        }

        tabStrip = FindVisualDescendant<DocumentTabStrip>(dockControl)
                   ?? throw new InvalidOperationException("DocumentTabStrip was not generated.");

        tabStrip.ApplyTemplate();
        tabStrip.UpdateLayout();
        DrainDispatcher();

        return tabStrip.ContainerFromIndex(0) as DocumentTabStripItem
               ?? FindVisualDescendant<DocumentTabStripItem>(dockControl)
               ?? throw new InvalidOperationException("DocumentTabStripItem was not generated.");
    }

    private static void BeginDockDrag(Control source, Control boundsHost, Point startPoint, Point outsidePointInHost)
    {
        var outsideInSource = boundsHost.TranslatePoint(outsidePointInHost, source)
                             ?? new Point(
                                 startPoint.X + boundsHost.Bounds.Width + 100,
                                 startPoint.Y + boundsHost.Bounds.Height + 100);

        RaisePointerPressed(source, startPoint, MouseButton.Left);
        RaisePointerMoved(source, outsideInSource, leftPressed: true);
    }

    private static void EndDockDrag(Control dockControl, Point dockPoint)
    {
        RaisePointerMoved(dockControl, dockPoint, leftPressed: true);
        RaisePointerReleased(dockControl, dockPoint, MouseButton.Left);
    }

    private sealed record DragPreviewWindowLeakResult(
        WeakReference[] PreviewWindowRefs,
        bool PreviewClosed,
        Window WindowKeepAlive,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);

    private sealed record DockAdornerWindowLeakResult(
        WeakReference[] AdornerRefs,
        bool AllClosed,
        Window WindowKeepAlive,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);

    private sealed record ManagedDragPreviewLeakResult(
        WeakReference? PreviewRef,
        bool PreviewCleared,
        Window WindowKeepAlive,
        IFactory FactoryKeepAlive,
        IDock LayoutKeepAlive);
}
