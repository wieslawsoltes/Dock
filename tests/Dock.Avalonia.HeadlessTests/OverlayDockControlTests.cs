using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Avalonia.Themes.Fluent;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class OverlayDockControlTests
{
    private sealed class TestFactory : Factory
    {
        public bool FloatCalled { get; private set; }

        public override void FloatDockable(IDockable dockable)
        {
            FloatCalled = true;
        }
    }

    [AvaloniaFact]
    public void OverlayDockControl_Has_Default_DataTemplates()
    {
        var templates = Dock.Avalonia.Internal.DockDataTemplateHelper.CreateDefaultDataTemplates().ToList();

        Assert.Contains(templates, t => t.Match(new OverlayDock()));
        Assert.Contains(templates, t => t.Match(new OverlayPanel()));
        Assert.Contains(templates, t => t.Match(new OverlaySplitterGroup()));
        Assert.Contains(templates, t => t.Match(new OverlaySplitter()));
    }

    [AvaloniaFact]
    public void OverlayPanel_Move_Updates_Position()
    {
        var overlayDock = new OverlayDock
        {
            AllowPanelDragging = true
        };

        var panel = new OverlayPanel
        {
            X = 10,
            Y = 20,
            Width = 200,
            Height = 120,
            Owner = overlayDock,
            ShowHeader = true
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { panel };

        var control = new OverlayPanelControl { DataContext = panel };

        var moveHandler = typeof(OverlayPanelControl).GetMethod("MoveThumbOnDragDelta", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(moveHandler);
        var args = new VectorEventArgs
        {
            RoutedEvent = Thumb.DragDeltaEvent,
            Vector = new Vector(15, 5)
        };

        moveHandler!.Invoke(control, new object?[] { null, args });

        Assert.Equal(25, panel.X);
        Assert.Equal(25, panel.Y);
    }

    [AvaloniaFact]
    public void OverlayPanel_BringToFront_NormalizesZIndex()
    {
        var overlayDock = new OverlayDock
        {
            AllowPanelDragging = true
        };

        var bottomPanel = new OverlayPanel
        {
            X = 0,
            Y = 0,
            ZIndex = 5,
            Owner = overlayDock
        };

        var topPanel = new OverlayPanel
        {
            X = 10,
            Y = 10,
            ZIndex = 0,
            Owner = overlayDock
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { bottomPanel, topPanel };

        var control = new OverlayPanelControl { DataContext = topPanel };

        var moveHandler = typeof(OverlayPanelControl).GetMethod("MoveThumbOnDragDelta", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(moveHandler);

        var args = new VectorEventArgs
        {
            RoutedEvent = Thumb.DragDeltaEvent,
            Vector = new Vector(1, 0)
        };

        moveHandler!.Invoke(control, new object?[] { null, args });

        Assert.True(topPanel.ZIndex > bottomPanel.ZIndex);
        Assert.Equal(1, bottomPanel.ZIndex);
        Assert.Equal(2, topPanel.ZIndex);
    }

    [AvaloniaFact]
    public void OverlayPanel_Resize_Updates_Size()
    {
        var overlayDock = new OverlayDock
        {
            AllowPanelResizing = true
        };

        var panel = new OverlayPanel
        {
            X = 0,
            Y = 0,
            Width = 120,
            Height = 100,
            Owner = overlayDock
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { panel };

        var control = new OverlayPanelControl { DataContext = panel };

        var resizeHandler = typeof(OverlayPanelControl).GetMethod("ResizeThumbOnDragDelta", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(resizeHandler);
        var args = new VectorEventArgs
        {
            RoutedEvent = Thumb.DragDeltaEvent,
            Vector = new Vector(10, 8)
        };

        resizeHandler!.Invoke(control, new object?[] { null, args });

        Assert.Equal(130, panel.Width);
        Assert.Equal(108, panel.Height);
    }

    [AvaloniaFact]
    public void OverlaySplitter_Resizes_Panels()
    {
        var before = new OverlayPanel { Width = 150, Height = 100 };
        var after = new OverlayPanel { Width = 150, Height = 100, X = 150 };

        var splitter = new OverlaySplitter
        {
            Orientation = Orientation.Horizontal,
            CanResize = true,
            PanelBefore = before,
            PanelAfter = after,
            MinSizeBefore = 50,
            MinSizeAfter = 50
        };

        var control = new OverlaySplitterControl { DataContext = splitter };

        var handler = typeof(OverlaySplitterControl).GetMethod(
            "OnDragDelta",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(object), typeof(VectorEventArgs) },
            modifiers: null);
        Assert.NotNull(handler);
        var args = new VectorEventArgs
        {
            RoutedEvent = Thumb.DragDeltaEvent,
            Vector = new Vector(20, 0)
        };

        handler!.Invoke(control, new object?[] { null, args });

        Assert.Equal(170, before.Width);
        Assert.Equal(130, after.Width);
        Assert.Equal(170, after.X);
    }

    [AvaloniaFact]
    public void OverlayPanel_SnapToEdge_UsesCanvasAncestor()
    {
        var overlayDock = new OverlayDock
        {
            AllowPanelDragging = true,
            EnableSnapToEdge = true,
            EnableSnapToPanel = false,
            SnapThreshold = 8
        };

        var panel = new OverlayPanel
        {
            X = 485,
            Y = 10,
            Width = 20,
            Height = 40,
            Owner = overlayDock
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { panel };

        var control = new OverlayPanelControl { DataContext = panel };

        var canvas = new Canvas { Width = 500, Height = 400 };
        canvas.Children.Add(control);
        canvas.Measure(new Size(500, 400));
        canvas.Arrange(new Rect(0, 0, 500, 400));

        var applySnap = typeof(OverlayPanelControl).GetMethod(
            "ApplySnap",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(IOverlayDock), typeof(IOverlayPanel) },
            modifiers: null);

        Assert.NotNull(applySnap);

        applySnap!.Invoke(control, new object?[] { overlayDock, panel });

        Assert.Equal(480, panel.X);
        Assert.Equal(10, panel.Y);
    }

    [AvaloniaFact]
    public void OverlayPanel_SnapToPanel_AlignsAdjacentPanels()
    {
        var overlayDock = new OverlayDock
        {
            AllowPanelDragging = true,
            EnableSnapToEdge = false,
            EnableSnapToPanel = true,
            SnapThreshold = 6
        };

        var neighbor = new OverlayPanel
        {
            X = 100,
            Y = 20,
            Width = 100,
            Height = 80,
            Owner = overlayDock
        };

        var panel = new OverlayPanel
        {
            X = 205,
            Y = 20,
            Width = 60,
            Height = 80,
            Owner = overlayDock
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { neighbor, panel };

        var control = new OverlayPanelControl { DataContext = panel };

        var canvas = new Canvas { Width = 500, Height = 400 };
        canvas.Children.Add(control);
        canvas.Measure(new Size(500, 400));
        canvas.Arrange(new Rect(0, 0, 500, 400));

        var applySnap = typeof(OverlayPanelControl).GetMethod(
            "ApplySnap",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(IOverlayDock), typeof(IOverlayPanel) },
            modifiers: null);

        Assert.NotNull(applySnap);

        applySnap!.Invoke(control, new object?[] { overlayDock, panel });

        Assert.Equal(200, panel.X);
        Assert.Equal(20, panel.Y);
    }

    [AvaloniaFact]
    public void OverlayPanel_DoubleClick_Respects_FloatFlags()
    {
        var factory = new TestFactory();
        var overlayDock = new OverlayDock { Factory = factory };
        var panel = new OverlayPanel
        {
            Owner = overlayDock,
            FloatOnDoubleClick = false,
            CanFloat = true
        };

        var control = new OverlayPanelControl { DataContext = panel };

        var handler = typeof(OverlayPanelControl).GetMethod(
            "MoveThumbOnDoubleTapped",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(handler);

        handler!.Invoke(control, new object?[] { null, new RoutedEventArgs() });
        Assert.False(factory.FloatCalled);

        panel.FloatOnDoubleClick = true;
        panel.CanFloat = false;

        handler.Invoke(control, new object?[] { null, new RoutedEventArgs() });
        Assert.False(factory.FloatCalled);
    }

    [AvaloniaFact]
    public void OverlayPanel_DoubleClick_Floats_When_Allowed()
    {
        var factory = new TestFactory();
        var overlayDock = new OverlayDock { Factory = factory };
        var panel = new OverlayPanel
        {
            Owner = overlayDock,
            FloatOnDoubleClick = true,
            CanFloat = true
        };

        var control = new OverlayPanelControl { DataContext = panel };

        var handler = typeof(OverlayPanelControl).GetMethod(
            "MoveThumbOnDoubleTapped",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(handler);

        handler!.Invoke(control, new object?[] { null, new RoutedEventArgs() });
        Assert.True(factory.FloatCalled);
    }

    [AvaloniaFact]
    public void OverlayDockControl_AnchoredPanel_UpdatesPosition()
    {
        var overlayDock = new OverlayDock();
        var panel = new OverlayPanel
        {
            Width = 120,
            Height = 80,
            Anchor = OverlayAnchor.TopRight,
            AnchorOffsetX = -10,
            AnchorOffsetY = 5
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { panel };

        var control = new OverlayDockControl
        {
            Width = 400,
            Height = 300,
            DataContext = overlayDock
        };
        control.Styles.Add(new DockFluentTheme());

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = control
        };

        window.Show();

        control.Measure(new Size(400, 300));
        control.Arrange(new Rect(0, 0, 400, 300));

        Assert.InRange(panel.X, 269.5, 270.5);
        Assert.InRange(panel.Y, 4.5, 5.5);
    }

    [AvaloniaFact]
    public void OverlayDockControl_AnchoredGroup_UpdatesPosition()
    {
        var overlayDock = new OverlayDock();
        var group = new OverlaySplitterGroup
        {
            Width = 200,
            Height = 100,
            Anchor = OverlayAnchor.Center
        };

        overlayDock.SplitterGroups = new List<IOverlaySplitterGroup> { group };

        var control = new OverlayDockControl
        {
            Width = 400,
            Height = 300,
            DataContext = overlayDock
        };
        control.Styles.Add(new DockFluentTheme());

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = control
        };

        window.Show();

        control.Measure(new Size(400, 300));
        control.Arrange(new Rect(0, 0, 400, 300));

        Assert.InRange(group.X, 99.5, 100.5);
        Assert.InRange(group.Y, 99.5, 100.5);
    }

    [AvaloniaFact]
    public void OverlayDockControl_EdgeDock_Left_SpansOverlay()
    {
        var overlayDock = new OverlayDock();
        var group = new OverlaySplitterGroup
        {
            Width = 120,
            Height = 80,
            EdgeDock = OverlayEdgeDock.Left
        };

        overlayDock.SplitterGroups = new List<IOverlaySplitterGroup> { group };

        var control = new OverlayDockControl
        {
            Width = 400,
            Height = 300,
            DataContext = overlayDock
        };
        control.Styles.Add(new DockFluentTheme());

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = control
        };

        window.Show();

        control.Measure(new Size(400, 300));
        control.Arrange(new Rect(0, 0, 400, 300));

        Assert.InRange(group.X, -0.5, 0.5);
        Assert.InRange(group.Y, -0.5, 0.5);
        Assert.InRange(group.Width, 119.5, 120.5);
        Assert.InRange(group.Height, 299.5, 300.5);
    }

    [AvaloniaFact]
    public void OverlayDockControl_GroupedPanel_NotDuplicatedInOverlayPanels()
    {
        var freePanel = new OverlayPanel { Id = "FreePanel" };
        var groupedPanel = new OverlayPanel { Id = "GroupedPanel" };
        var group = new OverlaySplitterGroup
        {
            Panels = new List<IOverlayPanel> { groupedPanel }
        };

        var overlayDock = new OverlayDock
        {
            SplitterGroups = new List<IOverlaySplitterGroup> { group }
        };

        overlayDock.OverlayPanels = new List<IOverlayPanel> { freePanel, groupedPanel };

        var control = new OverlayDockControl
        {
            Width = 400,
            Height = 300,
            DataContext = overlayDock
        };
        control.Styles.Add(new DockFluentTheme());

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = control
        };

        window.Show();

        control.Measure(new Size(400, 300));
        control.Arrange(new Rect(0, 0, 400, 300));

        var panelsControl = control.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(items => items.Name == "PART_OverlayPanels");
        Assert.NotNull(panelsControl);

        var items = panelsControl!.Items.Cast<object>().ToList();
        Assert.Contains(freePanel, items);
        Assert.DoesNotContain(groupedPanel, items);
    }

    [AvaloniaFact]
    public void OverlaySplitterGroup_Interleaves_Panels_And_Splitters()
    {
        var panel1 = new OverlayPanel { Proportion = 0.4 };
        var panel2 = new OverlayPanel { Proportion = 0.3 };
        var panel3 = new OverlayPanel { Proportion = 0.3 };

        var splitter1 = new OverlaySplitter { Orientation = Orientation.Horizontal };
        var splitter2 = new OverlaySplitter { Orientation = Orientation.Horizontal };

        var group = new OverlaySplitterGroup
        {
            Orientation = Orientation.Horizontal,
            Panels = new List<IOverlayPanel> { panel1, panel2, panel3 },
            Splitters = new List<IOverlaySplitter> { splitter1, splitter2 }
        };

        var control = new OverlaySplitterGroupControl { DataContext = group };
        control.Styles.Add(new DockFluentTheme());

        var window = new Window
        {
            Width = 300,
            Height = 200,
            Content = control
        };

        window.Show();

        var itemsControl = control.GetVisualDescendants()
            .OfType<ItemsControl>()
            .FirstOrDefault(items => items.Name == "PART_ItemsHost");
        Assert.NotNull(itemsControl);

        var items = itemsControl!.Items.Cast<object>().ToList();
        Assert.Equal(new object[] { panel1, splitter1, panel2, splitter2, panel3 }, items);
    }

    [AvaloniaFact]
    public void OverlaySplitterGroup_Layout_RespectsProportions_And_SplitterThickness()
    {
        var panel1 = new OverlayPanel { Proportion = 0.25 };
        var panel2 = new OverlayPanel { Proportion = 0.75 };
        var splitter = new OverlaySplitter { Orientation = Orientation.Horizontal, Thickness = 8 };

        var group = new OverlaySplitterGroup
        {
            Orientation = Orientation.Horizontal,
            Panels = new List<IOverlayPanel> { panel1, panel2 },
            Splitters = new List<IOverlaySplitter> { splitter }
        };

        var control = new OverlaySplitterGroupControl
        {
            Width = 400,
            Height = 200,
            DataContext = group
        };
        control.Styles.Add(new DockFluentTheme());
        foreach (var template in DockDataTemplateHelper.CreateDefaultDataTemplates())
        {
            control.DataTemplates.Add(template);
        }

        var window = new Window
        {
            Width = 400,
            Height = 200,
            Content = control
        };

        window.Show();

        control.Measure(new Size(400, 200));
        control.Arrange(new Rect(0, 0, 400, 200));

        var panel = control.GetVisualDescendants()
            .OfType<ProportionalStackPanel>()
            .FirstOrDefault();
        Assert.NotNull(panel);

        var children = panel!.Children.ToList();
        Assert.Equal(3, children.Count);

        var availableWidth = 400 - splitter.Thickness;
        var expectedFirst = availableWidth * 0.25;
        var expectedSecond = availableWidth * 0.75;

        Assert.InRange(children[0].Bounds.Width, expectedFirst - 1, expectedFirst + 1);
        Assert.InRange(children[1].Bounds.Width, splitter.Thickness - 0.5, splitter.Thickness + 0.5);
        Assert.InRange(children[2].Bounds.Width, expectedSecond - 1, expectedSecond + 1);
    }

    [AvaloniaFact]
    public void DockManagerState_OverlayDrop_UsesDropControlCoordinates()
    {
        var manager = new DockManager(new DockService());
        var state = new DockControlState(manager, new DefaultDragOffsetCalculator());
        var overlayDock = new OverlayDock();

        var root = new Canvas
        {
            Width = 400,
            Height = 300
        };

        var dropControl = new OverlayDockControl
        {
            Width = 200,
            Height = 150,
            DataContext = overlayDock
        };

        Canvas.SetLeft(dropControl, 100);
        Canvas.SetTop(dropControl, 80);
        root.Children.Add(dropControl);

        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = root
        };

        window.Show();

        root.Measure(new Size(400, 300));
        root.Arrange(new Rect(0, 0, 400, 300));

        var dropControlProperty = typeof(DockManagerState)
            .GetProperty("DropControl", BindingFlags.Instance | BindingFlags.NonPublic);
        dropControlProperty!.SetValue(state, dropControl);

        var getDockPoint = typeof(DockManagerState)
            .GetMethod("GetDockPoint", BindingFlags.Instance | BindingFlags.NonPublic);

        var point = new Point(150, 100);
        var dockPoint = (DockPoint)getDockPoint!.Invoke(state, new object?[] { point, root, overlayDock })!;

        Assert.Equal(50, dockPoint.X);
        Assert.Equal(20, dockPoint.Y);
    }
}
