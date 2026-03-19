using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Documents;

public interface IOfficeDocumentViewModel
{
    OfficeAppKind AppKind { get; }
    RoutingState Router { get; }
    string Title { get; }
}
