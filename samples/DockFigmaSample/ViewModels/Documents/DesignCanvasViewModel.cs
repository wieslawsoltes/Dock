using ReactiveUI;

namespace DockFigmaSample.ViewModels.Documents;

public class DesignCanvasViewModel : ReactiveObject, IRoutableViewModel
{
    public DesignCanvasViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "design-canvas";
    public IScreen HostScreen { get; }

    public string FrameName { get; } = "Landing Page";
    public string FrameSize { get; } = "1440 x 900";
}
