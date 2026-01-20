using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class RibbonToolView : DockReactiveUserControl<RibbonToolViewModel>
{
    public RibbonToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
