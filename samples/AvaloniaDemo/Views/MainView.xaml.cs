using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Avalonia;

namespace AvaloniaDemo.Views
{
    public class MainView : UserControl
    {
        public MainView()
        {
            this.InitializeComponent();

            this.FindControl<MenuItem>("OptionsDragBehaviorIsEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DragBehavior.IsEnabledProperty);
                SetValue(DragBehavior.IsEnabledProperty, !isEnabled);
            };

            this.FindControl<MenuItem>("OptionsDropBehaviorIsEnabled").Click += (sender, e) =>
            {
                bool isEnabled = (bool)GetValue(DropBehavior.IsEnabledProperty);
                SetValue(DropBehavior.IsEnabledProperty, !isEnabled);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
