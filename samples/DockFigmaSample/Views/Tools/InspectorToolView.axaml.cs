using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorToolView : ReactiveUserControl<InspectorToolViewModel>
{
    public InspectorToolView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
