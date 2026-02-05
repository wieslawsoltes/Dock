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
public class ToolPinItemLeakTests
{
    [ReleaseFact]
    public void ToolPinItemControl_DetachWhileWindowAlive_DoesNotLeak()
    {
        var result = RunInSession(() =>
        {
            var context = LeakContext.Create();
            var items = context.Factory.CreateList<IDockable>(context.Tool);

            var pinnedControl = new ToolPinnedControl
            {
                ItemsSource = items
            };

            var window = new Window { Content = pinnedControl };
            window.Styles.Add(new FluentTheme());
            window.Styles.Add(new DockFluentTheme());

            ShowWindow(window);
            pinnedControl.ApplyTemplate();
            pinnedControl.UpdateLayout();
            DrainDispatcher();

            var pinItem = pinnedControl.ContainerFromIndex(0) as ToolPinItemControl
                          ?? FindVisualDescendant<ToolPinItemControl>(pinnedControl);
            Assert.NotNull(pinItem);

            var startPoint = new Point(pinItem!.Bounds.Width / 2, pinItem.Bounds.Height / 2);
            var dragPoint = new Point(
                startPoint.X + DockSettings.MinimumHorizontalDragDistance + 20,
                startPoint.Y + DockSettings.MinimumVerticalDragDistance + 20);

            RaisePointerPressed(pinItem, startPoint, MouseButton.Left);
            RaisePointerMoved(pinItem, dragPoint, leftPressed: true);
            DrainDispatcher();

            items.Remove(context.Tool);
            DrainDispatcher();
            ClearInputState(window);

            var itemRef = new WeakReference(pinItem);
            pinItem = null;

            return new ToolPinItemLeakResult(itemRef, window, pinnedControl, context.Tool, items);
        });

        AssertCollected(result.ItemRef);
        GC.KeepAlive(result.WindowKeepAlive);
        GC.KeepAlive(result.PinnedControlKeepAlive);
        GC.KeepAlive(result.ToolKeepAlive);
        GC.KeepAlive(result.ItemsKeepAlive);
    }

    private sealed record ToolPinItemLeakResult(
        WeakReference ItemRef,
        Window WindowKeepAlive,
        ToolPinnedControl PinnedControlKeepAlive,
        IDockable ToolKeepAlive,
        IList<IDockable> ItemsKeepAlive);
}
