using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class TabControlTests
{
    public static IEnumerable<object[]> DockPreviewCases()
    {
        foreach (DockOperation operation in new[] { DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight,
                     DockOperation.OuterBottom, DockOperation.Left, DockOperation.Top, DockOperation.Right, DockOperation.Bottom, DockOperation.Fill })
            for (int hosting = 0; hosting < 3; hosting++)
                foreach (bool grouped in new[] { false, true })
                    foreach (bool scaled in new[] { false, true })
                        yield return new object[] { operation, hosting, grouped, scaled };
    }

    [AvaloniaTheory]
    [MemberData(nameof(DockPreviewCases))]
    public void Dock_Previews_Use_Their_Region_While_Selectors_Stay_Over_The_Hovered_Pane(
        DockOperation operation, int hosting, bool grouped, bool scaled)
    {
        bool oldFloating = DockSettings.UseFloatingDockAdorner;
        bool oldManaged = DockSettings.UseManagedWindows;
        DockSettings.UseFloatingDockAdorner = hosting != 0;
        DockSettings.UseManagedWindows = hosting == 2;
        try
        {
            using Workspace workspace = new(true);
            workspace.Window.Width = 1800; workspace.Window.Height = 1000;
            workspace.Manager.AllowMixedOrientation = true;
            bool outer = operation is DockOperation.OuterLeft or DockOperation.OuterTop or DockOperation.OuterRight or DockOperation.OuterBottom;
            ((LayoutAnchorable)workspace.First).CanDockAsTabbedDocument = !outer;
            if (scaled)
            {
                workspace.Manager.RenderTransformOrigin = RelativePoint.TopLeft;
                workspace.Manager.RenderTransform = new ScaleTransform(0.75, 0.75);
            }
            LayoutDocumentPane pane = new(new LayoutDocument { Title = "Destination", Content = "Target" });
            ILayoutElement region = workspace.Manager.Layout.RootPanel;
            if (grouped)
            {
                LayoutDocumentPaneGroup group = new(new LayoutDocumentPane(new LayoutDocument { Title = "Sibling" }))
                    { DockWidth = new GridLength(5, GridUnitType.Star), Children = { new LayoutDocumentPaneGroup(pane) } };
                workspace.Manager.Layout.RootPanel.Children.Add(group);
                region = group;
            }
            else
            {
                pane.DockWidth = new GridLength(5, GridUnitType.Star);
                workspace.Manager.Layout.RootPanel.Children.Add(pane);
            }
            RecordingOverlayFactory factory = new(DockAdornerWindow.GetFactory(workspace.Manager)!);
            DockAdornerWindow.SetFactory(workspace.Manager, factory);
            workspace.Update();
            Control regionControl = workspace.Manager.GetVisualDescendants().OfType<Control>()
                .Single(control => control is ILayoutControl view && ReferenceEquals(view.Model, region));
            LayoutDocumentPaneControl paneControl = workspace.Manager.GetVisualDescendants().OfType<LayoutDocumentPaneControl>()
                .Single(control => ReferenceEquals(control.Model, pane));
            Control surface = paneControl.GetVisualDescendants().OfType<Control>().First(control =>
                DockProperties.GetIsDockTarget(control) && ReferenceEquals(control.DataContext, pane.DockModel) && control.IsEffectivelyVisible);
            Control source = NativeTab(workspace.Manager, workspace.First, true);
            Point start = Center(source);
            Point destination = surface.TranslatePoint(new Point(surface.Bounds.Width / 2, surface.Bounds.Height / 2), workspace.Window)!.Value;
            workspace.Window.MouseMove(start);
            workspace.Window.MouseDown(start, MouseButton.Left);
            workspace.Window.MouseMove(start + new Vector(12, 0), RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseMove(start + new Vector(0, -80), RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseMove(destination, RawInputModifiers.LeftMouseButton);
            workspace.Update();
            OverlayWindow? overlay = hosting == 1 ? factory.Windows.Last(window => window.IsVisible && window.Content is DockTarget) : null;
            DockTarget target = overlay is null ? Assert.Single(workspace.Window.GetVisualDescendants().OfType<DockTarget>())
                : Assert.IsType<DockTarget>(overlay.Content);
            target.UpdateLayout();
            Control selector = Assert.IsAssignableFrom<Control>(target.GetSelector(operation));
            Point drop = workspace.Window.PointToClient(selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2)));
            workspace.Window.MouseMove(drop, RawInputModifiers.LeftMouseButton);
            workspace.Update();
            string part = operation == DockOperation.Fill ? "Center" : operation.ToString();
            Control indicator = target.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_" + part + "Indicator");
            Assert.True(indicator.Opacity > 0, $"The {operation} preview did not activate after moving onto its selector.");
            AssertPreviewGeometry();
            workspace.Window.Width += 120;
            workspace.Window.Position = new PixelPoint(40, 30);
            workspace.Update(); target.UpdateLayout();
            AssertPreviewGeometry();
            if (overlay is not null)
            {
                Assert.Equal(DropAreaType.DocumentPane, overlay.DropArea!.Type);
                AssertPreviewRect(PreviewScreenBounds(paneControl), overlay.DropArea.DetectionRect);
            }
            drop = workspace.Window.PointToClient(selector.PointToScreen(new Point(selector.Bounds.Width / 2, selector.Bounds.Height / 2)));
            workspace.Window.MouseMove(drop, RawInputModifiers.LeftMouseButton);
            workspace.Window.MouseUp(drop, MouseButton.Left);
            workspace.Update();
            if (outer) Assert.IsType<LayoutAnchorablePane>(workspace.First.Parent);
            else Assert.IsType<LayoutDocumentPane>(workspace.First.Parent);
            Assert.False(workspace.First.IsFloating);

            void AssertPreviewGeometry()
            {
                Rect bounds = PreviewScreenBounds(outer ? regionControl : surface);
                Rect expected = operation switch
                {
                    DockOperation.OuterLeft => new Rect(bounds.Left, bounds.Top, bounds.Width / 3, bounds.Height),
                    DockOperation.OuterTop => new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height / 3),
                    DockOperation.OuterRight => new Rect(bounds.Right - bounds.Width / 3, bounds.Top, bounds.Width / 3, bounds.Height),
                    DockOperation.OuterBottom => new Rect(bounds.Left, bounds.Bottom - bounds.Height / 3, bounds.Width, bounds.Height / 3),
                    DockOperation.Left => new Rect(bounds.Left, bounds.Top, bounds.Width / 2, bounds.Height),
                    DockOperation.Top => new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height / 2),
                    DockOperation.Right => new Rect(bounds.Right - bounds.Width / 2, bounds.Top, bounds.Width / 2, bounds.Height),
                    DockOperation.Bottom => new Rect(bounds.Left, bounds.Bottom - bounds.Height / 2, bounds.Width, bounds.Height / 2),
                    _ => bounds
                };
                AssertPreviewRect(expected, PreviewScreenBounds(indicator));
                AssertPreviewRect(PreviewScreenBounds(regionControl), PreviewScreenBounds(target));
                Control center = Assert.IsAssignableFrom<Control>(target.GetSelector(DockOperation.Fill));
                Point paneCenter = PreviewScreenBounds(surface).Center;
                Point selectorCenter = PreviewScreenBounds(center).Center;
                Assert.InRange(Math.Abs(paneCenter.X - selectorCenter.X), 0, 2);
                Assert.InRange(Math.Abs(paneCenter.Y - selectorCenter.Y), 0, 2);
            }
        }
        finally { DockSettings.UseFloatingDockAdorner = oldFloating; DockSettings.UseManagedWindows = oldManaged; }
    }

    private static Rect PreviewScreenBounds(Control control)
    {
        PixelPoint first = control.PointToScreen(default);
        PixelPoint last = control.PointToScreen(new Point(control.Bounds.Width, control.Bounds.Height));
        return new Rect(first.X, first.Y, last.X - first.X, last.Y - first.Y);
    }

    private static void AssertPreviewRect(Rect expected, Rect actual)
    {
        Assert.True(Math.Abs(expected.Left - actual.Left) <= 2 && Math.Abs(expected.Top - actual.Top) <= 2 &&
            Math.Abs(expected.Width - actual.Width) <= 2 && Math.Abs(expected.Height - actual.Height) <= 2,
            $"Expected preview {expected}; actual {actual}.");
    }
}
