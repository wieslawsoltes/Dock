using System.Collections.ObjectModel;
using DockFigmaSample.Models;
using ReactiveUI;

namespace DockFigmaSample.ViewModels.Tools;

public class InspectorInspectViewModel : ReactiveObject, IRoutableViewModel
{
    public InspectorInspectViewModel(IScreen host)
    {
        HostScreen = host;
    }

    public string UrlPathSegment { get; } = "inspector-inspect";
    public IScreen HostScreen { get; }

    public ObservableCollection<InspectSpecItem> Specs { get; } = SampleData.CreateInspectSpecs();
}
