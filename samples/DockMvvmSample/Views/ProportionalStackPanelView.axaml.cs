using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockMvvmSample.Views;

public partial class ProportionalStackPanelView : UserControl
{
    public ProportionalStackPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
