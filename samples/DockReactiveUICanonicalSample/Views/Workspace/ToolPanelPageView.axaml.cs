using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class ToolPanelPageView : ReactiveUserControl<ToolPanelPageViewModel>
{
    public ToolPanelPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
