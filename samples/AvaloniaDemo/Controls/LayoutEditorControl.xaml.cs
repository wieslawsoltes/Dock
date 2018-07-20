using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Controls
{
    public class LayoutEditorControl : UserControl
    {
        public LayoutEditorControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
