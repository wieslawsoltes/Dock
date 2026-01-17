using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI.Avalonia;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class ToolPanelView : ReactiveUserControl<ToolPanelViewModel>
{
    public ToolPanelView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
