using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Documents;

public partial class DesignCanvasView : ReactiveUserControl<DesignCanvasViewModel>
{
    public DesignCanvasView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
