using Avalonia.Markup.Xaml;

namespace DockSimplifiedSample;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Title = "Dock Simplified Sample";
        Width = 800;
        Height = 600;
    }
}
