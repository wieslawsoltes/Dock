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
                bool isEnabled = (bool)GetValue(DockControl.IsDragEnabledProperty);
                SetValue(DockControl.IsDragEnabledProperty, !isEnabled);
            };

            this.FindControl<MenuItem>("OptionsIsDropEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DockControl.IsDropEnabledProperty);
                SetValue(DockControl.IsDropEnabledProperty, !isEnabled);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
