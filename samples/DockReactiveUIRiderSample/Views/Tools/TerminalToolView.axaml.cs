using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRiderSample.Views.Tools;

public partial class TerminalToolView : UserControl
{
    public TerminalToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
