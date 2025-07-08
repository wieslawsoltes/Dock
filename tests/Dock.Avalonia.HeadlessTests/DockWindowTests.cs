using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockWindowTests
{
    [AvaloniaFact]
    public void DockWindow_Defaults_Are_Correct()
    {
        var window = new DockWindow();

        Assert.Equal(nameof(IDockWindow), window.Id);
        Assert.Equal(nameof(IDockWindow), window.Title);
        Assert.Equal(0, window.X);
        Assert.Equal(0, window.Y);
        Assert.Equal(0, window.Width);
        Assert.Equal(0, window.Height);
        Assert.False(window.Topmost);
    }
}
