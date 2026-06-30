using System.Collections.Generic;

namespace DockReactiveUIFlatSample.ViewModels;

internal interface IThemeService
{
    IReadOnlyList<string> PresetNames { get; }

    int CurrentPresetIndex { get; }

    bool IsDark { get; }

    void SwitchDark(bool isDark);

    void SwitchPreset(int index);
}
