using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentTabStripDoubleTapTests
{
    [AvaloniaFact]
    public void ShouldIgnoreWindowStateToggleSource_WhenSourceIsButton_ReturnsTrue()
    {
        var tabStrip = new DocumentTabStrip();
        var sourceButton = new Button();

        var result = tabStrip.ShouldIgnoreWindowStateToggleSource(sourceButton);

        Assert.True(result);
    }

    [AvaloniaFact]
    public void ShouldIgnoreWindowStateToggleSource_WhenSourceIsTabStrip_ReturnsFalse()
    {
        var tabStrip = new DocumentTabStrip();

        var result = tabStrip.ShouldIgnoreWindowStateToggleSource(tabStrip);

        Assert.False(result);
    }
}
