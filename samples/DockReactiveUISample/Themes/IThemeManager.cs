using Avalonia;

namespace DockReactiveUISample.Themes;

public interface IThemeManager
{
    void Initialize(Application application);

    void Switch(int index);
}
