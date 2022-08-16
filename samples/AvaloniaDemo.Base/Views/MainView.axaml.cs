using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;

namespace AvaloniaDemo.Views;

public class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        InitializeThemes();
        InitializeMenu();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeThemes()
    {
        var themes = this.Find<ComboBox>("Themes");
        if (themes is { })
        {
            themes.SelectionChanged += (_, _) => ThemeManager.Switch(themes.SelectedIndex);
        }
    }

    private void InitializeMenu()
    {
        var optionsIsDragEnabled = this.FindControl<MenuItem>("OptionsIsDragEnabled");
        if (optionsIsDragEnabled is { })
        {
            optionsIsDragEnabled.Click += (_, _) =>
            {
                if (VisualRoot is Window window)
                {
                    var isEnabled = window.GetValue(DockProperties.IsDragEnabledProperty);
                    window.SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
                }
            };
        }

        var optionsIsDropEnabled = this.FindControl<MenuItem>("OptionsIsDropEnabled");
        if (optionsIsDropEnabled is { })
        {
            optionsIsDropEnabled.Click += (_, _) =>
            {
                if (VisualRoot is Window window)
                {
                    var isEnabled = window.GetValue(DockProperties.IsDropEnabledProperty);
                    window.SetValue(DockProperties.IsDropEnabledProperty, !isEnabled);
                }
            };
        }
    }
}
