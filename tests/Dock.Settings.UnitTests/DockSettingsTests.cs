using Avalonia;
using Dock.Settings;
using Xunit;

namespace Dock.Settings.UnitTests;

public class DockSettingsTests
{
    [Fact]
    public void IsMinimumDragDistance_Vector_Works()
    {
        var oldH = DockSettings.MinimumHorizontalDragDistance;
        var oldV = DockSettings.MinimumVerticalDragDistance;
        try
        {
            DockSettings.MinimumHorizontalDragDistance = 4;
            DockSettings.MinimumVerticalDragDistance = 4;

            Assert.False(DockSettings.IsMinimumDragDistance(new Vector(3, 3)));
            Assert.True(DockSettings.IsMinimumDragDistance(new Vector(5, 1)));
            Assert.True(DockSettings.IsMinimumDragDistance(new Vector(0, 5)));
        }
        finally
        {
            DockSettings.MinimumHorizontalDragDistance = oldH;
            DockSettings.MinimumVerticalDragDistance = oldV;
        }
    }

    [Fact]
    public void IsMinimumDragDistance_PixelPoint_Works()
    {
        var oldH = DockSettings.MinimumHorizontalDragDistance;
        var oldV = DockSettings.MinimumVerticalDragDistance;
        try
        {
            DockSettings.MinimumHorizontalDragDistance = 4;
            DockSettings.MinimumVerticalDragDistance = 4;

            Assert.False(DockSettings.IsMinimumDragDistance(new PixelPoint(3, 3)));
            Assert.True(DockSettings.IsMinimumDragDistance(new PixelPoint(5, 1)));
            Assert.True(DockSettings.IsMinimumDragDistance(new PixelPoint(0, 5)));
        }
        finally
        {
            DockSettings.MinimumHorizontalDragDistance = oldH;
            DockSettings.MinimumVerticalDragDistance = oldV;
        }
    }
}
