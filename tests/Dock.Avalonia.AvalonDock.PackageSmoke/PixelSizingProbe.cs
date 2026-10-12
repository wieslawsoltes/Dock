using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Avalonia.Controls;
using Dock.Controls.ProportionalStackPanel;
using Orientation = Avalonia.Layout.Orientation;

namespace PackageConsumer;

internal static class PixelSizingProbe
{
    internal static void Verify(Window window, DockingManager manager)
    {
        foreach (Orientation orientation in new[] { Orientation.Horizontal, Orientation.Vertical })
        {
            bool horizontal = orientation == Orientation.Horizontal;
            LayoutAnchorablePane first = new(new LayoutAnchorable { Title = "First", ContentId = "pixel-first" })
                { DockWidth = new GridLength(200), DockHeight = new GridLength(200) };
            LayoutAnchorablePane second = new(new LayoutAnchorable { Title = "Second", ContentId = "pixel-second" })
                { DockWidth = new GridLength(300), DockHeight = new GridLength(300) };
            manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel { Orientation = orientation, Children = { first, second } } };
            manager.GridSplitterWidth = manager.GridSplitterHeight = 24;
            Update();
            if (first.DockMinWidth != 25 || first.DockMinHeight != 25)
                throw new Exception("Packaged panes did not use the public minimum-size defaults.");
            ToolDockControl firstView = Pane(first);
            ToolDockControl secondView = Pane(second);
            CheckExtent(firstView, 200); CheckExtent(secondView, 300);
            window.Width += 100; window.Height += 100;
            Update();
            CheckExtent(firstView, 200); CheckExtent(secondView, 300);
            ProportionalStackPanelSplitter splitter = manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().Single(control => control.IsEffectivelyVisible);
            if (!splitter.PreviewResize)
                throw new Exception("The packaged native splitter did not enable preview resizing.");
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            Vector delta = horizontal ? new Vector(40, 0) : new Vector(0, 40);
            window.MouseDown(start, MouseButton.Left); window.MouseMove(start + delta);
            Update();
            CheckExtent(firstView, 200);
            CheckLengths(200, 300);
            window.MouseUp(start + delta, MouseButton.Left);
            Update();
            CheckLengths(240, 260);
            CheckExtent(firstView, 240); CheckExtent(secondView, 260);
            if (horizontal) first.DockMinWidth = 280; else first.DockMinHeight = 280;
            Update();
            CheckLengths(240, 260);
            CheckExtent(firstView, 280); CheckExtent(secondView, 260);
            using StringWriter xml = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.Serialize(xml);
            serializer.Deserialize(new StringReader(xml.ToString()));
            Update();
            first = (LayoutAnchorablePane)manager.Layout.RootPanel.Children[0];
            second = (LayoutAnchorablePane)manager.Layout.RootPanel.Children[1];
            CheckLengths(240, 260);
            CheckExtent(Pane(first), 280); CheckExtent(Pane(second), 260);

            ToolDockControl Pane(LayoutAnchorablePane pane) => manager.GetVisualDescendants().OfType<ToolDockControl>()
                .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
            void CheckExtent(Control control, double expected)
            {
                double actual = horizontal ? control.Bounds.Width : control.Bounds.Height;
                if (Math.Abs(actual - expected) > 1)
                    throw new Exception($"The packaged fixed pane rendered {actual}, expected {expected}.");
            }
            void CheckLengths(double a, double b)
            {
                if ((horizontal ? first.DockWidth : first.DockHeight) != new GridLength(a) ||
                    (horizontal ? second.DockWidth : second.DockHeight) != new GridLength(b))
                    throw new Exception("The packaged splitter did not preserve the public pixel length units.");
            }
        }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }
}
