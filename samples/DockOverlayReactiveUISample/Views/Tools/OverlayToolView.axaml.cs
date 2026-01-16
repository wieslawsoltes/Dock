using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockOverlayReactiveUISample.Views.Tools;

public partial class OverlayToolView : UserControl
{
    public OverlayToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
