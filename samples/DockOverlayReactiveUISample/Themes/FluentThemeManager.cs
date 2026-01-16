using System.Linq;
using Avalonia;
using Avalonia.Styling;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;

namespace DockOverlayReactiveUISample.Themes;

public class FluentThemeManager : IThemeManager
{
    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        var styles = Application.Current.Styles;

        // Remove any existing dock theme styles before applying a new one.
        for (var i = styles.Count - 1; i >= 0; i--)
        {
            if (styles[i] is DockFluentTheme || styles[i] is DockSimpleTheme)
            {
                styles.RemoveAt(i);
            }
        }

        switch (index)
        {
            case 1:
                styles.Add(new DockSimpleTheme());
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                break;
            default:
                styles.Add(new DockFluentTheme());
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                break;
        }
    }
}
