using DockPrismSample.ViewModels.Documents;
using Dock.Model.Prism.Controls;
using Prism.Commands;
using Prism.Events;

namespace DockPrismSample.ViewModels.Docks;

public class CustomDocumentDock : DocumentDock
{
    private readonly IEventAggregator _eventAggregator;

    public CustomDocumentDock(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        CreateDocument = new DelegateCommand(CreateNewDocument);
    }

    private void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var index = (VisibleDockables?.Count ?? 0) + 1;
        var document = new DocumentViewModel(_eventAggregator)
        {
            Id = $"Document{index}",
            Title = $"Document{index}"
        };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
