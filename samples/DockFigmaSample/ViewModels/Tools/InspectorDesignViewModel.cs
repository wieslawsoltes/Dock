using ReactiveUI;

namespace DockFigmaSample.ViewModels.Tools;

public class InspectorDesignViewModel : ReactiveObject, IRoutableViewModel
{
    public InspectorDesignViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "inspector-design";
    public IScreen HostScreen { get; }

    public string Width { get; } = "1280";
    public string Height { get; } = "720";
    public string Radius { get; } = "24";
    public string Fill { get; } = "#FFFFFF";
    public string Opacity { get; } = "100";
    public string Layout { get; } = "Auto";
    public string Spacing { get; } = "24";
}
