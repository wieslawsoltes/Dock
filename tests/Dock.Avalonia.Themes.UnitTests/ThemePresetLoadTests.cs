using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Dock.Avalonia.Themes;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ThemePresetLoadTests
{
    [AvaloniaFact]
    public void Fluent_VsCodeDark_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml",
            expectedSidebar: Color.Parse("#FF252526"),
            expectedIndicator: Color.Parse("#FF3794FF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Fluent_RiderLight_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml",
            expectedSidebar: Color.Parse("#FFF6F6F6"),
            expectedIndicator: Color.Parse("#FF4A8BFF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Fluent_VsCodeLight_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml",
            expectedSidebar: Color.Parse("#FFF3F3F3"),
            expectedIndicator: Color.Parse("#FF007ACC"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Fluent_RiderDark_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml",
            expectedSidebar: Color.Parse("#FF2A2C2F"),
            expectedIndicator: Color.Parse("#FF4C9FFF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Fluent_Default_Preset_Should_Load_And_Expose_Key_Tokens()
    {
        AssertPresetLoads(
            baseTheme: new DockFluentTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml",
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Simple_VsCodeDark_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml",
            expectedSidebar: Color.Parse("#FF252526"),
            expectedIndicator: Color.Parse("#FF3794FF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Simple_RiderLight_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderLight.axaml",
            expectedSidebar: Color.Parse("#FFF6F6F6"),
            expectedIndicator: Color.Parse("#FF4A8BFF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Simple_VsCodeLight_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeLight.axaml",
            expectedSidebar: Color.Parse("#FFF3F3F3"),
            expectedIndicator: Color.Parse("#FF007ACC"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Simple_RiderDark_Preset_Should_Load_And_Override_Key_Tokens()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml",
            expectedSidebar: Color.Parse("#FF2A2C2F"),
            expectedIndicator: Color.Parse("#FF4C9FFF"),
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Simple_Default_Preset_Should_Load_And_Expose_Key_Tokens()
    {
        AssertPresetLoads(
            baseTheme: new DockSimpleTheme(),
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml",
            expectedHeaderHeight: 26d);
    }

    [AvaloniaFact]
    public void Fluent_VsCodeDark_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml",
            expectedSidebar: Color.Parse("#FF252526"),
            expectedIndicator: Color.Parse("#FF3794FF"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Simple_VsCodeDark_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml",
            expectedSidebar: Color.Parse("#FF252526"),
            expectedIndicator: Color.Parse("#FF3794FF"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Fluent_RiderDark_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml",
            expectedSidebar: Color.Parse("#FF2A2C2F"),
            expectedIndicator: Color.Parse("#FF4C9FFF"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Fluent_VsCodeLight_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockFluentTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeLight.axaml",
            expectedSidebar: Color.Parse("#FFF3F3F3"),
            expectedIndicator: Color.Parse("#FF007ACC"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Simple_RiderDark_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml",
            expectedSidebar: Color.Parse("#FF2A2C2F"),
            expectedIndicator: Color.Parse("#FF4C9FFF"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Simple_VsCodeLight_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetOverrides(
            baseTheme: new DockSimpleTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeLight.axaml",
            expectedSidebar: Color.Parse("#FFF3F3F3"),
            expectedIndicator: Color.Parse("#FF007ACC"),
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Fluent_Default_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetLoads(
            baseTheme: new DockFluentTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml",
            expectedHeaderHeight: 22d);
    }

    [AvaloniaFact]
    public void Simple_Default_Preset_Should_Work_With_Compact_Density()
    {
        AssertPresetLoads(
            baseTheme: new DockSimpleTheme
            {
                DensityStyle = DockDensityStyle.Compact
            },
            presetUri: "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml",
            expectedHeaderHeight: 22d);
    }

    private static void AssertPresetOverrides(Styles baseTheme, string presetUri, Color expectedSidebar, Color expectedIndicator, double expectedHeaderHeight)
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
        app.Styles.Add(baseTheme);
        window.Resources = new ResourceDictionary();
        window.Resources.MergedDictionaries.Add(new ResourceInclude(new System.Uri(presetUri))
        {
            Source = new System.Uri(presetUri)
        });

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var host = (Border)window.Content!;

            AssertBrushColor(host, "DockSurfaceSidebarBrush", expectedSidebar);
            AssertBrushColor(host, "DockTabActiveIndicatorBrush", expectedIndicator);

            Assert.True(host.TryFindResource("DockHeaderHeight", out var heightValue));
            Assert.NotNull(heightValue);
            Assert.IsType<double>(heightValue);
            Assert.Equal(expectedHeaderHeight, (double)heightValue!);
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

    private static void AssertPresetLoads(Styles baseTheme, string presetUri, double expectedHeaderHeight)
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
        app.Styles.Add(baseTheme);
        window.Resources = new ResourceDictionary();
        window.Resources.MergedDictionaries.Add(new ResourceInclude(new System.Uri(presetUri))
        {
            Source = new System.Uri(presetUri)
        });

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var host = (Border)window.Content!;

            AssertBrushResource(host, "DockSurfaceSidebarBrush");
            AssertBrushResource(host, "DockTabActiveIndicatorBrush");

            Assert.True(host.TryFindResource("DockHeaderHeight", out var heightValue));
            Assert.NotNull(heightValue);
            Assert.IsType<double>(heightValue);
            Assert.Equal(expectedHeaderHeight, (double)heightValue!);
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

    private static void AssertBrushColor(Border host, string key, Color expectedColor)
    {
        Assert.True(host.TryFindResource(key, out var value), $"Missing resource '{key}'.");
        Assert.NotNull(value);

        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(value);
        Assert.Equal(expectedColor, brush.Color);
    }

    private static void AssertBrushResource(Border host, string key)
    {
        Assert.True(host.TryFindResource(key, out var value), $"Missing resource '{key}'.");
        Assert.NotNull(value);
        Assert.IsAssignableFrom<ISolidColorBrush>(value);
    }
}
