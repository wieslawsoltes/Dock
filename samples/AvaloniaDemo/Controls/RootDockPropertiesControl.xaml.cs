using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Controls
{
    public class RootDockPropertiesControl : UserControl
    {
        public RootDockPropertiesControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
