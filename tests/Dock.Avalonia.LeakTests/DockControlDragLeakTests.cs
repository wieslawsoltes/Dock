using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Xunit;
using static Dock.Avalonia.LeakTests.LeakTestHelpers;
using static Dock.Avalonia.LeakTests.LeakTestSession;

namespace Dock.Avalonia.LeakTests;

[Collection("LeakTests")]
public class DockControlDragLeakTests
{
    [ReleaseFact]
    public void DockControl_DragDockable_ShowsAndCleansOverlays_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            context.Document.CanDrag = true;
            context.Document.CanFloat = true;
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
            DrainDispatcher();
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

            var tabStrip = FindVisualDescendant<DocumentTabStrip>(dockControl);
            if (tabStrip is not null)
            {
                tabStrip.ApplyTemplate();
                tabStrip.UpdateLayout();
                DrainDispatcher();
            }

            var tabItem = tabStrip?.ItemContainerGenerator.ContainerFromIndex(0) as DocumentTabStripItem
                          ?? FindVisualDescendant<DocumentTabStripItem>(dockControl);
            if (tabStrip is null)
            {
                TraceVisualTree(dockControl, "DockControl_DragDockable/visuals");
            }
            Assert.NotNull(tabStrip);
            Assert.NotNull(tabItem);

            var startPoint = new Point(tabItem!.Bounds.Width / 2, tabItem.Bounds.Height / 2);
            var outsideInItems = new Point(tabStrip!.Bounds.Width + 100, tabStrip.Bounds.Height + 100);
            var outsideInTab = tabStrip.TranslatePoint(outsideInItems, tabItem)
                               ?? new Point(startPoint.X + tabStrip.Bounds.Width + 100,
                                   startPoint.Y + tabStrip.Bounds.Height + 100);

            RaisePointerPressed(tabItem, startPoint, MouseButton.Left);
            RaisePointerMoved(tabItem, outsideInTab, leftPressed: true);
            DrainDispatcher();

            var dragStarted = dockControl.IsDraggingDock || dockControl.IsOpen;

            var dockPoint = new Point(dockControl.Bounds.Width / 2, dockControl.Bounds.Height / 2);
            RaisePointerMoved(dockControl, dockPoint, leftPressed: true);
            RaisePointerReleased(dockControl, dockPoint, MouseButton.Left);
            DrainDispatcher();

            var result = new DragLeakResult(
                new WeakReference(dockControl),
                new WeakReference(context.Root),
                dragStarted);

            CleanupWindow(window);
            return result;
        });

        Assert.True(result.DragStarted, "Dock drag did not start.");
        AssertCollected(result.ControlRef, result.LayoutRef);
    }
}
