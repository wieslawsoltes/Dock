using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Views
{
    public class ProportionalStackPanelView : UserControl
    {
        public ProportionalStackPanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
