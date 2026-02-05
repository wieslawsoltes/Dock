using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
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
public class ItemDragAutoScrollLeakTests
{
    [ReleaseFact]
    public void ToolTabStripItem_AutoScroll_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var tools = new List<IDockable>();

            var tabStrip = new ToolTabStrip
            {
                Width = 180,
                Height = 30
            };

            for (var i = 0; i < 12; i++)
            {
                var tool = context.Factory.CreateTool();
                tool.Factory = context.Factory;
                tools.Add(tool);

                var item = new ToolTabStripItem
                {
                    DataContext = tool,
                    Width = 120
                };
                tabStrip.Items.Add(item);
            }

            var targetItem = (ToolTabStripItem)tabStrip.Items[0]!;

            var window = new Window { Width = 220, Height = 100, Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var startPoint = new Point(targetItem.Bounds.Width / 2, targetItem.Bounds.Height / 2);
            var dragStart = new Point(
                startPoint.X + DockSettings.MinimumHorizontalDragDistance + 20,
                startPoint.Y);

            RaisePointerPressed(targetItem, startPoint, MouseButton.Left);
            RaisePointerMoved(targetItem, dragStart, leftPressed: true);
            DrainDispatcher();

            var edgePoint = new Point(-10, startPoint.Y);
            RaisePointerMoved(targetItem, edgePoint, leftPressed: true);
            DrainDispatcher();
            RaisePointerReleased(targetItem, edgePoint, MouseButton.Left);
            DrainDispatcher();

            tabStrip.Items.Remove(targetItem);
            DrainDispatcher();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();
            ClearInputState(window);

            var itemRef = new WeakReference(targetItem);
            targetItem = null!;
            tabStrip = null!;

            return new AutoScrollLeakResult(itemRef, window, tools);
        });

        AssertCollected(result.ItemRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockablesKeepAlive);
    }

    [ReleaseFact]
    public void DocumentTabStripItem_AutoScroll_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var documents = new List<IDockable>();

            var tabStrip = new DocumentTabStrip
            {
                Width = 180,
                Height = 30
            };

            for (var i = 0; i < 12; i++)
            {
                var document = context.Factory.CreateDocument();
                document.Factory = context.Factory;
                documents.Add(document);

                var item = new DocumentTabStripItem
                {
                    DataContext = document,
                    Width = 120
                };
                tabStrip.Items.Add(item);
            }

            var targetItem = (DocumentTabStripItem)tabStrip.Items[0]!;

            var window = new Window { Width = 220, Height = 100, Content = tabStrip };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            tabStrip.ApplyTemplate();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            var startPoint = new Point(targetItem.Bounds.Width / 2, targetItem.Bounds.Height / 2);
            var dragStart = new Point(
                startPoint.X + DockSettings.MinimumHorizontalDragDistance + 20,
                startPoint.Y);

            RaisePointerPressed(targetItem, startPoint, MouseButton.Left);
            RaisePointerMoved(targetItem, dragStart, leftPressed: true);
            DrainDispatcher();

            var edgePoint = new Point(-10, startPoint.Y);
            RaisePointerMoved(targetItem, edgePoint, leftPressed: true);
            DrainDispatcher();
            RaisePointerReleased(targetItem, edgePoint, MouseButton.Left);
            DrainDispatcher();

            tabStrip.Items.Remove(targetItem);
            DrainDispatcher();
            tabStrip.UpdateLayout();
            DrainDispatcher();

            window.Content = new Border();
            DrainDispatcher();
            ClearInputState(window);

            var itemRef = new WeakReference(targetItem);
            targetItem = null!;
            tabStrip = null!;

            return new AutoScrollLeakResult(itemRef, window, documents);
        });

        AssertCollected(result.ItemRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.DockablesKeepAlive);
    }

    private sealed record AutoScrollLeakResult(
        WeakReference ItemRef,
        Window WindowKeepAlive,
        IReadOnlyList<IDockable> DockablesKeepAlive);
}
