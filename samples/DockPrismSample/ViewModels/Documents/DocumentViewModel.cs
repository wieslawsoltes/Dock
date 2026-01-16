using System;
using Dock.Model.Prism.Controls;
using DockPrismSample.Events;
using Prism.Commands;
using Prism.Events;

namespace DockPrismSample.ViewModels.Documents;

public class DocumentViewModel : Document
{
    private readonly IEventAggregator _eventAggregator;
    private string? _lastSaved;

    public DelegateCommand Save { get; }

    public string? LastSaved
    {
        get => _lastSaved;
        private set => SetProperty(ref _lastSaved, value);
    }

    public DocumentViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        Save = new DelegateCommand(OnSave);
        DockPrismSample.ViewModels.DockCommands.SaveAll.RegisterCommand(Save);
    }

    private void OnSave()
    {
        LastSaved = DateTime.Now.ToString("T");
        _eventAggregator.GetEvent<DocumentSavedEvent>()
            .Publish(new DocumentSavedPayload(Id, Title, LastSaved ?? string.Empty));
    }

    public override bool OnClose()
    {
        DockPrismSample.ViewModels.DockCommands.SaveAll.UnregisterCommand(Save);
        _eventAggregator.GetEvent<DocumentClosedEvent>()
            .Publish(new DocumentClosedPayload(Id, Title));
        return base.OnClose();
    }
}
