using System.Collections.Generic;
using Avalonia;
using Avalonia.Styling;
using Dock.Avalonia.Themes;
using DockReactiveUIFlatSample.ViewModels;

namespace DockReactiveUIFlatSample.Services;

internal sealed class DockThemeService : IThemeService
{
    private readonly IDockThemeManager _themeManager;

    public DockThemeService(IDockThemeManager themeManager)
    {
        _themeManager = themeManager;
    }

    public IReadOnlyList<string> PresetNames => _themeManager.PresetNames;

    public int CurrentPresetIndex => _themeManager.CurrentPresetIndex;

    public bool IsDark => Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;

    public void SwitchDark(bool isDark)
    {
        _themeManager.Switch(isDark ? 1 : 0);
    }

    public void SwitchPreset(int index)
    {
        _themeManager.SwitchPreset(index);
    }
}
