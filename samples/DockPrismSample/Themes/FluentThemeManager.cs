using Avalonia;
using Avalonia.Styling;

namespace DockPrismSample.Themes;

public class FluentThemeManager : IThemeManager
{
    public void Switch(int index)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = index switch
        {
            0 => ThemeVariant.Light,
            1 => ThemeVariant.Dark,
            _ => Application.Current.RequestedThemeVariant
        };
    }
}
