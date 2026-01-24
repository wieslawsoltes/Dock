using System.Collections.ObjectModel;
using ReactiveUI;

namespace DockFigmaSample.ViewModels.Documents;

public class PrototypeCanvasViewModel : ReactiveObject, IRoutableViewModel
{
    public PrototypeCanvasViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "prototype-canvas";
    public IScreen HostScreen { get; }

    public ObservableCollection<string> Frames { get; } = new()
    {
        "Landing Page",
        "Pricing Modal",
        "Checkout"
    };
}
