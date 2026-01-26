using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorDesignView : ReactiveUserControl<InspectorDesignViewModel>
{
    public InspectorDesignView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
