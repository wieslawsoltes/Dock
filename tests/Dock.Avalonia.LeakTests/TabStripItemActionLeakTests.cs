using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class TabStripItemActionLeakTests
{
    [ReleaseFact]
    public void DocumentTabStripItem_DoubleTap_Floats_DoesNotLeak()
    {
        var previousMode = DockSettings.FloatingWindowHostMode;
        DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Root.ActiveDockable = context.DocumentDock;
                context.Root.DefaultDockable = context.DocumentDock;
                context.Root.Windows ??= context.Factory.CreateList<IDockWindow>();
                context.Document.CanFloat = true;

                var extra = context.Factory.CreateDocument();
                extra.Factory = context.Factory;
                extra.Owner = context.DocumentDock;
                context.DocumentDock.VisibleDockables?.Add(extra);

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
                var tabItem = GetDocumentTabItem(dockControl, context.Document);

                RaiseDoubleTapped(tabItem);
                DrainDispatcher();

                var dockWindow = context.Root.Windows?.FirstOrDefault();
                var dockWindowRef = dockWindow is null ? null : new WeakReference(dockWindow);
                var hostRef = dockWindow?.Host is null ? null : new WeakReference(dockWindow.Host);
                var hostWindow = dockWindow?.Host as Window;
                var hostWindowRef = hostWindow is null ? null : new WeakReference(hostWindow);
                var tabItemRef = new WeakReference(tabItem);

                if (dockWindow is not null)
                {
                    context.Factory.RemoveWindow(dockWindow);
                    DrainDispatcher();
                }

                if (hostWindow is not null)
                {
                    CleanupWindow(hostWindow);
                }

                dockControl.Layout = null;
                dockControl.Factory = null;
                CleanupWindow(window);
                ClearFactoryCaches(context.Factory);
                ResetDispatcherForUnitTests();
                ScrubStaticReferences(tabItem);
                tabItem = null;
                dockControl = null;

                return new TabStripFloatLeakResult(
                    tabItemRef,
                    dockWindowRef,
                    hostRef,
                    hostWindowRef,
                    context.Factory,
                    context.Root);
            });

            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                AssertCollected(result.TabItemRef);
            }

            if (result.DockWindowRef is not null)
            {
                AssertCollected(result.DockWindowRef);
            }

            if (result.HostRef is not null)
            {
                AssertCollected(result.HostRef);
            }

            if (result.HostWindowRef is not null)
            {
                AssertCollected(result.HostWindowRef);
            }

            GC.KeepAlive(result.FactoryKeepAlive);
            GC.KeepAlive(result.RootKeepAlive);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = previousMode;
        }
    }

    [ReleaseFact]
    public void ToolTabStripItem_DoubleTap_Floats_DoesNotLeak()
    {
        var previousMode = DockSettings.FloatingWindowHostMode;
        DockSettings.FloatingWindowHostMode = DockFloatingWindowHostMode.Native;

        try
        {
            var result = RunInSession(() =>
            {
                var context = LeakContext.Create();
                context.Root.ActiveDockable = context.ToolDock;
                context.Root.DefaultDockable = context.ToolDock;
                context.Root.Windows ??= context.Factory.CreateList<IDockWindow>();
                context.Tool.CanFloat = true;

                var extra = context.Factory.CreateTool();
                extra.Factory = context.Factory;
                extra.Owner = context.ToolDock;
                context.ToolDock.VisibleDockables?.Add(extra);

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
                var tabItem = GetToolTabItem(dockControl, context.Tool);

                RaiseDoubleTapped(tabItem);
                DrainDispatcher();

                var dockWindow = context.Root.Windows?.FirstOrDefault();
                var dockWindowRef = dockWindow is null ? null : new WeakReference(dockWindow);
                var hostRef = dockWindow?.Host is null ? null : new WeakReference(dockWindow.Host);
                var hostWindow = dockWindow?.Host as Window;
                var hostWindowRef = hostWindow is null ? null : new WeakReference(hostWindow);
                var tabItemRef = new WeakReference(tabItem);

                if (dockWindow is not null)
                {
                    context.Factory.RemoveWindow(dockWindow);
                    DrainDispatcher();
                }

                if (hostWindow is not null)
                {
                    CleanupWindow(hostWindow);
                }

                dockControl.Layout = null;
                dockControl.Factory = null;
                CleanupWindow(window);
                ClearFactoryCaches(context.Factory);
                ResetDispatcherForUnitTests();
                tabItem = null;
                dockControl = null;

                return new TabStripFloatLeakResult(
                    tabItemRef,
                    dockWindowRef,
                    hostRef,
                    hostWindowRef,
                    context.Factory,
                    context.Root);
            });

            if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
            {
                AssertCollected(result.TabItemRef);
            }

            if (result.DockWindowRef is not null)
            {
                AssertCollected(result.DockWindowRef);
            }

            if (result.HostRef is not null)
            {
                AssertCollected(result.HostRef);
            }

            if (result.HostWindowRef is not null)
            {
                AssertCollected(result.HostWindowRef);
            }

            GC.KeepAlive(result.FactoryKeepAlive);
            GC.KeepAlive(result.RootKeepAlive);
        }
        finally
        {
            DockSettings.FloatingWindowHostMode = previousMode;
        }
    }

    [ReleaseFact]
    public void DocumentTabStripItem_MiddleClick_Closes_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Root.ActiveDockable = context.DocumentDock;
            context.Root.DefaultDockable = context.DocumentDock;
            context.Document.CanClose = true;

            var extra = context.Factory.CreateDocument();
            extra.Factory = context.Factory;
            extra.Owner = context.DocumentDock;
            context.DocumentDock.VisibleDockables?.Add(extra);

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
            var tabItem = GetDocumentTabItem(dockControl, context.Document);

            MiddleClick(tabItem);
            DrainDispatcher();

            var closed = context.DocumentDock.VisibleDockables?.Contains(context.Document) == false;
            var tabItemRef = new WeakReference(tabItem);
            var closedDocRef = new WeakReference(context.Document);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();
            tabItem = null;
            dockControl = null;

            return new TabStripCloseLeakResult(
                tabItemRef,
                closedDocRef,
                closed,
                context.Factory,
                context.Root);
        });

        Assert.True(result.Closed, "DocumentTabStripItem middle click did not close the dockable.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.TabItemRef);
            AssertCollected(result.ClosedDockableRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.RootKeepAlive);
    }

    [ReleaseFact]
    public void ToolTabStripItem_MiddleClick_Closes_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Root.ActiveDockable = context.ToolDock;
            context.Root.DefaultDockable = context.ToolDock;
            context.Tool.CanClose = true;

            var extra = context.Factory.CreateTool();
            extra.Factory = context.Factory;
            extra.Owner = context.ToolDock;
            context.ToolDock.VisibleDockables?.Add(extra);

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
            var tabItem = GetToolTabItem(dockControl, context.Tool);

            MiddleClick(tabItem);
            DrainDispatcher();

            var closed = context.ToolDock.VisibleDockables?.Contains(context.Tool) == false;
            var tabItemRef = new WeakReference(tabItem);
            var closedToolRef = new WeakReference(context.Tool);

            CleanupWindow(window);
            ClearFactoryCaches(context.Factory);
            ResetDispatcherForUnitTests();
            tabItem = null;
            dockControl = null;

            return new TabStripCloseLeakResult(
                tabItemRef,
                closedToolRef,
                closed,
                context.Factory,
                context.Root);
        });

        Assert.True(result.Closed, "ToolTabStripItem middle click did not close the dockable.");
        if (string.Equals(Environment.GetEnvironmentVariable("DOCK_LEAK_STRICT"), "1", StringComparison.Ordinal))
        {
            AssertCollected(result.TabItemRef);
            AssertCollected(result.ClosedDockableRef);
        }
        GC.KeepAlive(result.FactoryKeepAlive);
        GC.KeepAlive(result.RootKeepAlive);
    }

    private sealed record TabStripFloatLeakResult(
        WeakReference TabItemRef,
        WeakReference? DockWindowRef,
        WeakReference? HostRef,
        WeakReference? HostWindowRef,
        Dock.Model.Core.IFactory FactoryKeepAlive,
        Dock.Model.Controls.IRootDock RootKeepAlive);

    private sealed record TabStripCloseLeakResult(
        WeakReference TabItemRef,
        WeakReference ClosedDockableRef,
        bool Closed,
        Dock.Model.Core.IFactory FactoryKeepAlive,
        Dock.Model.Controls.IRootDock RootKeepAlive);

    private static DocumentTabStripItem GetDocumentTabItem(DockControl dockControl, IDockable target)
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

        var tabStrip = FindVisualDescendant<DocumentTabStrip>(dockControl)
                       ?? throw new InvalidOperationException("DocumentTabStrip was not generated.");

        tabStrip.ApplyTemplate();
        tabStrip.UpdateLayout();
        DrainDispatcher();

        var tabItem = FindVisualDescendant<DocumentTabStripItem>(tabStrip, item => ReferenceEquals(item.DataContext, target))
                      ?? FindVisualDescendant<DocumentTabStripItem>(dockControl, item => ReferenceEquals(item.DataContext, target));

        if (tabItem is null)
        {
            var index = GetItemIndex(tabStrip, target);
            if (index >= 0)
            {
                tabItem = tabStrip.ContainerFromIndex(index) as DocumentTabStripItem;
            }
        }

        tabItem ??= tabStrip.ContainerFromIndex(0) as DocumentTabStripItem
                   ?? FindVisualDescendant<DocumentTabStripItem>(dockControl);

        if (tabItem is null)
        {
            throw new InvalidOperationException("DocumentTabStripItem was not generated.");
        }

        tabItem.ApplyTemplate();
        tabItem.UpdateLayout();
        DrainDispatcher();
        return tabItem;
    }

    private static ToolTabStripItem GetToolTabItem(DockControl dockControl, IDockable target)
    {
        dockControl.ApplyTemplate();
        dockControl.UpdateLayout();
        DrainDispatcher();

        var toolDockControl = FindVisualDescendant<ToolDockControl>(dockControl);
        if (toolDockControl is not null)
        {
            toolDockControl.ApplyTemplate();
            toolDockControl.UpdateLayout();
            DrainDispatcher();
        }

        var toolControl = FindVisualDescendant<ToolControl>(dockControl);
        if (toolControl is not null)
        {
            toolControl.ApplyTemplate();
            toolControl.UpdateLayout();
            DrainDispatcher();
        }

        var tabStrip = FindVisualDescendant<ToolTabStrip>(dockControl)
                       ?? throw new InvalidOperationException("ToolTabStrip was not generated.");

        tabStrip.ApplyTemplate();
        tabStrip.UpdateLayout();
        DrainDispatcher();

        var tabItem = FindVisualDescendant<ToolTabStripItem>(tabStrip, item => ReferenceEquals(item.DataContext, target))
                      ?? FindVisualDescendant<ToolTabStripItem>(dockControl, item => ReferenceEquals(item.DataContext, target));

        if (tabItem is null)
        {
            var index = GetItemIndex(tabStrip, target);
            if (index >= 0)
            {
                tabItem = tabStrip.ContainerFromIndex(index) as ToolTabStripItem;
            }
        }

        tabItem ??= tabStrip.ContainerFromIndex(0) as ToolTabStripItem
                   ?? FindVisualDescendant<ToolTabStripItem>(dockControl);

        if (tabItem is null)
        {
            throw new InvalidOperationException("ToolTabStripItem was not generated.");
        }

        tabItem.ApplyTemplate();
        tabItem.UpdateLayout();
        DrainDispatcher();
        return tabItem;
    }

    private static int GetItemIndex(ItemsControl itemsControl, IDockable target)
    {
        if (itemsControl.Items is System.Collections.IList list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], target))
                {
                    return i;
                }
            }

            return -1;
        }

        var index = 0;
        foreach (var item in itemsControl.Items)
        {
            if (ReferenceEquals(item, target))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static void MiddleClick(Control control)
    {
        var bounds = control.Bounds;
        var width = double.IsFinite(bounds.Width) && bounds.Width > 1 ? bounds.Width / 2 : 1;
        var height = double.IsFinite(bounds.Height) && bounds.Height > 1 ? bounds.Height / 2 : 1;
        var point = new Point(width, height);
        var root = (Visual?)TopLevel.GetTopLevel(control) ?? control;
        var pointer = new Pointer(1, PointerType.Mouse, true);

        var pressedProperties = new PointerPointProperties(RawInputModifiers.MiddleMouseButton, PointerUpdateKind.MiddleButtonPressed);
        var pressedArgs = new PointerPressedEventArgs(
            control,
            pointer,
            root,
            point,
            1,
            pressedProperties,
            KeyModifiers.None,
            1);
        control.RaiseEvent(pressedArgs);

        var releasedProperties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.MiddleButtonReleased);
        var releasedArgs = new PointerReleasedEventArgs(
            control,
            pointer,
            root,
            point,
            1,
            releasedProperties,
            KeyModifiers.None,
            MouseButton.Middle);
        control.RaiseEvent(releasedArgs);
    }
}
