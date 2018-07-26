using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.INPC.Views
{
    public class Document1 : UserControl
    {
        public Document1()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
