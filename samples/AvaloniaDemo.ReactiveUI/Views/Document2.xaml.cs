using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.ReactiveUI.Views
{
    public class Document2 : UserControl
    {
        public Document2()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
