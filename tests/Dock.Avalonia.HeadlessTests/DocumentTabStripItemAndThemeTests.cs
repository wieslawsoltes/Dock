using Avalonia.Styling;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DocumentTabStripItemAndThemeTests
{
    [AvaloniaFact]
    public void DocumentTabStripItem_Default_IsActive_False()
    {
        var item = new DocumentTabStripItem();
        Assert.False(item.IsActive);
    }

    [AvaloniaFact]
    public void DocumentTabStripItem_IsActive_CanBeSet()
    {
        var item = new DocumentTabStripItem { IsActive = true };
        Assert.True(item.IsActive);
    }

    [AvaloniaFact]
    public void DockFluentTheme_Can_Instantiate()
    {
        Styles theme = new DockFluentTheme();
        Assert.NotNull(theme);
    }

    [AvaloniaFact]
    public void DockSimpleTheme_Can_Instantiate()
    {
        Styles theme = new DockSimpleTheme();
        Assert.NotNull(theme);
    }
}
