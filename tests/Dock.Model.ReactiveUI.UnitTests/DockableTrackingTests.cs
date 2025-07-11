using Dock.Model.ReactiveUI.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class DockableTrackingTests
{
    [Fact]
    public void VisibleBounds_Are_Tracked()
    {
        var doc = new Document();
        doc.GetVisibleBounds(out var x, out var y, out var w, out var h);
        Assert.True(double.IsNaN(x));
        Assert.True(double.IsNaN(y));
        Assert.True(double.IsNaN(w));
        Assert.True(double.IsNaN(h));

        doc.SetVisibleBounds(1, 2, 3, 4);
        doc.GetVisibleBounds(out x, out y, out w, out h);
        Assert.Equal(1, x);
        Assert.Equal(2, y);
        Assert.Equal(3, w);
        Assert.Equal(4, h);
    }

    [Fact]
    public void PinnedBounds_Are_Tracked()
    {
        var doc = new Document();
        doc.SetPinnedBounds(5, 6, 7, 8);
        doc.GetPinnedBounds(out var x, out var y, out var w, out var h);
        Assert.Equal(5, x);
        Assert.Equal(6, y);
        Assert.Equal(7, w);
        Assert.Equal(8, h);
    }

    [Fact]
    public void TabBounds_Are_Tracked()
    {
        var doc = new Document();
        doc.SetTabBounds(9, 10, 11, 12);
        doc.GetTabBounds(out var x, out var y, out var w, out var h);
        Assert.Equal(9, x);
        Assert.Equal(10, y);
        Assert.Equal(11, w);
        Assert.Equal(12, h);
    }

    [Fact]
    public void PointerPositions_Are_Tracked()
    {
        var doc = new Document();
        doc.SetPointerPosition(13, 14);
        doc.GetPointerPosition(out var x, out var y);
        Assert.Equal(13, x);
        Assert.Equal(14, y);

        doc.SetPointerScreenPosition(15, 16);
        doc.GetPointerScreenPosition(out x, out y);
        Assert.Equal(15, x);
        Assert.Equal(16, y);
    }
}
