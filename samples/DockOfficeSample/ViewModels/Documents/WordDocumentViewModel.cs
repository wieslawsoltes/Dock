using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Documents;

public class WordDocumentViewModel : RoutableDocument
{
    private readonly OfficeDocumentPageViewModel _readMode;
    private readonly OfficeDocumentPageViewModel _printLayoutMode;
    private readonly OfficeDocumentPageViewModel _webLayoutMode;

    public WordDocumentViewModel(IScreen host, string fileName) : base(host, "word-document")
    {
        FileName = fileName;
        AppKind = OfficeAppKind.Word;

        _readMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "read",
            "Read Mode",
            "A focused reading experience with simplified layout and clean typography.",
            new[] { "Distraction-free columns", "Quick search", "Read aloud" });

        _printLayoutMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "print",
            "Print Layout",
            "WYSIWYG layout with margins, headers, and page boundaries.",
            new[] { "Page breaks", "Rulers", "Header and footer" });

        _webLayoutMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "web",
            "Web Layout",
            "Continuous flow for screen-first documents and sharing.",
            new[] { "Responsive width", "Live links", "Continuous scroll" });

        Router.Navigate.Execute(_printLayoutMode).Subscribe(_ => { });
    }

    public OfficeAppKind AppKind { get; }
    public string FileName { get; }

    public void SetMode(WordViewMode mode)
    {
        var target = mode switch
        {
            WordViewMode.Read => _readMode,
            WordViewMode.PrintLayout => _printLayoutMode,
            WordViewMode.WebLayout => _webLayoutMode,
            _ => _printLayoutMode
        };

        Router.Navigate.Execute(target).Subscribe(_ => { });
    }
}
