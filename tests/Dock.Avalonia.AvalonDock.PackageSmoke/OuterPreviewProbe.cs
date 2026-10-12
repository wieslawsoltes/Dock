using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace PackageConsumer;

internal static class OuterPreviewProbe
{
    internal static void Verify()
    {
        bool oldFloating = DockSettings.UseFloatingDockAdorner;
        DockSettings.UseFloatingDockAdorner = false;
        LayoutAnchorable tool = new() { Title = "Tool", Content = "Tool", CanDockAsTabbedDocument = false };
        LayoutAnchorablePane tools = new(tool) { Children = { new LayoutAnchorable { Title = "Sibling" } } };
        LayoutDocumentPane destination = new(new LayoutDocument { Title = "Editor", Content = "Document" });
        LayoutDocumentPaneGroup region = new(new LayoutDocumentPane(new LayoutDocument { Title = "Other" }))
            { DockWidth = new GridLength(5, GridUnitType.Star), Children = { new LayoutDocumentPaneGroup(destination) } };
        DockingManager manager = new() { AllowMixedOrientation = true,
            RenderTransformOrigin = RelativePoint.TopLeft, RenderTransform = new ScaleTransform(0.75, 0.75),
            Layout = new() { RootPanel = new() { Children = { tools, region } } } };
        Window window = new() { Width = 1800, Height = 1000, Content = manager };
        window.Styles.Add(new AvalonDockTheme());
        try
        {
            window.Show(); Update();
            LayoutDocumentPaneControl pane = manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>()
                .Single(control => ReferenceEquals(control.Model, destination));
            Control surface = pane.GetVisualDescendants().OfType<Control>().First(control =>
                DockProperties.GetIsDockTarget(control) && ReferenceEquals(control.DataContext, destination.DockModel));
            Control host = DockTarget.GetAdornerHostProvider(surface)?.GetAdornerHost(surface)
                ?? throw new Exception("The packaged compatibility theme did not supply its preview host provider.");
            if (host is not ILayoutControl view || !ReferenceEquals(view.Model, region))
                throw new Exception("The packaged preview did not resolve the highest document group.");
            LayoutAnchorableTabItem tab = manager.GetVisualDescendants().OfType<LayoutAnchorableTabItem>()
                .Single(control => ReferenceEquals(control.Model, tool));
            Point start = Center(tab);
            window.MouseMove(start); window.MouseDown(start, MouseButton.Left);
            window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            window.MouseMove(start + new Vector(0, -80), RawInputModifiers.LeftMouseButton);
            window.MouseMove(Center(surface), RawInputModifiers.LeftMouseButton); Update();
            DockTarget target = window.GetVisualDescendants().OfType<DockTarget>().Single();
            Control selector = target.GetSelector(DockOperation.OuterLeft)
                ?? throw new Exception("The packaged outer selector is missing.");
            window.MouseMove(Center(selector), RawInputModifiers.LeftMouseButton); Update();
            Control indicator = target.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_OuterLeftIndicator");
            CheckPreview();
            window.Width += 120; window.Position = new PixelPoint(40, 30); Update();
            CheckPreview();
            Point end = Center(selector);
            window.MouseMove(end, RawInputModifiers.LeftMouseButton); window.MouseUp(end, MouseButton.Left); Update();
            if (tool.Parent is not LayoutAnchorablePane || tool.IsFloating)
                throw new Exception("The packaged expanded preview did not complete the outer tool drop.");
            if (target.TargetMargin != default)
                throw new Exception("The packaged adorner retained its expanded geometry after the drop.");

            void CheckPreview()
            {
                PixelPoint origin = host.PointToScreen(default);
                PixelPoint edge = host.PointToScreen(new Point(host.Bounds.Width / 3, host.Bounds.Height));
                PixelPoint actualOrigin = indicator.PointToScreen(default);
                PixelPoint actualEdge = indicator.PointToScreen(new Point(indicator.Bounds.Width, indicator.Bounds.Height));
                if (indicator.Opacity <= 0 || target.TargetMargin.Left <= 0 ||
                    Math.Abs(origin.X - actualOrigin.X) > 2 || Math.Abs(origin.Y - actualOrigin.Y) > 2 ||
                    Math.Abs(edge.X - actualEdge.X) > 2 || Math.Abs(edge.Y - actualEdge.Y) > 2)
                    throw new Exception("The packaged outer highlight did not track its transformed document region.");
            }
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); DockSettings.UseFloatingDockAdorner = oldFloating; }
        Point Center(Control control) => window.PointToClient(control.PointToScreen(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2)));
        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }
}
