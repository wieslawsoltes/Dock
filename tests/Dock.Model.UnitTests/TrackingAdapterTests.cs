using Dock.Model.Adapters;
using Xunit;

namespace Dock.Model.UnitTests;

public class TrackingAdapterTests
{
    [Fact]
    public void SetAndGetVisibleBounds_RoundTrip()
    {
        var adapter = new TrackingAdapter();
        adapter.SetVisibleBounds(1, 2, 3, 4);
        adapter.GetVisibleBounds(out var x, out var y, out var w, out var h);
        Assert.Equal(1, x);
        Assert.Equal(2, y);
        Assert.Equal(3, w);
        Assert.Equal(4, h);
    }

    [Fact]
    public void SetAndGetPinnedBounds_RoundTrip()
    {
        var adapter = new TrackingAdapter();
        adapter.SetPinnedBounds(5, 6, 7, 8);
        adapter.GetPinnedBounds(out var x, out var y, out var w, out var h);
        Assert.Equal(5, x);
        Assert.Equal(6, y);
        Assert.Equal(7, w);
        Assert.Equal(8, h);
    }

    [Fact]
    public void SetAndGetTabBounds_RoundTrip()
    {
        var adapter = new TrackingAdapter();
        adapter.SetTabBounds(9, 10, 11, 12);
        adapter.GetTabBounds(out var x, out var y, out var w, out var h);
        Assert.Equal(9, x);
        Assert.Equal(10, y);
        Assert.Equal(11, w);
        Assert.Equal(12, h);
    }

    [Fact]
    public void SetAndGetPointerPositions_RoundTrip()
    {
        var adapter = new TrackingAdapter();
        adapter.SetPointerPosition(13, 14);
        adapter.GetPointerPosition(out var x, out var y);
        Assert.Equal(13, x);
        Assert.Equal(14, y);

        adapter.SetPointerScreenPosition(15, 16);
        adapter.GetPointerScreenPosition(out var sx, out var sy);
        Assert.Equal(15, sx);
        Assert.Equal(16, sy);
    }
}
