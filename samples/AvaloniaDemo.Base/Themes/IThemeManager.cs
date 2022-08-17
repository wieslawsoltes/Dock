using Avalonia;

namespace AvaloniaDemo.Themes;

public interface IThemeManager
{
    void Initialize(Application application);

    void Switch(int index);
}
