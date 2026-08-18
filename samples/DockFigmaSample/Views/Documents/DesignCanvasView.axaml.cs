using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Documents;
using ReactiveUI.Avalonia.Reactive;

namespace DockFigmaSample.Views.Documents;

public partial class DesignCanvasView : ReactiveUserControl<DesignCanvasViewModel>
{
    public DesignCanvasView()
    {
        InitializeComponent();
    }
}
