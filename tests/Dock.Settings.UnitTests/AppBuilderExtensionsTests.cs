using Avalonia;
using Avalonia.Headless;
using Dock.Model.Core;
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
        var originalMinimumH = DockSettings.MinimumHorizontalDragDistance;
        var originalMinimumV = DockSettings.MinimumVerticalDragDistance;
        var originalFloatingDockAdorner = DockSettings.UseFloatingDockAdorner;
        var originalPinnedDockWindow = DockSettings.UsePinnedDockWindow;
        var originalOwnerForFloating = DockSettings.UseOwnerForFloatingWindows;
        var originalHostMode = DockSettings.FloatingWindowHostMode;
        var originalOwnerPolicy = DockSettings.FloatingWindowOwnerPolicy;
        var options = new DockSettingsOptions
        {
            MinimumHorizontalDragDistance = 10,
            MinimumVerticalDragDistance = 12,
            UseFloatingDockAdorner = true,
            UsePinnedDockWindow = true,
            UseOwnerForFloatingWindows = false,
            FloatingWindowHostMode = DockFloatingWindowHostMode.Managed,
            FloatingWindowOwnerPolicy = DockFloatingWindowOwnerPolicy.NeverOwned
        };

        try
        {
            var result = builder.WithDockSettings(options);

            Assert.Same(builder, result);
            Assert.Equal(10, DockSettings.MinimumHorizontalDragDistance);
            Assert.Equal(12, DockSettings.MinimumVerticalDragDistance);
            Assert.True(DockSettings.UseFloatingDockAdorner);
            Assert.True(DockSettings.UsePinnedDockWindow);
            Assert.False(DockSettings.UseOwnerForFloatingWindows);
            Assert.Equal(DockFloatingWindowHostMode.Managed, DockSettings.FloatingWindowHostMode);
            Assert.Equal(DockFloatingWindowOwnerPolicy.NeverOwned, DockSettings.FloatingWindowOwnerPolicy);
        }
        finally
        {
            DockSettings.MinimumHorizontalDragDistance = originalMinimumH;
            DockSettings.MinimumVerticalDragDistance = originalMinimumV;
            DockSettings.UseFloatingDockAdorner = originalFloatingDockAdorner;
            DockSettings.UsePinnedDockWindow = originalPinnedDockWindow;
            DockSettings.UseOwnerForFloatingWindows = originalOwnerForFloating;
            DockSettings.FloatingWindowHostMode = originalHostMode;
            DockSettings.FloatingWindowOwnerPolicy = originalOwnerPolicy;
        }
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
        var originalFloatingDockAdorner = DockSettings.UseFloatingDockAdorner;
        var originalPinnedDockWindow = DockSettings.UsePinnedDockWindow;
        var originalOwnerForFloating = DockSettings.UseOwnerForFloatingWindows;
        var originalHostMode = DockSettings.FloatingWindowHostMode;
        var originalOwnerPolicy = DockSettings.FloatingWindowOwnerPolicy;

        try
        {
            builder.UseFloatingDockAdorner(true)
                   .UsePinnedDockWindow(true)
                   .UseOwnerForFloatingWindows(false)
                   .UseFloatingWindowHostMode(DockFloatingWindowHostMode.Native)
                   .UseFloatingWindowOwnerPolicy(DockFloatingWindowOwnerPolicy.AlwaysOwned);

            Assert.True(DockSettings.UseFloatingDockAdorner);
            Assert.True(DockSettings.UsePinnedDockWindow);
            Assert.False(DockSettings.UseOwnerForFloatingWindows);
            Assert.Equal(DockFloatingWindowHostMode.Native, DockSettings.FloatingWindowHostMode);
            Assert.Equal(DockFloatingWindowOwnerPolicy.AlwaysOwned, DockSettings.FloatingWindowOwnerPolicy);
        }
        finally
        {
            DockSettings.UseFloatingDockAdorner = originalFloatingDockAdorner;
            DockSettings.UsePinnedDockWindow = originalPinnedDockWindow;
            DockSettings.UseOwnerForFloatingWindows = originalOwnerForFloating;
            DockSettings.FloatingWindowHostMode = originalHostMode;
            DockSettings.FloatingWindowOwnerPolicy = originalOwnerPolicy;
        }
    }
}
