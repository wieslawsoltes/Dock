using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Notepad.Views.Tools
{
    public class ReplaceView : UserControl
    {
        public ReplaceView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
