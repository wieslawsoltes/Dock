using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class AssetsToolView : ReactiveUserControl<AssetsToolViewModel>
{
    public AssetsToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
