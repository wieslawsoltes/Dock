using Avalonia.Markup.Xaml;

namespace DockSimplifiedFluentSample;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        Title = "Dock Simplified Fluent Sample";
        Width = 800;
        Height = 600;
    }
}


