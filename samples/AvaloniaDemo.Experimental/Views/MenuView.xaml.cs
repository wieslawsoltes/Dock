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
            this.InitializeComponent();

            this.FindControl<MenuItem>("OptionsIsDragEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DockProperties.IsDragEnabledProperty);
                SetValue(DockProperties.IsDragEnabledProperty, !isEnabled);
            };

            this.FindControl<MenuItem>("OptionsIsDropEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DockProperties.IsDropEnabledProperty);
                SetValue(DockProperties.IsDropEnabledProperty, !isEnabled);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
