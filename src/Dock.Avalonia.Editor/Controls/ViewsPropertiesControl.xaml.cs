using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dock.Avalonia.Editor.Controls
{
    public class ViewsPropertiesControl : UserControl
    {
        public ViewsPropertiesControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
