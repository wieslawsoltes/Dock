using System.Collections.Generic;
using System.Reactive;
using Dock.Model.Controls;
using DockOfficeSample.ViewModels.Documents;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Workspaces;

public class ExcelWorkspaceViewModel : ReactiveObject, IRoutableViewModel
{
    private IRootDock? _layout;
    private ExcelViewMode _editorMode;
    private readonly IReadOnlyList<ExcelDocumentViewModel> _documents;
    private readonly OfficeInspectorToolViewModel _inspectorTool;

    public ExcelWorkspaceViewModel(IScreen host)
    {
        HostScreen = host;

        var factory = new ExcelWorkspaceDockFactory(host);
        Layout = factory.CreateLayout();
        if (Layout is not null)
        {
            factory.InitLayout(Layout);
        }

        _documents = factory.Documents;
        _inspectorTool = factory.InspectorTool;

        SwitchMode = ReactiveCommand.Create<ExcelViewMode>(SetMode);
        SetMode(ExcelViewMode.Normal);
    }

    public string UrlPathSegment { get; } = "excel";
    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public ExcelViewMode EditorMode
    {
        get => _editorMode;
        private set => this.RaiseAndSetIfChanged(ref _editorMode, value);
    }

    public string FileName { get; } = "Q3Forecast.xlsx";
    public string StatusMessage { get; } = "AutoSave on";

    public ReactiveCommand<ExcelViewMode, Unit> SwitchMode { get; }

    private void SetMode(ExcelViewMode mode)
    {
        EditorMode = mode;

        foreach (var document in _documents)
        {
            document.SetMode(mode);
        }

        var section = mode switch
        {
            ExcelViewMode.Normal => "formulas",
            ExcelViewMode.PageLayout => "charts",
            ExcelViewMode.PageBreakPreview => "data",
            _ => "formulas"
        };

        _inspectorTool.SetSection(section);
    }
}
