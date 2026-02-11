using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolHomeView : ReactiveUserControl<ToolHomeViewModel>
{
    public ToolHomeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
