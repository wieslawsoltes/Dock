using Dock.Model.Prism.Controls;
using Xunit;

namespace Dock.Model.Prism.UnitTests;

public class DockableBaseTests
{
    [Fact]
    public void TrackingBounds_Roundtrip()
    {
        var doc = new Document();

        doc.SetVisibleBounds(1, 2, 3, 4);
        doc.GetVisibleBounds(out var vx, out var vy, out var vw, out var vh);
        Assert.Equal((1,2,3,4), (vx,vy,vw,vh));

        doc.SetPinnedBounds(5, 6, 7, 8);
        doc.GetPinnedBounds(out var px, out var py, out var pw, out var ph);
        Assert.Equal((5,6,7,8), (px,py,pw,ph));

        doc.SetTabBounds(9, 10, 11, 12);
        doc.GetTabBounds(out var tx, out var ty, out var tw, out var th);
        Assert.Equal((9,10,11,12), (tx,ty,tw,th));

        doc.SetPointerPosition(13, 14);
        doc.GetPointerPosition(out var ppx, out var ppy);
        Assert.Equal((13,14), (ppx,ppy));

        doc.SetPointerScreenPosition(15, 16);
        doc.GetPointerScreenPosition(out var psx, out var psy);
        Assert.Equal((15,16), (psx,psy));
    }
}
