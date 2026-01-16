using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DockOverlayReactiveUISample.Views.Tools;

public partial class DashboardActionsView : UserControl
{
    public DashboardActionsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
