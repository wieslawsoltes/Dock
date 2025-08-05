using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolView : ReactiveUserControl<ToolViewModel>
{
    public ToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
