using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Tools;

public partial class AssetsToolView : ReactiveUserControl<AssetsToolViewModel>
{
    public AssetsToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
