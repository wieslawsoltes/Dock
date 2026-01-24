using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Documents;

public partial class CanvasDocumentView : ReactiveUserControl<CanvasDocumentViewModel>
{
    public CanvasDocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
