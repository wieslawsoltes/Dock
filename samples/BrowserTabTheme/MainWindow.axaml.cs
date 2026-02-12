using Avalonia.Markup.Xaml;

namespace BrowserTabTheme;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Title = "BrowserTabTheme Sample";
        Width = 1300;
        Height = 820;
    }
}

