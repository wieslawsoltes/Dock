using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Documents;

public partial class PrototypeCanvasView : ReactiveUserControl<PrototypeCanvasViewModel>
{
    public PrototypeCanvasView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
