using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolComparisonView : ReactiveUserControl<ToolComparisonViewModel>
{
    public ToolComparisonView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
