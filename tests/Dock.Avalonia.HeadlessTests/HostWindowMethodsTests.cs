using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowMethodsTests
{
    [AvaloniaFact]
    public void SetPosition_And_GetPosition_Work()
    {
        var window = new HostWindow();
        window.SetPosition(100, 50);
        window.GetPosition(out var x, out var y);
        Assert.Equal(100, x);
        Assert.Equal(50, y);
    }

    [AvaloniaFact]
    public void SetSize_And_GetSize_Work()
    {
        var window = new HostWindow();
        window.SetSize(800, 600);
        window.GetSize(out var w, out var h);
        Assert.Equal(800, w);
        Assert.Equal(600, h);
    }

    [AvaloniaFact]
    public void SetPosition_Ignores_NaN()
    {
        var window = new HostWindow();
        window.SetPosition(double.NaN, double.NaN);
        window.GetPosition(out var x, out var y);
        Assert.Equal(0, x);
        Assert.Equal(0, y);
    }

    [AvaloniaFact]
    public void SetSize_Ignores_NaN()
    {
        var window = new HostWindow();
        window.SetSize(double.NaN, double.NaN);
        window.GetSize(out var w, out var h);
        Assert.True(double.IsNaN(w));
        Assert.True(double.IsNaN(h));
    }
}
