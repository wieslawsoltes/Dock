using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolCrossNavigationDetailView : ReactiveUserControl<ToolCrossNavigationDetailViewModel>
{
    public ToolCrossNavigationDetailView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
