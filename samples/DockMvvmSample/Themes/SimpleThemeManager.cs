using System;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Simple;

namespace DockMvvmSample.Themes;

public class SimpleThemeManager : IThemeManager
{
    private static readonly Uri BaseUri = new("avares://DockMvvmSample.Base/Styles");

    private static readonly SimpleTheme Simple = new()
    {
        Mode = SimpleThemeMode.Light
    };

    private static readonly Styles DockSimple = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://Dock.Avalonia/Themes/DockSimpleTheme.axaml")
        }
    };

    private static readonly Styles SimpleLight = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://DockMvvmSample.Base/Themes/SimpleLight.axaml")
        }
    };

    private static readonly Styles SimpleDark = new()
    {
        new StyleInclude(BaseUri)
        {
            Source = new Uri("avares://DockMvvmSample.Base/Themes/SimpleDark.axaml")
        }
    };

    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        switch (index)
        {
            // Simple Light
            case 0:
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
            case 1:
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

    public void Initialize(Application application)
    {
        application.Styles.Insert(0, Simple);
        application.Styles.Insert(1, DockSimple);
        application.Styles.Insert(2, SimpleDark);
    }
}
