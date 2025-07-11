using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUIRoutingSample.Views.Inner;

public partial class InnerView : UserControl
{
    public InnerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
