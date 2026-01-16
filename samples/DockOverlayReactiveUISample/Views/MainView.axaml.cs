using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockOverlayReactiveUISample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
