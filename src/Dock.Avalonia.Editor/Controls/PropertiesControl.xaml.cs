using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Editor.Controls
{
    public class PropertiesControl : UserControl
    {
        public PropertiesControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
