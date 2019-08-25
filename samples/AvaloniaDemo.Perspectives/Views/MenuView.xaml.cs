﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;
using Dock.Avalonia.Controls;

namespace AvaloniaDemo.Views
{
    public class MenuView : UserControl
    {
        public MenuView()
        {
            this.InitializeComponent();

            this.FindControl<MenuItem>("OptionsIsDragEnabled").Click += (sender, e) =>
            {
                if (VisualRoot is Window window)
                {
                    bool isEnabled = (bool)window.GetValue(DockProperties.IsDragEnabledProperty);
                    window.SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
                }
            };

            this.FindControl<MenuItem>("OptionsIsDropEnabled").Click += (sender, e) =>
            {
                if (VisualRoot is Window window)
                {
                    bool isEnabled = (bool)window.GetValue(DockProperties.IsDropEnabledProperty);
                    window.SetValue(DockProperties.IsDropEnabledProperty, !isEnabled);
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
