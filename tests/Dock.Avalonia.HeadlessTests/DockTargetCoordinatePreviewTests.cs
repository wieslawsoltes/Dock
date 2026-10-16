using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockTargetCoordinatePreviewTests
{
    private sealed class Target : DockTarget
    {
        public void SetSelector(Control selector)
        {
            IndicatorOperations.Clear();
            SelectorsOperations.Clear();
            IndicatorOperations[DockOperation.Left] = new Panel();
            SelectorsOperations[DockOperation.Left] = selector;
        }
    }

    private sealed class GlobalTarget : GlobalDockTarget
    {
        public void SetSelector(DockOperation operation)
        {
            IndicatorOperations.Clear();
            SelectorsOperations.Clear();
            IndicatorOperations[operation] = new Panel();
            SelectorsOperations[operation] = this;
        }
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left, false)]
    [InlineData(DockOperation.Right, false)]
    [InlineData(DockOperation.Top, false)]
    [InlineData(DockOperation.Bottom, false)]
    [InlineData(DockOperation.Top, true)]
    public void Global_zone_uses_pane_size_when_hovering_small_header(DockOperation operation, bool explicitHost)
    {
        var header = new Border { Width = 60, Height = 24 };
        var pane = new Panel { Children = { header } };
        pane.SetValue(DockProperties.IsDockTargetProperty, true);
        if (explicitHost) DockProperties.SetDockAdornerHost(header, pane);
        var target = new GlobalTarget { EdgeHitTestThickness = 20 };
        var content = new Panel { Children = { pane, target } };
        var window = new Window { Width = 1000, Height = 600, Content = content };
        try
        {
            window.Show(); window.UpdateLayout(); target.SetSelector(operation);
            var point = operation switch
            {
                DockOperation.Left => new Point(15, 300),
                DockOperation.Right => new Point(985, 300),
                DockOperation.Top => new Point(500, 15),
                _ => new Point(500, 585)
            };
            Assert.Equal(operation, target.GetDockOperation(point, header, content, DragAction.Move, (_, _, _, _) => true));
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(0, 0)]
    [InlineData(350, 80)]
    public void Selector_uses_adorned_pane_coordinates(double x, double y)
    {
        var pane = new Border { Width = 400, Height = 300, Background = Brushes.Gray };
        var canvas = new Canvas { Children = { pane } };
        Canvas.SetLeft(pane, x);
        Canvas.SetTop(pane, y);
        var window = new Window { Width = 1000, Height = 600, Content = canvas };
        try
        {
            window.Show();
            window.UpdateLayout();
            var layer = AdornerLayer.GetAdornerLayer(pane)!;
            Assert.NotNull(layer);
            var target = new Target();
            AdornerLayer.SetAdornedElement(target, pane);
            layer.Children.Add(target);
            window.UpdateLayout();
            // A selector at the target origin. Ordinary visual-tree translation omits
            // the compositor transform used to place the adorner over the pane.
            target.SetSelector(target);
            target.SetPreviewBounds(new Rect(0, 0, 200, 300));
            window.UpdateLayout();
            Assert.Equal(DockOperation.Left, target.GetDockOperation(new Point(x + 200, y + 150), pane, canvas,
                DragAction.Move, (_, _, _, _) => true));
            Assert.Equal(DockOperation.Window, target.GetDockOperation(new Point(x + 420, y + 150), pane, canvas,
                DragAction.Move, (_, _, _, _) => true));
        }
        finally { window.Close(); }
    }
}
