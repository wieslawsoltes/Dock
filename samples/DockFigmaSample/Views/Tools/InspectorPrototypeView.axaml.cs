using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorPrototypeView : ReactiveUserControl<InspectorPrototypeViewModel>
{
    public InspectorPrototypeView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
