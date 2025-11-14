using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolCrossNavigationDetailView : ReactiveUserControl<ToolCrossNavigationDetailViewModel>
{
    public ToolCrossNavigationDetailView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}