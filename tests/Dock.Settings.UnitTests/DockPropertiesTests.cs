using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Settings;
using Dock.Model.Core;
using Xunit;

namespace Dock.Settings.UnitTests;

public class DockPropertiesTests
{
    [AvaloniaFact]
    public void DefaultValues_Are_Correct()
    {
        var control = new Control();
        Assert.False(DockProperties.GetIsDockTarget(control));
        Assert.False(DockProperties.GetIsDragArea(control));
        Assert.False(DockProperties.GetIsDropArea(control));
        Assert.True(DockProperties.GetIsDragEnabled(control));
        Assert.True(DockProperties.GetIsDropEnabled(control));
        Assert.False(DockProperties.GetShowDockIndicatorOnly(control));
        Assert.Equal(DockOperation.Fill, DockProperties.GetIndicatorDockOperation(control));
        Assert.Null(DockProperties.GetDockAdornerHost(control));
    }

    [AvaloniaFact]
    public void Setters_Update_Property_Values()
    {
        var control = new Control();
        var host = new Control();

        DockProperties.SetIsDockTarget(control, true);
        DockProperties.SetIsDragArea(control, true);
        DockProperties.SetIsDropArea(control, true);
        DockProperties.SetIsDragEnabled(control, false);
        DockProperties.SetIsDropEnabled(control, false);
        DockProperties.SetShowDockIndicatorOnly(control, true);
        DockProperties.SetIndicatorDockOperation(control, DockOperation.Left);
        DockProperties.SetDockAdornerHost(control, host);

        Assert.True(DockProperties.GetIsDockTarget(control));
        Assert.True(DockProperties.GetIsDragArea(control));
        Assert.True(DockProperties.GetIsDropArea(control));
        Assert.False(DockProperties.GetIsDragEnabled(control));
        Assert.False(DockProperties.GetIsDropEnabled(control));
        Assert.True(DockProperties.GetShowDockIndicatorOnly(control));
        Assert.Equal(DockOperation.Left, DockProperties.GetIndicatorDockOperation(control));
        Assert.Equal(host, DockProperties.GetDockAdornerHost(control));
    }
}
