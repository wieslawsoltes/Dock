using System.Collections.ObjectModel;
using DockFigmaSample.Models;
using ReactiveUI;

namespace DockFigmaSample.ViewModels.Tools;

public class InspectorPrototypeViewModel : ReactiveObject, IRoutableViewModel
{
    public InspectorPrototypeViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "inspector-prototype";
    public IScreen HostScreen { get; }

    public ObservableCollection<PrototypeFlowItem> Flows { get; } = SampleData.CreatePrototypeFlows();
}
