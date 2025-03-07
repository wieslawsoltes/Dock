using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockReactiveUISample.Views.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
