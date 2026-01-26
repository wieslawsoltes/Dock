using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI.Avalonia;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorInspectView : ReactiveUserControl<InspectorInspectViewModel>
{
    public InspectorInspectView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
