using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Tab_Drops_Use_Public_Overlay_Windows_And_The_Existing_Dock_Models(bool tool)
    {
        bool previous = DockSettings.UseFloatingDockAdorner;
        DockSettings.UseFloatingDockAdorner = true;
        try
        {
            using Workspace workspace = new(tool);
            LayoutContent destination = tool ? new LayoutAnchorable { Title = "Destination", Content = "Target tool" }
                : new LayoutDocument { Title = "Destination", Content = "Target document" };
            ILayoutPane pane = tool ? new LayoutAnchorablePane((LayoutAnchorable)destination) : new LayoutDocumentPane(destination);
            workspace.Manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)pane);
            RecordingOverlayFactory factory = new(DockAdornerWindow.GetFactory(workspace.Manager)!);
            DockAdornerWindow.SetFactory(workspace.Manager, factory);
            workspace.Update();
            Control sourceTab = NativeTab(workspace.Manager, workspace.First, tool);
            Control targetPane = workspace.Manager.GetVisualDescendants().OfType<Control>().Single(control =>
                control is LayoutAnchorablePaneControl anchorable && ReferenceEquals(anchorable.Model, pane) ||
                control is LayoutDocumentPaneControl document && ReferenceEquals(document.Model, pane));
            Point start = Center(sourceTab);
            Point destinationPoint = targetPane.TranslatePoint(new Point(targetPane.Bounds.Width / 2, targetPane.Bounds.Height / 2), workspace.Window)!.Value;
            workspace.Window.MouseMove(start);
            workspace.Window.MouseDown(start, MouseButton.Left);
            workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseMove(start + new Vector(0, tool ? -80 : 80), RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseMove(destinationPoint, RawInputModifiers.LeftMouseButton);
            workspace.Update();
            Assert.True(workspace.Manager.GetVisualDescendants().OfType<DockControl>().Single().IsDraggingDock);
            OverlayWindow overlay = factory.Windows.Last(window => window.IsVisible && window.Content is DockTarget);
            Assert.Equal(tool ? DropAreaType.AnchorablePane : DropAreaType.DocumentPane, overlay.DropArea!.Type);
            DockTarget native = Assert.IsType<DockTarget>(overlay.Content);
            native.UpdateLayout();
            Control center = native.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_CenterSelector");
            PixelPoint screen = center.PointToScreen(new Point(center.Bounds.Width / 2, center.Bounds.Height / 2));
            Point drop = workspace.Window.PointToClient(screen);
            workspace.Window.MouseMove(drop, RawInputModifiers.LeftMouseButton);
            workspace.Update();
            Assert.Contains(overlay.DropTargets, target => target.Type == (tool
                ? OverlayWindowDropTargetType.AnchorablePaneDockInside : OverlayWindowDropTargetType.DocumentPaneDockInside));
            workspace.Window.MouseUp(drop, MouseButton.Left);
            workspace.Update();
            Assert.Same(pane, workspace.First.Parent);
            Assert.Same(destination.DockModel.Owner, workspace.First.DockModel.Owner);
            Assert.False(workspace.First.IsFloating);
            Assert.All(factory.Windows, window =>
            {
                Assert.False(window.IsVisible);
                Assert.Null(window.DropArea);
                Assert.Null(window.Content);
            });
        }
        finally { DockSettings.UseFloatingDockAdorner = previous; }
    }

    private sealed class RecordingOverlayFactory(IDockAdornerWindowFactory inner) : IDockAdornerWindowFactory
    {
        public List<OverlayWindow> Windows { get; } = new();
        public DockAdornerWindow? CreateWindow(Visual adornedElement, Control adorner)
        {
            OverlayWindow window = Assert.IsType<OverlayWindow>(inner.CreateWindow(adornedElement, adorner));
            Windows.Add(window);
            return window;
        }
    }
}
