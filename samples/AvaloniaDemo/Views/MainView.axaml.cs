using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
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
    }
}
