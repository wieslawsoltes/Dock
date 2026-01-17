using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class RibbonToolView : ReactiveUserControl<RibbonToolViewModel>
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
