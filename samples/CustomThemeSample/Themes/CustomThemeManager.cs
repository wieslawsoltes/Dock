using System;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace CustomThemeSample.Themes;

public class CustomThemeManager : IThemeManager
{
    private readonly Uri _lightUri = new("avares://CustomThemeSample/Themes/LightDockTheme.axaml");
    private readonly Uri _darkUri = new("avares://CustomThemeSample/Themes/DarkDockTheme.axaml");
    private IStyle? _current;

    public CustomThemeManager()
    {
        if (Application.Current is { } app)
        {
            _current = app.Styles.OfType<StyleInclude>()
                .FirstOrDefault(x => x.Source == _lightUri);
        }
    }

    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        var uri = index == 0 ? _lightUri : _darkUri;

        if (_current is StyleInclude current && current.Source == uri)
        {
            return;
        }

        var style = new StyleInclude(new Uri("avares://CustomThemeSample/"))
        {
            Source = uri
        };

        if (_current != null)
        {
            Application.Current.Styles.Remove(_current);
        }

        Application.Current.Styles.Add(style);
        Application.Current.RequestedThemeVariant = index == 0 ? ThemeVariant.Light : ThemeVariant.Dark;
        _current = style;
    }
}
