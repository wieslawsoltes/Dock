using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;

namespace AvaloniaDemo.Views
{
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

            themes.SelectionChanged += (_, _) =>
            {
                Application.Current.Styles[0] = themes.SelectedIndex switch
                {
                    0 => App.FluentLight,
                    1 => App.FluentDark,
                    2 => App.DefaultLight,
                    3 => App.DefaultDark,
                    _ => throw new Exception("Not support theme.")
                };
            };
        }

        private void InitializeMenu()
        {
            this.FindControl<MenuItem>("OptionsIsDragEnabled").Click += (_, _) =>
            {
                if (VisualRoot is Window window)
                {
                    var isEnabled = window.GetValue(DockProperties.IsDragEnabledProperty);
                    window.SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
                }
            };

            this.FindControl<MenuItem>("OptionsIsDropEnabled").Click += (_, _) =>
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
