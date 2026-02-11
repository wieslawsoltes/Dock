using Avalonia.Markup.Xaml;
using DockFigmaSample.ViewModels.Tools;
using Avalonia.ReactiveUI;

namespace DockFigmaSample.Views.Tools;

public partial class InspectorInspectView : ReactiveUserControl<InspectorInspectViewModel>
{
    public InspectorInspectView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
