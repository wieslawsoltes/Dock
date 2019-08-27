using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Dock.Avalonia.Controls;

namespace AvaloniaDemo.Views
{
    public class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
