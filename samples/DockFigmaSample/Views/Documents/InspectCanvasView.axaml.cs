using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Documents;

public partial class InspectCanvasView : ReactiveUserControl<InspectCanvasViewModel>
{
    public InspectCanvasView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
