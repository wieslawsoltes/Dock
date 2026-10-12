using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTabSourcePreviewTests
{
    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(false, 4)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 3)]
    [InlineData(true, 4)]
    public void Tab_preview_hides_source_reveals_remaining_tab_and_restores(bool document, int ending)
    {
        var factory = new Factory();
        IDock Pane(string title)
        {
            IDock pane = document ? factory.CreateDocumentDock() : factory.CreateToolDock();
            IDockable item = document ? factory.CreateDocument() : factory.CreateTool();
            item.Title = title; item.CanFloat = false;
            pane.VisibleDockables = factory.CreateList<IDockable>(item); pane.ActiveDockable = item;
            return pane;
        }
        var originalPane = Pane("Orderbook");
        var remainingPane = Pane("Chart");
        var target = Pane("Target");
        var source = originalPane.ActiveDockable!;
        var remaining = remainingPane.ActiveDockable!;
        var row = factory.CreateProportionalDock();
        row.VisibleDockables = factory.CreateList<IDockable>(originalPane, factory.CreateProportionalDockSplitter(), remainingPane,
            factory.CreateProportionalDockSplitter(), target);
        var root = factory.CreateRootDock(); root.VisibleDockables = factory.CreateList<IDockable>(row); root.ActiveDockable = row;
        factory.InitLayout(root);
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        var state = (DockControlState)host.DockControlState;
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            // The first move vacates a standalone pane, then tabs it into Chart.
            using (var first = DockDragPreviewLayout.TryCreate(source, host)) Assert.NotNull(first);
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, remainingPane, DragAction.Move, DockOperation.Fill, true));
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            Assert.Equal(2, remainingPane.VisibleDockables!.Count);
            Assert.Same(source, remainingPane.ActiveDockable);
            var paneControl = (Control)factory.VisibleDockableControls[remainingPane];
            var originalBounds = paneControl.Bounds;
            var members = remainingPane.VisibleDockables.ToArray();
            var events = 0;
            factory.DockableRemoved += (_, _) => events++;
            factory.DockableAdded += (_, _) => events++;
            factory.DockableClosed += (_, _) => events++;
            for (var repeat = 0; repeat < 2; repeat++)
            {
                var tab = host.GetVisualDescendants().OfType<Control>().Single(c =>
                    c is ToolTabStripItem or DocumentTabStripItem && ReferenceEquals(c.DataContext, source));
                state.StartDrag(tab, default, new global::Avalonia.Point(500, 300), host);
                Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
                Assert.False(tab.IsVisible);
                Assert.Same(remaining, remainingPane.ActiveDockable);
                Assert.Same(remainingPane, source.Owner);
                Assert.Equal(members, remainingPane.VisibleDockables);
                Assert.Equal(originalBounds, paneControl.Bounds);
                Assert.True(paneControl.IsEffectivelyVisible);
                Assert.Equal(0, events);
                if (ending == 0) window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
                else if (ending == 1) state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
                else if (ending == 2) state.Process(default, default, EventType.Moved, DragAction.Copy, host, new List<IDockControl> { host });
                else if (ending == 3) { window.Content = null; }
                else { host.IsDockingEnabled = false; state.Process(default, default, EventType.Moved, DragAction.Move, host, new List<IDockControl> { host }); }
                window.UpdateLayout();
                Assert.True(tab.IsVisible);
                Assert.Same(source, remainingPane.ActiveDockable);
                Assert.Equal(members, remainingPane.VisibleDockables);
                Assert.Equal(0, events);
                state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
                host.IsDockingEnabled = true;
                if (ending == 3) { window.Content = host; Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
            }
        }
        finally
        {
            state.Process(default, default, EventType.CaptureLost, DragAction.None, host, new List<IDockControl> { host });
            window.Close();
        }
    }
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Tab_preview_preserves_inactive_or_application_changed_selection(bool inactive, bool externalSelection)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.4, 0.3, 0.3);
        var pane = panes[0];
        pane.IsCollapsable = false; // A tab can leave even when its group cannot collapse.
        var source = pane.ActiveDockable!;
        var remaining = factory.CreateTool();
        var third = factory.CreateTool();
        factory.AddDockable(pane, remaining);
        factory.AddDockable(pane, third);
        if (inactive) pane.ActiveDockable = remaining;
        var original = pane.ActiveDockable;
        var host = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = host };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            var tab = host.GetVisualDescendants().OfType<ToolTabStripItem>().Single(c => ReferenceEquals(c.DataContext, source));
            using var preview = DockDragPreviewLayout.TryCreate(source, host);
            Assert.NotNull(preview);
            Assert.False(tab.IsVisible);
            Assert.Same(remaining, pane.ActiveDockable);
            if (externalSelection) pane.ActiveDockable = third;
            preview!.Dispose();
            Assert.True(tab.IsVisible);
            Assert.Same(externalSelection ? third : original, pane.ActiveDockable);
            Assert.Same(pane, source.Owner);
            Assert.Equal(3, pane.VisibleDockables!.Count);
        }
        finally { window.Close(); }
    }

}
