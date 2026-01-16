using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolDetailView : ReactiveUserControl<ToolDetailViewModel>
{
    public ToolDetailView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}