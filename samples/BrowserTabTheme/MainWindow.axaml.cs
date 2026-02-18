using Avalonia.Markup.Xaml;

namespace BrowserTabTheme;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
