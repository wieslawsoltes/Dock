using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Tools;

public class InspectorSectionViewModel : ReactiveObject, IRoutableViewModel
{
    private bool _isActive;

    public InspectorSectionViewModel(
        IScreen host,
        OfficeAppKind appKind,
        string urlSegment,
        string sectionTitle,
        string description,
        IReadOnlyList<string> items)
    {
        HostScreen = host;
        AppKind = appKind;
        UrlPathSegment = urlSegment;
        SectionTitle = sectionTitle;
        Description = description;
        Items = new ObservableCollection<string>(items);
    }

    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public OfficeAppKind AppKind { get; }
    public string SectionTitle { get; }
    public string Description { get; }
    public ObservableCollection<string> Items { get; }

    public ReactiveCommand<InspectorSectionViewModel, Unit>? SwitchSection { get; set; }

    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }
}
