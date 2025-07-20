using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class ToolChromeControlTests
{
    [AvaloniaFact]
    public void ShowPinMenu_Defaults_To_True()
    {
        var control = new ToolChromeControl();
        Assert.True(control.ShowPinMenu);
    }

    [AvaloniaFact]
    public void ShowPinMenu_Can_Be_Disabled()
    {
        var control = new ToolChromeControl { ShowPinMenu = false };
        Assert.False(control.ShowPinMenu);
    }
}

