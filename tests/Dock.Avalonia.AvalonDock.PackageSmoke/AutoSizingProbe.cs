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

internal static class AutoSizingProbe
{
    internal static void Verify(Window window, DockingManager manager)
    {
        foreach (Orientation orientation in new[] { Orientation.Horizontal, Orientation.Vertical })
        {
            bool horizontal = orientation == Orientation.Horizontal;
            Border content = new() { Width = 220, Height = 160 };
            LayoutAnchorablePane first = new(new LayoutAnchorable { Title = "A", ContentId = "auto-first", Content = content })
                { DockWidth = GridLength.Auto, DockHeight = GridLength.Auto };
            LayoutAnchorablePane second = Pane("B");
            manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel { Orientation = orientation, Children = { first, second } } };
            manager.GridSplitterWidth = manager.GridSplitterHeight = 12;
            Update();
            Control view = View(first);
            double natural = horizontal ? view.DesiredSize.Width : view.DesiredSize.Height;
            Check(view, natural);
            window.Width += 100; window.Height += 100;
            Update(); Check(view, natural);
            if (horizontal) content.Width += 60; else content.Height += 60;
            Update(); Check(view, natural + 60);
            if (horizontal) first.DockMinWidth = 400; else first.DockMinHeight = 400;
            Update(); Check(view, 400);
            if (Length(first) != GridLength.Auto) throw new Exception("Auto sizing overwrote the public length.");
            if (horizontal) first.DockMinWidth = 25; else first.DockMinHeight = 25;
            Update();
            using StringWriter xml = new();
            XmlLayoutSerializer serializer = new(manager);
            serializer.Serialize(xml);
            serializer.Deserialize(new StringReader(xml.ToString()));
            Update();
            first = (LayoutAnchorablePane)manager.Layout.RootPanel.Children[0];
            view = View(first);
            if (Length(first) != GridLength.Auto || !ReferenceEquals(content, first.Children[0].Content))
                throw new Exception("Packaged XML restoration lost Auto or its content identity.");
            Check(view, natural + 60);
            ProportionalStackPanelSplitter splitter = manager.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>().Single(control => control.IsEffectivelyVisible);
            Point start = splitter.TranslatePoint(new Point(splitter.Bounds.Width / 2, splitter.Bounds.Height / 2), window)!.Value;
            Vector delta = horizontal ? new Vector(40, 0) : new Vector(0, 40);
            window.MouseDown(start, MouseButton.Left); window.MouseMove(start + delta);
            Update();
            if (Length(first) != GridLength.Auto) throw new Exception("The packaged preview committed Auto before release.");
            window.MouseUp(start + delta, MouseButton.Left);
            Update(); Check(view, natural + 100);
            if (Length(first) != new GridLength(natural + 100)) throw new Exception("The packaged resize did not convert Auto to pixels.");

            LayoutAnchorablePane a = Pane("C"), b = Pane("D");
            a.DockWidth = a.DockHeight = new GridLength(3, GridUnitType.Star);
            LayoutPanel inner = new()
            {
                Orientation = orientation, DockWidth = GridLength.Auto, DockHeight = GridLength.Auto, Children = { a, b }
            };
            manager.Layout = new LayoutRoot { RootPanel = new LayoutPanel { Orientation = orientation, Children = { inner, Pane("E") } } };
            Update();
            Control aView = View(a), bView = View(b);
            double aNatural = horizontal ? aView.DesiredSize.Width : aView.DesiredSize.Height;
            double bNatural = horizontal ? bView.DesiredSize.Width : bView.DesiredSize.Height;
            Check(aView, aNatural); Check(bView, bNatural);
            Panel panel = aView.GetVisualAncestors().OfType<IProportionalSplitterHost>().First().Panel;
            Check(panel, aNatural + bNatural + 12);

            Control View(LayoutAnchorablePane pane) => manager.GetVisualDescendants().OfType<ToolDockControl>()
                .Single(control => ReferenceEquals(control.DataContext, pane.DockModel));
            GridLength Length(LayoutAnchorablePane pane) => horizontal ? pane.DockWidth : pane.DockHeight;
            void Check(Control control, double expected)
            {
                double actual = horizontal ? control.Bounds.Width : control.Bounds.Height;
                if (Math.Abs(actual - expected) > 1)
                    throw new Exception($"The packaged intrinsic pane rendered {actual}, expected {expected}.");
            }
        }
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private static LayoutAnchorablePane Pane(string title) => new(new LayoutAnchorable
    {
        Title = title, Content = new Border { Width = 220, Height = 160 }
    });
}
