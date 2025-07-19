using Dock.Model.Prism.Controls;
using Xunit;

namespace Dock.Model.Prism.UnitTests;

public class DockableBaseTests
{
    [Fact]
    public void TrackingBounds_Roundtrip()
    {
        var doc = new Document();

        doc.TrackingAdapter.SetVisibleBounds(1, 2, 3, 4);
        doc.TrackingAdapter.GetVisibleBounds(out var vx, out var vy, out var vw, out var vh);
        Assert.Equal((1,2,3,4), (vx,vy,vw,vh));

        doc.TrackingAdapter.SetPinnedBounds(5, 6, 7, 8);
        doc.TrackingAdapter.GetPinnedBounds(out var px, out var py, out var pw, out var ph);
        Assert.Equal((5,6,7,8), (px,py,pw,ph));

        doc.TrackingAdapter.SetTabBounds(9, 10, 11, 12);
        doc.TrackingAdapter.GetTabBounds(out var tx, out var ty, out var tw, out var th);
        Assert.Equal((9,10,11,12), (tx,ty,tw,th));

        doc.TrackingAdapter.SetPointerPosition(13, 14);
        doc.TrackingAdapter.GetPointerPosition(out var ppx, out var ppy);
        Assert.Equal((13,14), (ppx,ppy));

        doc.TrackingAdapter.SetPointerScreenPosition(15, 16);
        doc.TrackingAdapter.GetPointerScreenPosition(out var psx, out var psy);
        Assert.Equal((15,16), (psx,psy));
    }
}
