using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorDesignView : ReactiveUserControl<InspectorDesignViewModel>
{
    public InspectorDesignView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
