using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockControlPanelsSample;

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