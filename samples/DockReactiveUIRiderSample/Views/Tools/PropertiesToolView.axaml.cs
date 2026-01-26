using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRiderSample.Views.Tools;

public partial class PropertiesToolView : UserControl
{
    public PropertiesToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
