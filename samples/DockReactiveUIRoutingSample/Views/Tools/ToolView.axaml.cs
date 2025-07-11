using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolView : UserControl
{
    public ToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
