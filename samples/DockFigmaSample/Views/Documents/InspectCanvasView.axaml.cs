using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Documents;

public partial class InspectCanvasView : ReactiveUserControl<InspectCanvasViewModel>
{
    public InspectCanvasView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
