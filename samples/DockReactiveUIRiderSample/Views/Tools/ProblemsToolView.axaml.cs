using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRiderSample.Views.Tools;

public partial class ProblemsToolView : UserControl
{
    public ProblemsToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
