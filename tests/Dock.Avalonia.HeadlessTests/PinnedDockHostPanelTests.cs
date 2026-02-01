using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockHostPanelTests
{
    private sealed class FixedSizeControl : Control
    {
        private readonly Size _size;

        public FixedSizeControl(double width, double height)
        {
            _size = new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }
    }

    private sealed class FixedSizePinnedDockControl : PinnedDockControl
    {
        private readonly Size _size;

        public FixedSizePinnedDockControl(double width, double height)
        {
            _size = new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }
    }

    [AvaloniaFact]
    public void PinnedDockHostPanel_Inline_Left_AllocatesSpace()
    {
        var panel = new PinnedDockHostPanel
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Inline,
            PinnedDockAlignment = Alignment.Left
        };

        var main = new FixedSizeControl(300, 200);
        var pinned = new FixedSizePinnedDockControl(100, 200);

        panel.Children.Add(main);
        panel.Children.Add(pinned);

        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        Assert.Equal(new Rect(0, 0, 100, 200), pinned.Bounds);
        Assert.Equal(new Rect(100, 0, 300, 200), main.Bounds);
    }

    [AvaloniaFact]
    public void PinnedDockHostPanel_Overlay_OverlapsChildren()
    {
        var panel = new PinnedDockHostPanel
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Overlay,
            PinnedDockAlignment = Alignment.Left
        };

        var main = new FixedSizeControl(300, 200);
        var pinned = new FixedSizePinnedDockControl(100, 200);

        panel.Children.Add(main);
        panel.Children.Add(pinned);

        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        var expected = new Rect(0, 0, 400, 200);
        Assert.Equal(expected, pinned.Bounds);
        Assert.Equal(expected, main.Bounds);
    }

    [AvaloniaFact]
    public void PinnedDockHostPanel_Inline_WithNoPinnedDockables_DoesNotReserveSpace()
    {
        var panel = new PinnedDockHostPanel
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Inline,
            PinnedDockAlignment = Alignment.Left
        };

        var main = new FixedSizeControl(300, 200);
        var pinned = new FixedSizePinnedDockControl(100, 200);

        panel.Children.Add(main);
        panel.Children.Add(pinned);

        var root = new RootDock
        {
            PinnedDock = new ToolDock
            {
                VisibleDockables = new System.Collections.Generic.List<IDockable>()
            }
        };

        panel.DataContext = root;

        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        var expected = new Rect(0, 0, 400, 200);
        Assert.Equal(expected, pinned.Bounds);
        Assert.Equal(expected, main.Bounds);
    }

    [AvaloniaFact]
    public void PinnedDockHostPanel_Inline_UsesPinnedControlAlignment()
    {
        var panel = new PinnedDockHostPanel
        {
            PinnedDockDisplayMode = PinnedDockDisplayMode.Inline,
            PinnedDockAlignment = Alignment.Left
        };

        var main = new FixedSizeControl(300, 200);
        var pinned = new FixedSizePinnedDockControl(100, 200)
        {
            PinnedDockAlignment = Alignment.Right
        };

        panel.Children.Add(main);
        panel.Children.Add(pinned);

        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        Assert.Equal(new Rect(300, 0, 100, 200), pinned.Bounds);
        Assert.Equal(new Rect(0, 0, 300, 200), main.Bounds);
    }
}
