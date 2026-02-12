using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace DockReactiveUISample.Views;

[RequiresUnreferencedCode("Requires unreferenced code for ThemeManager.")]
[RequiresDynamicCode("Requires unreferenced code for ThemeManager.")]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        InitializeThemes();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeThemes()
    {
        var themeManager = App.ThemeManager;

        if (themeManager is null)
        {
            return;
        }

        var dark = Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;
        var theme = this.Find<Button>("ThemeButton");
        if (theme is not null)
        {
            theme.Click += (_, _) =>
            {
                dark = !dark;
                themeManager.Switch(dark ? 1 : 0);
            };
        }

        var preset = this.Find<ComboBox>("PresetComboBox");
        if (preset is not null)
        {
            preset.ItemsSource = themeManager.PresetNames;
            preset.SelectedIndex = themeManager.CurrentPresetIndex;
            preset.SelectionChanged += (_, _) =>
            {
                if (preset.SelectedIndex >= 0)
                {
                    themeManager.SwitchPreset(preset.SelectedIndex);
                }
            };
        }
    }
}
