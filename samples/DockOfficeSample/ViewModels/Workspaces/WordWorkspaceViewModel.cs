using System.Collections.Generic;
using System.Reactive;
using Dock.Model.Controls;
using DockOfficeSample.ViewModels.Documents;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Workspaces;

public class WordWorkspaceViewModel : ReactiveObject, IRoutableViewModel
{
    private IRootDock? _layout;
    private WordViewMode _editorMode;
    private readonly IReadOnlyList<WordDocumentViewModel> _documents;
    private readonly OfficeInspectorToolViewModel _inspectorTool;

    public WordWorkspaceViewModel(IScreen host)
    {
        HostScreen = host;

        var factory = new WordWorkspaceDockFactory(host);
        Layout = factory.CreateLayout();
        if (Layout is not null)
        {
            factory.InitLayout(Layout);
        }

        _documents = factory.Documents;
        _inspectorTool = factory.InspectorTool;

        SwitchMode = ReactiveCommand.Create<WordViewMode>(SetMode);
        SetMode(WordViewMode.PrintLayout);
    }

    public string UrlPathSegment { get; } = "word";
    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public WordViewMode EditorMode
    {
        get => _editorMode;
        private set => this.RaiseAndSetIfChanged(ref _editorMode, value);
    }

    public string FileName { get; } = "LaunchPlan.docx";
    public string StatusMessage { get; } = "All changes saved to OneDrive";

    public ReactiveCommand<WordViewMode, Unit> SwitchMode { get; }

    private void SetMode(WordViewMode mode)
    {
        EditorMode = mode;

        foreach (var document in _documents)
        {
            document.SetMode(mode);
        }

        var section = mode switch
        {
            WordViewMode.Read => "review",
            WordViewMode.PrintLayout => "styles",
            WordViewMode.WebLayout => "references",
            _ => "styles"
        };

        _inspectorTool.SetSection(section);
    }
}
