using Avalonia.Markup.Xaml;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services.Avalonia.Controls;

namespace DockReactiveUICanonicalSample.Views.Workspace;

public partial class ToolPanelPageView : DockReactiveUserControl<ToolPanelPageViewModel>
{
    public ToolPanelPageView()
    {
        InitializeComponent();
    }
}
