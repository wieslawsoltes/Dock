using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class RibbonPageView : DockReactiveUserControl<RibbonPageViewModel>
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
