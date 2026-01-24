using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Documents;

public partial class PrototypeCanvasView : ReactiveUserControl<PrototypeCanvasViewModel>
{
    public PrototypeCanvasView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
