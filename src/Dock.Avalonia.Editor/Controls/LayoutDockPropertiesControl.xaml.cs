using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Editor.Controls
{
    public class LayoutDockPropertiesControl : UserControl
    {
        public LayoutDockPropertiesControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
