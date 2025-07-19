using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockChromeSample.Views.Documents;

public partial class FileView : UserControl
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
