using ReactiveUI;

namespace DockFigmaSample.ViewModels.Documents;

public class InspectCanvasViewModel : ReactiveObject, IRoutableViewModel
{
    public InspectCanvasViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "inspect-canvas";
    public IScreen HostScreen { get; }

    public string SelectedLayer { get; } = "Hero / CTA";
    public string Measurements { get; } = "320 x 56";
}
