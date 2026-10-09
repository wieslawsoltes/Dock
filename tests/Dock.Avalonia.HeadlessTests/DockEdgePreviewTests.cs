using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockEdgePreviewTests
{
    private sealed class EdgeTarget : GlobalDockTarget
    {
        public void SetSelector(DockOperation operation, Control selector)
        {
            IndicatorOperations.Clear();
            SelectorsOperations.Clear();
            IndicatorOperations[operation] = new Panel();
            SelectorsOperations[operation] = selector;
        }
    }

    [AvaloniaTheory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Right)]
    [InlineData(DockOperation.Top)]
    [InlineData(DockOperation.Bottom)]
    public void Edge_filter_is_opt_in_and_preserves_local_space_in_small_panes(DockOperation operation)
    {
        var target = new EdgeTarget();
        var selector = new Border();
        var pane = new Border { Width = 40, Height = 40 };
        var panel = new Panel { Children = { selector, target, pane } };
        var window = new Window { Width = 1000, Height = 600, Content = panel };
        try
        {
            window.Show();
            window.UpdateLayout();
            target.SetSelector(operation, selector);
            Point AtDistance(double distance) => operation switch
            {
                DockOperation.Left => new Point(distance, 300),
                DockOperation.Right => new Point(1000 - distance, 300),
                DockOperation.Top => new Point(500, distance),
                _ => new Point(500, 600 - distance)
            };
            DockOperation Hit(double distance) => target.GetDockOperation(AtDistance(distance), pane, panel,
                DragAction.Move, (_, _, _, _) => true);
            Assert.True(double.IsNaN(target.EdgeHitTestThickness));
            Assert.Equal(operation, Hit(100)); // Existing template hit regions remain unchanged by default.
            target.EdgeHitTestThickness = 20;
            Assert.Equal(operation, Hit(7));
            Assert.Equal(DockOperation.None, Hit(10)); // 40-DIP pane gets only an 8-DIP outer zone.
            pane.Width = pane.Height = 240;
            window.UpdateLayout();
            Assert.Equal(operation, Hit(19));
            Assert.Equal(DockOperation.None, Hit(21)); // Larger panes still respect the configured maximum.
            target.EdgeHitTestThickness = double.NaN;
            Assert.Equal(operation, Hit(100));
        }
        finally { window.Close(); }
    }
}
