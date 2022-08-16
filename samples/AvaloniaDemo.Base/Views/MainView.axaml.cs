using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
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
            themes.SelectionChanged += (_, _) =>
            {
                if (Application.Current is { })
                {
                    var index = themes.SelectedIndex;

                    switch (index)
                    {
                        // Fluent Light
                        case 0:
                        {
                            if (App.Fluent.Mode != FluentThemeMode.Light)
                            {
                                App.Fluent.Mode = FluentThemeMode.Light;
                            }
                            Application.Current.Styles[0] = App.Fluent;
                            Application.Current.Styles[1] = App.DockFluent;
                            Application.Current.Styles[2] = App.FluentLight;
                            break;
                        }
                        // Fluent Dark
                        case 1:
                        {
                            if (App.Fluent.Mode != FluentThemeMode.Dark)
                            {
                                App.Fluent.Mode = FluentThemeMode.Dark;
                            }
                            Application.Current.Styles[0] = App.Fluent;
                            Application.Current.Styles[1] = App.DockFluent;
                            Application.Current.Styles[2] = App.FluentDark;
                            break;
                        }
                        // Simple Light
                        case 2:
                        {
                            if (App.Simple.Mode != SimpleThemeMode.Light)
                            {
                                App.Simple.Mode = SimpleThemeMode.Light;
                            }
                            Application.Current.Styles[0] = App.Simple;
                            Application.Current.Styles[1] = App.DockSimple;
                            Application.Current.Styles[2] = App.SimpleLight;
                            break;
                        }
                        // Simple Dark
                        case 3:
                        {
                            if (App.Simple.Mode != SimpleThemeMode.Dark)
                            {
                                App.Simple.Mode = SimpleThemeMode.Dark;
                            }
                            Application.Current.Styles[0] = App.Simple;
                            Application.Current.Styles[1] = App.DockSimple;
                            Application.Current.Styles[2] = App.SimpleDark;
                            break;
                        }
                    }
                }
            };
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
