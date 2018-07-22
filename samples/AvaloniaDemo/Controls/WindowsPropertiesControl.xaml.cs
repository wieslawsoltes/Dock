using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Controls
{
    public class WindowsPropertiesControl : UserControl
    {
        public WindowsPropertiesControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
