using Avalonia;
using Avalonia.Headless;
using Dock.Settings;
using Xunit;

namespace Dock.Settings.UnitTests;

public class AppBuilderExtensionsTests
{
    private static AppBuilder CreateBuilder() =>
        AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());

    [Fact]
    public void WithDockSettings_Applies_Options()
    {
        var builder = CreateBuilder();
        var options = new DockSettingsOptions
        {
            MinimumHorizontalDragDistance = 10,
            MinimumVerticalDragDistance = 12,
            UseFloatingDockAdorner = true,
            UseFloatingDragPreview = true,
            UsePinnedDockWindow = true,
            EnableGlobalDocking = false,
            UseOwnerForFloatingWindows = false
        };

        var result = builder.WithDockSettings(options);

        Assert.Same(builder, result);
        Assert.Equal(10, DockSettings.MinimumHorizontalDragDistance);
        Assert.Equal(12, DockSettings.MinimumVerticalDragDistance);
        Assert.True(DockSettings.UseFloatingDockAdorner);
        Assert.True(DockSettings.UseFloatingDragPreview);
        Assert.True(DockSettings.UsePinnedDockWindow);
        Assert.False(DockSettings.EnableGlobalDocking);
        Assert.False(DockSettings.UseOwnerForFloatingWindows);
    }

    [Fact]
    public void WithDockSettings_Returns_Builder_When_Options_Null()
    {
        var builder = CreateBuilder();
        var oldH = DockSettings.MinimumHorizontalDragDistance;
        var oldV = DockSettings.MinimumVerticalDragDistance;
        try
        {
            var result = builder.WithDockSettings(null);
            Assert.Same(builder, result);
            Assert.Equal(oldH, DockSettings.MinimumHorizontalDragDistance);
            Assert.Equal(oldV, DockSettings.MinimumVerticalDragDistance);
        }
        finally
        {
            DockSettings.MinimumHorizontalDragDistance = oldH;
            DockSettings.MinimumVerticalDragDistance = oldV;
        }
    }

    [Fact]
    public void Extension_Methods_Set_Flags()
    {
        var builder = CreateBuilder();

        builder.UseFloatingDockAdorner(true)
               .UseFloatingDragPreview(true)
               .UsePinnedDockWindow(true)
               .EnableGlobalDocking(false)
               .UseOwnerForFloatingWindows(false);

        Assert.True(DockSettings.UseFloatingDockAdorner);
        Assert.True(DockSettings.UseFloatingDragPreview);
        Assert.True(DockSettings.UsePinnedDockWindow);
        Assert.False(DockSettings.EnableGlobalDocking);
        Assert.False(DockSettings.UseOwnerForFloatingWindows);
    }
}
