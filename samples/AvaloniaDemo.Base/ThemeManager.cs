using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;

namespace AvaloniaDemo;

public static class ThemeManager
{
    private static FluentTheme Fluent = new(new Uri("avares://ControlCatalog/Styles"))
    {
        Mode = FluentThemeMode.Light
    };

    private static SimpleTheme Simple = new(new Uri("avares://ControlCatalog/Styles"))
    {
        Mode = SimpleThemeMode.Light
    };

    private static readonly Styles DockFluent = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockFluentTheme.axaml")
        }
    };

    private static readonly Styles DockSimple = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockSimpleTheme.axaml")
        }
    };

    private static readonly Styles FluentDark = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/FluentDark.axaml")
        }
    };

    private static readonly Styles FluentLight = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/FluentLight.axaml")
        }
    };

    private static readonly Styles SimpleLight = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/SimpleLight.axaml")
        }
    };

    private static readonly Styles SimpleDark = new()
    {
        new StyleInclude(new Uri("avares://AvaloniaDemo.Base/Styles"))
        {
            Source = new Uri("avares://AvaloniaDemo.Base/Themes/SimpleDark.axaml")
        }
    };

    public static void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        switch (index)
        {
            // Fluent Light
            case 0:
            {
                if (Fluent.Mode != FluentThemeMode.Light)
                {
                    Fluent.Mode = FluentThemeMode.Light;
                }
                Application.Current.Styles[0] = Fluent;
                Application.Current.Styles[1] = DockFluent;
                Application.Current.Styles[2] = FluentLight;
                break;
            }
            // Fluent Dark
            case 1:
            {
                if (Fluent.Mode != FluentThemeMode.Dark)
                {
                    Fluent.Mode = FluentThemeMode.Dark;
                }
                Application.Current.Styles[0] = Fluent;
                Application.Current.Styles[1] = DockFluent;
                Application.Current.Styles[2] = FluentDark;
                break;
            }
            // Simple Light
            case 2:
            {
                if (Simple.Mode != SimpleThemeMode.Light)
                {
                    Simple.Mode = SimpleThemeMode.Light;
                }
                Application.Current.Styles[0] = Simple;
                Application.Current.Styles[1] = DockSimple;
                Application.Current.Styles[2] = SimpleLight;
                break;
            }
            // Simple Dark
            case 3:
            {
                if (Simple.Mode != SimpleThemeMode.Dark)
                {
                    Simple.Mode = SimpleThemeMode.Dark;
                }
                Application.Current.Styles[0] = Simple;
                Application.Current.Styles[1] = DockSimple;
                Application.Current.Styles[2] = SimpleDark;
                break;
            }
        }
    }

    public static void Initialize(Application application)
    {
#if true
        application.Styles.Insert(0, Fluent);
        application.Styles.Insert(1, DockFluent);
        application.Styles.Insert(2, FluentLight);
#else
        application..Styles.Insert(0, Simple);
        application..Styles.Insert(1, DockSimple);
        application..Styles.Insert(2, SimpleDark);
#endif
    }
}
