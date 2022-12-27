using Avalonia;

namespace DockMvvmSample.Themes;

public interface IThemeManager
{
    void Initialize(Application application);

    void Switch(int index);
}
