using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Browser;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

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

    [AvaloniaFact]
    public void BrowserTabTheme_Enables_Full_Document_Dock_Selector()
    {
        var theme = new BrowserTabTheme();
        var resourceNode = Assert.IsAssignableFrom<IResourceNode>(theme);

        Assert.True(resourceNode.TryGetResource(
            "DockDocumentControlShowDockIndicatorOnly",
            ThemeVariant.Default,
            out var resource));
        Assert.False(Assert.IsType<bool>(resource));
    }
}
