using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockOfficeSample.Models;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Documents;

public class ExcelDocumentViewModel : RoutableDocument
{
    private readonly OfficeDocumentPageViewModel _normalMode;
    private readonly OfficeDocumentPageViewModel _pageLayoutMode;
    private readonly OfficeDocumentPageViewModel _pageBreakMode;

    public ExcelDocumentViewModel(IScreen host, string fileName) : base(host, "excel-document")
    {
        FileName = fileName;
        AppKind = OfficeAppKind.Excel;

        _normalMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "normal",
            "Normal View",
            "The standard grid optimized for data entry, formulas, and quick analysis.",
            new[] { "Freeze panes", "Smart fill", "Quick calculations" });

        _pageLayoutMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "layout",
            "Page Layout",
            "Preview headers, footers, and print margins while editing.",
            new[] { "Print headers", "Margin guides", "Scale controls" });

        _pageBreakMode = new OfficeDocumentPageViewModel(
            this,
            AppKind,
            "breaks",
            "Page Break Preview",
            "Manage print sections with clear page boundaries and scaling hints.",
            new[] { "Drag page breaks", "Print area", "Scaling" });

        Router.Navigate.Execute(_normalMode).Subscribe(_ => { });
    }

    public OfficeAppKind AppKind { get; }
    public string FileName { get; }

    public void SetMode(ExcelViewMode mode)
    {
        var target = mode switch
        {
            ExcelViewMode.Normal => _normalMode,
            ExcelViewMode.PageLayout => _pageLayoutMode,
            ExcelViewMode.PageBreakPreview => _pageBreakMode,
            _ => _normalMode
        };

        Router.Navigate.Execute(target).Subscribe(_ => { });
    }
}
