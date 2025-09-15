using Dock.Model.Prism.Controls;
using DockPrismSample.Events;
using Prism.Events;

namespace DockPrismSample.ViewModels.Tools;

public class Tool1ViewModel : Tool
{
    private readonly IEventAggregator _eventAggregator;
    private string? _status;

    public string? Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public Tool1ViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        _eventAggregator.GetEvent<DocumentSavedEvent>()
            .Subscribe(payload => Status = $"Saved {payload.Title} at {payload.Timestamp}");

        _eventAggregator.GetEvent<DocumentClosedEvent>()
            .Subscribe(payload => Status = $"Closed {payload.Title}");
    }
}
