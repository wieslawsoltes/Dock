using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolCrossNavigationView : ReactiveUserControl<ToolCrossNavigationViewModel>
{
    public ToolCrossNavigationView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
