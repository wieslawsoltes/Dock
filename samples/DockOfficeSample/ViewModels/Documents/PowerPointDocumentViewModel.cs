using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Documents;

public class PowerPointDocumentViewModel : RoutableDocument
{
    private readonly OfficeDocumentPageViewModel _normalMode;
    private readonly OfficeDocumentPageViewModel _slideSorterMode;
    private readonly OfficeDocumentPageViewModel _presenterMode;

    public PowerPointDocumentViewModel(IScreen host, string fileName) : base(host, "powerpoint-document")
    {
        FileName = fileName;
        AppKind = OfficeAppKind.PowerPoint;

        _normalMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "normal",
            "Normal View",
            "Compose slides with editing tools, notes, and quick formatting.",
            new[] { "Slide canvas", "Speaker notes", "Design suggestions" });

        _slideSorterMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "sorter",
            "Slide Sorter",
            "Rearrange and group slides while previewing transitions.",
            new[] { "Reorder slides", "Section grouping", "Bulk actions" });

        _presenterMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "presenter",
            "Presenter View",
            "Run the presentation with notes, timers, and slide previews.",
            new[] { "Timer controls", "Upcoming slide", "Presenter tools" });

        Router.Navigate.Execute(_normalMode).Subscribe(_ => { });
    }

    public OfficeAppKind AppKind { get; }
    public string FileName { get; }

    public void SetMode(PowerPointViewMode mode)
    {
        var target = mode switch
        {
            PowerPointViewMode.Normal => _normalMode,
            PowerPointViewMode.SlideSorter => _slideSorterMode,
            PowerPointViewMode.Presenter => _presenterMode,
            _ => _normalMode
        };

        Router.Navigate.Execute(target).Subscribe(_ => { });
    }
}
