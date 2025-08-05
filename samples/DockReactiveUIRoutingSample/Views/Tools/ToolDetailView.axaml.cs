using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DockReactiveUIRoutingSample.ViewModels.Tools;

namespace DockReactiveUIRoutingSample.Views.Tools;

public partial class ToolDetailView : ReactiveUserControl<ToolDetailViewModel>
{
    public ToolDetailView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}