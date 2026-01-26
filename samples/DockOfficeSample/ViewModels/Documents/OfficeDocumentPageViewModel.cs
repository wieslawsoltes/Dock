using System.Collections.Generic;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Documents;

public class OfficeDocumentPageViewModel : ReactiveObject, IRoutableViewModel
{
    public OfficeDocumentPageViewModel(
        IScreen host,
        OfficeAppKind appKind,
        string urlSegment,
        string modeTitle,
        string description,
        IReadOnlyList<string> highlights)
    {
        HostScreen = host;
        AppKind = appKind;
        UrlPathSegment = urlSegment;
        ModeTitle = modeTitle;
        Description = description;
        Highlights = highlights;
    }

    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public OfficeAppKind AppKind { get; }
    public string ModeTitle { get; }
    public string Description { get; }
    public IReadOnlyList<string> Highlights { get; }
}
