using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Tools;

public class OfficeInspectorToolViewModel : RoutableTool
{
    private InspectorSectionViewModel? _activeSection;

    public OfficeInspectorToolViewModel(IScreen host, OfficeAppKind appKind)
        : base(host, "inspector")
    {
        AppKind = appKind;
        Sections = new ObservableCollection<InspectorSectionViewModel>();
        SwitchSection = ReactiveCommand.Create<InspectorSectionViewModel>(ActivateSection);
    }

    public OfficeAppKind AppKind { get; }
    public ObservableCollection<InspectorSectionViewModel> Sections { get; }
    public ReactiveCommand<InspectorSectionViewModel, Unit> SwitchSection { get; }

    public InspectorSectionViewModel? ActiveSection
    {
        get => _activeSection;
        private set => this.RaiseAndSetIfChanged(ref _activeSection, value);
    }

    public void InitializeSections(IEnumerable<InspectorSectionViewModel> sections)
    {
        Sections.Clear();
        foreach (var section in sections)
        {
            section.SwitchSection = SwitchSection;
            Sections.Add(section);
        }

        var initial = Sections.FirstOrDefault();
        if (initial is not null)
        {
            ActivateSection(initial);
        }
    }

    public void SetSection(string urlSegment)
    {
        var target = Sections.FirstOrDefault(section => section.UrlPathSegment == urlSegment);
        if (target is not null)
        {
            ActivateSection(target);
        }
    }

    private void ActivateSection(InspectorSectionViewModel section)
    {
        foreach (var item in Sections)
        {
            item.IsActive = item == section;
        }

        ActiveSection = section;
        Router.Navigate.Execute(section).Subscribe(_ => { });
    }
}
