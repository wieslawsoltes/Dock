using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Avalonia.Themes;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ThemeDensityStyleTests
{
    [AvaloniaFact]
    public void DockFluentTheme_Should_Resolve_Compact_Density_Resources()
    {
        var theme = new DockFluentTheme
        {
            DensityStyle = DockDensityStyle.Compact
        };

        AssertDensityResources(
            theme,
            expectedTabItemMinHeight: 22d,
            expectedToolTabItemMinHeight: 0d,
            expectedCloseButtonSize: 12d,
            expectedMdiIconSize: 10d,
            expectedChromeGripHeight: 4d,
            expectedTabContentSpacing: 1d,
            expectedDragPreviewStatusIconSize: 9d,
            expectedOverlayMessageFontSize: 13d,
            expectedDialogTitleFontSize: 14d);
    }

    [AvaloniaFact]
    public void DockSimpleTheme_Should_Resolve_Compact_Density_Resources()
    {
        var theme = new DockSimpleTheme
        {
            DensityStyle = DockDensityStyle.Compact
        };

        AssertDensityResources(
            theme,
            expectedTabItemMinHeight: 22d,
            expectedToolTabItemMinHeight: 0d,
            expectedCloseButtonSize: 12d,
            expectedMdiIconSize: 10d,
            expectedChromeGripHeight: 4d,
            expectedTabContentSpacing: 1d,
            expectedDragPreviewStatusIconSize: 9d,
            expectedOverlayMessageFontSize: 13d,
            expectedDialogTitleFontSize: 14d);
    }

    [AvaloniaFact]
    public void DockFluentTheme_Should_Resolve_Normal_Density_Resources()
    {
        var theme = new DockFluentTheme
        {
            DensityStyle = DockDensityStyle.Normal
        };

        AssertDensityResources(
            theme,
            expectedTabItemMinHeight: 24d,
            expectedToolTabItemMinHeight: 0d,
            expectedCloseButtonSize: 14d,
            expectedMdiIconSize: 12d,
            expectedChromeGripHeight: 5d,
            expectedTabContentSpacing: 2d,
            expectedDragPreviewStatusIconSize: 10d,
            expectedOverlayMessageFontSize: 14d,
            expectedDialogTitleFontSize: 16d);
    }

    [AvaloniaFact]
    public void DockSimpleTheme_Should_Resolve_Normal_Density_Resources()
    {
        var theme = new DockSimpleTheme
        {
            DensityStyle = DockDensityStyle.Normal
        };

        AssertDensityResources(
            theme,
            expectedTabItemMinHeight: 24d,
            expectedToolTabItemMinHeight: 0d,
            expectedCloseButtonSize: 14d,
            expectedMdiIconSize: 12d,
            expectedChromeGripHeight: 5d,
            expectedTabContentSpacing: 2d,
            expectedDragPreviewStatusIconSize: 10d,
            expectedOverlayMessageFontSize: 14d,
            expectedDialogTitleFontSize: 16d);
    }

    [AvaloniaFact]
    public void DockFluentTheme_Should_Update_Resources_When_Density_Changes()
    {
        var theme = new DockFluentTheme
        {
            DensityStyle = DockDensityStyle.Normal
        };

        AssertDensitySwitch(theme, expectedNormalTabItemMinHeight: 24d, expectedCompactTabItemMinHeight: 22d);
    }

    [AvaloniaFact]
    public void DockSimpleTheme_Should_Update_Resources_When_Density_Changes()
    {
        var theme = new DockSimpleTheme
        {
            DensityStyle = DockDensityStyle.Normal
        };

        AssertDensitySwitch(theme, expectedNormalTabItemMinHeight: 24d, expectedCompactTabItemMinHeight: 22d);
    }

    private static void AssertDensityResources(
        Styles theme,
        double expectedTabItemMinHeight,
        double expectedToolTabItemMinHeight,
        double expectedCloseButtonSize,
        double expectedMdiIconSize,
        double expectedChromeGripHeight,
        double expectedTabContentSpacing,
        double expectedDragPreviewStatusIconSize,
        double expectedOverlayMessageFontSize,
        double expectedDialogTitleFontSize)
    {
        var app = Application.Current ?? throw new System.InvalidOperationException("Avalonia application is not initialized.");
        List<IStyle> previousStyles = app.Styles.ToList();
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = new Border()
        };

        app.Styles.Clear();
        app.Styles.Add(theme);

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var host = (Border)window.Content!;

            Assert.True(host.TryFindResource("DockTabItemMinHeight", out var tabItemHeightValue));
            Assert.Equal(expectedTabItemMinHeight, Assert.IsType<double>(tabItemHeightValue));

            Assert.True(host.TryFindResource("DockToolTabItemMinHeight", out var toolTabItemHeightValue));
            Assert.Equal(expectedToolTabItemMinHeight, Assert.IsType<double>(toolTabItemHeightValue));

            Assert.True(host.TryFindResource("DockCloseButtonSize", out var closeButtonSizeValue));
            Assert.Equal(expectedCloseButtonSize, Assert.IsType<double>(closeButtonSizeValue));

            Assert.True(host.TryFindResource("DockMdiTitleIconSize", out var mdiIconSizeValue));
            Assert.Equal(expectedMdiIconSize, Assert.IsType<double>(mdiIconSizeValue));

            Assert.True(host.TryFindResource("DockChromeGripHeight", out var chromeGripHeightValue));
            Assert.Equal(expectedChromeGripHeight, Assert.IsType<double>(chromeGripHeightValue));

            Assert.True(host.TryFindResource("DockTabContentSpacing", out var tabContentSpacingValue));
            Assert.Equal(expectedTabContentSpacing, Assert.IsType<double>(tabContentSpacingValue));

            Assert.True(host.TryFindResource("DockDragPreviewStatusIconSize", out var dragPreviewStatusIconSizeValue));
            Assert.Equal(expectedDragPreviewStatusIconSize, Assert.IsType<double>(dragPreviewStatusIconSizeValue));

            Assert.True(host.TryFindResource("DockOverlayMessageFontSize", out var overlayMessageFontSizeValue));
            Assert.Equal(expectedOverlayMessageFontSize, Assert.IsType<double>(overlayMessageFontSizeValue));

            Assert.True(host.TryFindResource("DockDialogTitleFontSize", out var dialogTitleFontSizeValue));
            Assert.Equal(expectedDialogTitleFontSize, Assert.IsType<double>(dialogTitleFontSizeValue));
        }
        finally
        {
            window.Close();
            app.Styles.Clear();
            foreach (var style in previousStyles)
            {
                app.Styles.Add(style);
            }
        }
    }

    private static void AssertDensitySwitch(Styles theme, double expectedNormalTabItemMinHeight, double expectedCompactTabItemMinHeight)
    {
        var app = Application.Current ?? throw new System.InvalidOperationException("Avalonia application is not initialized.");
        List<IStyle> previousStyles = app.Styles.ToList();
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Content = new Border()
        };

        app.Styles.Clear();
        app.Styles.Add(theme);

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var host = (Border)window.Content!;
            Assert.True(host.TryFindResource("DockTabItemMinHeight", out var initialValue));
            Assert.Equal(expectedNormalTabItemMinHeight, Assert.IsType<double>(initialValue));

            switch (theme)
            {
                case DockFluentTheme fluent:
                    fluent.DensityStyle = DockDensityStyle.Compact;
                    break;
                case DockSimpleTheme simple:
                    simple.DensityStyle = DockDensityStyle.Compact;
                    break;
            }

            Dispatcher.UIThread.RunJobs();

            Assert.True(host.TryFindResource("DockTabItemMinHeight", out var compactValue));
            Assert.Equal(expectedCompactTabItemMinHeight, Assert.IsType<double>(compactValue));
        }
        finally
        {
            window.Close();
            app.Styles.Clear();
            foreach (var style in previousStyles)
            {
                app.Styles.Add(style);
            }
        }
    }
}
