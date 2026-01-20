using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class ToolPanelView : DockReactiveUserControl<ToolPanelViewModel>
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
