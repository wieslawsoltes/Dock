using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Notepad.Views.Documents
{
    public class FileView : UserControl
    {
        public FileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
