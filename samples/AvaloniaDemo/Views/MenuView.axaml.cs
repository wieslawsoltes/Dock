using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;
using Dock.Avalonia.Controls;

namespace AvaloniaDemo.Views
{
    public class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();

            this.FindControl<MenuItem>("OptionsIsDragEnabled").Click += (sender, e) =>
            {
                if (VisualRoot is Window window)
                {
                    bool isEnabled = window.GetValue(DockProperties.IsDragEnabledProperty);
                    window.SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
                }
            };

            this.FindControl<MenuItem>("OptionsIsDropEnabled").Click += (sender, e) =>
            {
                if (VisualRoot is Window window)
                {
                    bool isEnabled = window.GetValue(DockProperties.IsDropEnabledProperty);
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
