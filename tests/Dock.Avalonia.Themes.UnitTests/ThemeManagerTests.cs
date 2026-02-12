using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Dock.Avalonia.Themes;
using Xunit;

namespace Dock.Avalonia.Themes.UnitTests;

[Collection(ThemeResourceIsolationCollection.Name)]
public class ThemeManagerTests
{
    [AvaloniaFact]
    public void Fluent_Manager_Should_Expose_Preset_List()
    {
        var manager = new DockFluentThemeManager();

        Assert.Equal(5, manager.PresetNames.Count);
        Assert.Equal("Default", manager.PresetNames[0]);
        Assert.Equal("VS Code Dark", manager.PresetNames[2]);
        Assert.Equal("Rider Dark", manager.PresetNames[4]);
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Switch_Preset_And_Remove_Duplicates()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesScope(app);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/VsCodeDark.axaml"));

        manager.SwitchPreset(3);

        Assert.Equal("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal(3, manager.CurrentPresetIndex);
        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
    }

    [AvaloniaFact]
    public void Simple_Manager_Should_Switch_Preset()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockSimpleThemeManager();

        using var scope = new AppResourcesScope(app);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml"));

        manager.SwitchPreset(2);

        Assert.Equal("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal(2, manager.CurrentPresetIndex);
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Create_Preset_Include_When_Missing()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesScope(app);

        manager.SwitchPreset(4);

        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal(4, manager.CurrentPresetIndex);
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Apply_Preset_Resources_After_Switch()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesScope(app);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));

        var host = new Border();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = host
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(2);
            Dispatcher.UIThread.RunJobs();
            AssertBrushColor(host, "DockSurfaceSidebarBrush", Color.Parse("#FF252526"));

            manager.SwitchPreset(3);
            Dispatcher.UIThread.RunJobs();
            AssertBrushColor(host, "DockSurfaceSidebarBrush", Color.Parse("#FFF6F6F6"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Update_Editor_Surface_Brush_After_Switch()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesScope(app);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));

        var host = new Border();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = host
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(2);
            Dispatcher.UIThread.RunJobs();
            AssertBrushColor(host, "DockSurfaceEditorBrush", Color.Parse("#FF1E1E1E"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Update_Tokens_When_DockFluentTheme_Is_Active()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        app.Styles.Add(new DockFluentTheme());
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));

        var host = new Border();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = host
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(2);
            Dispatcher.UIThread.RunJobs();
            AssertBrushColor(host, "DockSurfaceSidebarBrush", Color.Parse("#FF252526"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Update_DocumentTabStripItem_Styled_Brushes()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        app.Styles.Add(new DockFluentTheme());
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));

        var item = new DocumentTabStripItem();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = item
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(2); // VS Code Dark
            Dispatcher.UIThread.RunJobs();

            var foregroundBrush = Assert.IsAssignableFrom<ISolidColorBrush>(item.Foreground);
            var borderBrush = Assert.IsAssignableFrom<ISolidColorBrush>(item.BorderBrush);

            Assert.Equal(Color.Parse("#FFCCCCCC"), foregroundBrush.Color);
            Assert.Equal(Color.Parse("#FF3E3E42"), borderBrush.Color);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Insert_Preset_Into_DockTheme_Resources_By_Default()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockFluentTheme();
        app.Styles.Add(dockTheme);

        manager.SwitchPreset(4);

        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal(1, CountPresetIncludes(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderDark.axaml", FindFirstPresetUri(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Move_AppPreset_Include_To_DockTheme_Resources()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockFluentTheme();
        app.Styles.Add(dockTheme);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));

        manager.SwitchPreset(3);

        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal(1, CountPresetIncludes(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/RiderLight.axaml", FindFirstPresetUri(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
    }

    [AvaloniaFact]
    public void Simple_Manager_Should_Insert_Preset_Into_DockTheme_Resources_By_Default()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockSimpleThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockSimpleTheme();
        app.Styles.Add(dockTheme);

        manager.SwitchPreset(2);

        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal(1, CountPresetIncludes(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/VsCodeDark.axaml", FindFirstPresetUri(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
    }

    [AvaloniaFact]
    public void Simple_Manager_Should_Move_AppPreset_Include_To_DockTheme_Resources()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockSimpleThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockSimpleTheme();
        app.Styles.Add(dockTheme);
        app.Resources.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml"));

        manager.SwitchPreset(4);

        Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal(1, CountPresetIncludes(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml", FindFirstPresetUri(app.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
        Assert.Equal("avares://Dock.Avalonia.Themes.Simple/Presets/Ide/RiderDark.axaml", FindFirstPresetUri(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/"));
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Work_With_Nested_AppResource_Preset_Structure()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockFluentTheme();
        app.Styles.Add(dockTheme);

        var nestedPresetHost = new ResourceDictionary();
        nestedPresetHost.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));
        var outerHost = new ResourceDictionary();
        outerHost.MergedDictionaries.Add(nestedPresetHost);
        app.Resources.MergedDictionaries.Add(outerHost);

        var item = new DocumentTabStripItem();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = item
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(3); // Rider Light
            Dispatcher.UIThread.RunJobs();

            var foregroundBrush = Assert.IsAssignableFrom<ISolidColorBrush>(item.Foreground);
            Assert.Equal(Color.Parse("#FF2C2C2C"), foregroundBrush.Color);

            Assert.Equal(1, CountPresetIncludes(app.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
            Assert.Equal(1, CountPresetIncludes(dockTheme.Resources, "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Use_NonDefault_GripBrush_After_Runtime_Preset_Switch()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        using var scope = new AppResourcesAndStylesScope(app);
        var dockTheme = new DockFluentTheme();
        app.Styles.Add(dockTheme);

        var nestedPresetHost = new ResourceDictionary();
        nestedPresetHost.MergedDictionaries.Add(CreatePresetInclude("avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml"));
        var outerHost = new ResourceDictionary();
        outerHost.MergedDictionaries.Add(nestedPresetHost);
        app.Resources.MergedDictionaries.Add(outerHost);

        var host = new Border();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = host
        };

        window.Show();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            manager.SwitchPreset(2); // VS Code Dark
            Dispatcher.UIThread.RunJobs();

            Assert.True(host.TryFindResource("DockChromeGripBrush", out object? value));
            Assert.NotNull(value);
            Assert.IsNotType<VisualBrush>(value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Clear_Default_Grip_Pattern_On_Runtime_Switch_To_VsCodeDark()
    {
        var manager = new DockFluentThemeManager();

        AssertRuntimeSwitchClearsGripPattern(
            manager,
            new DockFluentTheme(),
            "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml",
            2);
    }

    [AvaloniaFact]
    public void Fluent_Manager_Should_Clear_Default_Grip_Pattern_On_Runtime_Switch_To_RiderDark()
    {
        var manager = new DockFluentThemeManager();

        AssertRuntimeSwitchClearsGripPattern(
            manager,
            new DockFluentTheme(),
            "avares://Dock.Avalonia.Themes.Fluent/Presets/Ide/Default.axaml",
            4);
    }

    [AvaloniaFact]
    public void Simple_Manager_Should_Clear_Default_Grip_Pattern_On_Runtime_Switch_To_VsCodeDark()
    {
        var manager = new DockSimpleThemeManager();

        AssertRuntimeSwitchClearsGripPattern(
            manager,
            new DockSimpleTheme(),
            "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml",
            2);
    }

    [AvaloniaFact]
    public void Simple_Manager_Should_Clear_Default_Grip_Pattern_On_Runtime_Switch_To_RiderDark()
    {
        var manager = new DockSimpleThemeManager();

        AssertRuntimeSwitchClearsGripPattern(
            manager,
            new DockSimpleTheme(),
            "avares://Dock.Avalonia.Themes.Simple/Presets/Ide/Default.axaml",
            4);
    }

    [AvaloniaFact]
    public void Manager_Should_Switch_Light_And_Dark_Variants()
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");
        var manager = new DockFluentThemeManager();

        ThemeVariant? previous = app.RequestedThemeVariant;
        try
        {
            manager.Switch(0);
            Assert.Equal(ThemeVariant.Light, app.RequestedThemeVariant);

            manager.Switch(1);
            Assert.Equal(ThemeVariant.Dark, app.RequestedThemeVariant);
        }
        finally
        {
            app.RequestedThemeVariant = previous;
        }
    }

    [AvaloniaFact]
    public void ResourceDictionary_MergedDictionary_Order_Should_Be_Reverse_Search()
    {
        var host = new Border();
        var owner = new ResourceDictionary();

        var first = new ResourceDictionary
        {
            ["TestBrush"] = new SolidColorBrush(Color.Parse("#FFFF0000"))
        };

        var second = new ResourceDictionary
        {
            ["TestBrush"] = new SolidColorBrush(Color.Parse("#FF0000FF"))
        };

        owner.MergedDictionaries.Add(first);
        owner.MergedDictionaries.Add(second);
        host.Resources = owner;

        Assert.True(host.TryFindResource("TestBrush", out object? value));
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(value);
        Assert.Equal(Color.Parse("#FF0000FF"), brush.Color);
    }

    private static int CountPresetIncludes(IResourceDictionary dictionary, string prefix)
    {
        int count = 0;

        foreach (IResourceProvider mergedDictionary in dictionary.MergedDictionaries)
        {
            if (mergedDictionary is ResourceInclude include &&
                include.Source is { } source &&
                source.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }

            if (mergedDictionary is IResourceDictionary mergedResourceDictionary)
            {
                count += CountPresetIncludes(mergedResourceDictionary, prefix);
            }
        }

        return count;
    }

    private static string? FindFirstPresetUri(IResourceDictionary dictionary, string prefix)
    {
        foreach (IResourceProvider mergedDictionary in dictionary.MergedDictionaries)
        {
            if (mergedDictionary is ResourceInclude include &&
                include.Source is { } source &&
                source.ToString().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return source.ToString();
            }

            if (mergedDictionary is IResourceDictionary mergedResourceDictionary)
            {
                string? nestedResult = FindFirstPresetUri(mergedResourceDictionary, prefix);
                if (nestedResult is not null)
                {
                    return nestedResult;
                }
            }
        }

        return null;
    }

    private sealed class AppResourcesScope : IDisposable
    {
        private readonly Application _application;
        private readonly List<IResourceProvider> _previousMergedDictionaries;
        private readonly List<IStyle> _previousStyles;

        public AppResourcesScope(Application application)
        {
            _application = application;
            _previousMergedDictionaries = _application.Resources.MergedDictionaries.ToList();
            _previousStyles = _application.Styles.ToList();
            _application.Resources.MergedDictionaries.Clear();
            _application.Styles.Clear();
        }

        public void Dispose()
        {
            _application.Resources.MergedDictionaries.Clear();
            foreach (IResourceProvider mergedDictionary in _previousMergedDictionaries)
            {
                _application.Resources.MergedDictionaries.Add(mergedDictionary);
            }

            _application.Styles.Clear();
            foreach (IStyle style in _previousStyles)
            {
                _application.Styles.Add(style);
            }
        }
    }

    private sealed class AppResourcesAndStylesScope : IDisposable
    {
        private readonly Application _application;
        private readonly List<IResourceProvider> _previousMergedDictionaries;
        private readonly List<IStyle> _previousStyles;

        public AppResourcesAndStylesScope(Application application)
        {
            _application = application;
            _previousMergedDictionaries = _application.Resources.MergedDictionaries.ToList();
            _previousStyles = _application.Styles.ToList();

            _application.Resources.MergedDictionaries.Clear();
            _application.Styles.Clear();
        }

        public void Dispose()
        {
            _application.Resources.MergedDictionaries.Clear();
            foreach (IResourceProvider mergedDictionary in _previousMergedDictionaries)
            {
                _application.Resources.MergedDictionaries.Add(mergedDictionary);
            }

            _application.Styles.Clear();
            foreach (IStyle style in _previousStyles)
            {
                _application.Styles.Add(style);
            }
        }
    }

    private static ResourceInclude CreatePresetInclude(string uri)
    {
        var presetUri = new Uri(uri);
#pragma warning disable IL2026
        return new ResourceInclude(presetUri)
        {
            Source = presetUri
        };
#pragma warning restore IL2026
    }

    private static void AssertRuntimeSwitchClearsGripPattern(
        IDockThemeManager manager,
        Styles baseTheme,
        string defaultPresetUri,
        int targetPresetIndex)
    {
        Application app = Application.Current ?? throw new InvalidOperationException("Avalonia application is not initialized.");

        using var scope = new AppResourcesAndStylesScope(app);
        app.Styles.Add(new FluentTheme());
        app.Styles.Add(baseTheme);

        var nestedPresetHost = new ResourceDictionary();
        nestedPresetHost.MergedDictionaries.Add(CreatePresetInclude(defaultPresetUri));
        var outerHost = new ResourceDictionary();
        outerHost.MergedDictionaries.Add(nestedPresetHost);
        app.Resources.MergedDictionaries.Add(outerHost);

        var toolChrome = new ToolChromeControl();
        var window = new Window
        {
            Width = 400,
            Height = 300,
            Content = toolChrome
        };

        window.Show();
        window.UpdateLayout();
        toolChrome.ApplyTemplate();
        toolChrome.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var beforeSwitchGrip = GetGripGrid(toolChrome);
            Assert.IsType<VisualBrush>(beforeSwitchGrip.Background);

            manager.SwitchPreset(targetPresetIndex);
            Dispatcher.UIThread.RunJobs();

            toolChrome.ApplyTemplate();
            toolChrome.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var afterSwitchGrip = GetGripGrid(toolChrome);
            Assert.NotNull(afterSwitchGrip.Background);
            Assert.IsNotType<VisualBrush>(afterSwitchGrip.Background);
        }
        finally
        {
            window.Close();
        }
    }

    private static Grid GetGripGrid(ToolChromeControl toolChrome)
    {
        Grid? gripGrid = toolChrome.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(x => x.Name == "PART_Grid");

        return Assert.IsType<Grid>(gripGrid);
    }

    private static void AssertBrushColor(Control host, string resourceKey, Color expectedColor)
    {
        Assert.True(host.TryFindResource(resourceKey, out object? resourceValue), $"Missing resource '{resourceKey}'.");
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(resourceValue);
        Assert.Equal(expectedColor, brush.Color);
    }
}
