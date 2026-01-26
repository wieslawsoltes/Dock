using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRiderSample.Views.Tools;

public partial class StructureToolView : UserControl
{
    public StructureToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
