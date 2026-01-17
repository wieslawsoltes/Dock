using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class RibbonPageView : ReactiveUserControl<RibbonPageViewModel>
{
    public RibbonPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
