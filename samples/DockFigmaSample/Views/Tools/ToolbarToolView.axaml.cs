using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class ToolbarToolView : ReactiveUserControl<ToolbarToolViewModel>
{
    public ToolbarToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
