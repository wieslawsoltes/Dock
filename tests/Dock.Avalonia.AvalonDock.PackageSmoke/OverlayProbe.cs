using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class OverlayProbe
{
    internal static void Verify()
    {
        DockingManager manager = new()
        {
            Layout = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Title = "Editor" })) }
        };
        Window owner = new() { Width = 600, Height = 400 };
        owner.Styles.Add(new AvalonDockTheme());
        owner.Content = manager;
        owner.Show(); owner.UpdateLayout(); Dispatcher.UIThread.RunJobs();
        OverlayWindow? overlay = null;
        try
        {
            LayoutDocumentPaneControl pane = manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>().Single();
            DockTarget target = new();
            IDockAdornerWindowFactory factory = DockAdornerWindow.GetFactory(pane)!;
            overlay = (OverlayWindow)factory.CreateWindow(pane, target)!;
            overlay.Content = target;
            overlay.Width = pane.Bounds.Width; overlay.Height = pane.Bounds.Height;
            overlay.Position = pane.PointToScreen(default);
            overlay.SizeToContent = SizeToContent.Manual;
            overlay.Show(owner); overlay.UpdateLayout(); Dispatcher.UIThread.RunJobs();
            ConsumerWindow resources = new();
            foreach (string key in new[] { "Overlay", "LegacyOverlay" })
            {
                overlay.Theme = (ControlTheme)resources.Resources[key]!;
                overlay.OnApplyTemplate(); overlay.UpdateLayout();
                if (Convert.ToInt32(overlay.Tag) != 8)
                    throw new Exception("The packaged overlay theme or selector enum did not compile in both XML namespaces.");
            }
            if (overlay.DropArea is not DropArea<LayoutDocumentPaneControl> { Type: DropAreaType.DocumentPane } area ||
                !ReferenceEquals(area.AreaElement, pane) || overlay.Area is not DocumentPaneControlOverlayArea)
                throw new Exception("The packaged overlay did not project its existing native pane.");
            Control center = target.GetSelector(DockOperation.Fill)
                ?? throw new Exception("The native target did not expose its registered center selector.");
            if (!ReferenceEquals(TopLevel.GetTopLevel(target), overlay) || !ReferenceEquals(TopLevel.GetTopLevel(center), overlay))
                throw new Exception("Switching packaged overlay themes detached the native target or its selector.");
            PixelPoint point = center.PointToScreen(new Point(center.Bounds.Width / 2, center.Bounds.Height / 2));
            if (target.GetDockOperation(pane.PointToClient(point), pane, pane, DragAction.Move, (_, _, _, _) => true) != DockOperation.Fill ||
                !overlay.DropTargets.Any(item => item.TargetType == DropTargetType.DocumentPaneDockInside &&
                    item.ScreenDetectionArea.Contains(new Point(point.X, point.Y))))
                throw new Exception("The public overlay geometry disagreed with native Dock hit testing.");
            overlay.Close();
            if (overlay.DropArea is not null || overlay.Content is not null || overlay.DropTargets.Count != 0)
                throw new Exception("Closing the overlay did not release its native presentation references.");
        }
        finally { overlay?.Close(); owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }
}
