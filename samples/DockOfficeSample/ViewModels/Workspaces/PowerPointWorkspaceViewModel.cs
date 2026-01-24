using System.Collections.Generic;
using System.Reactive;
using Dock.Model.Controls;
using DockOfficeSample.ViewModels.Documents;
using DockOfficeSample.ViewModels.Tools;
using ReactiveUI;

namespace DockOfficeSample.ViewModels.Workspaces;

public class PowerPointWorkspaceViewModel : ReactiveObject, IRoutableViewModel
{
    private IRootDock? _layout;
    private PowerPointViewMode _editorMode;
    private readonly IReadOnlyList<PowerPointDocumentViewModel> _documents;
    private readonly OfficeInspectorToolViewModel _inspectorTool;

    public PowerPointWorkspaceViewModel(IScreen host)
    {
        HostScreen = host;

        var factory = new PowerPointWorkspaceDockFactory(host);
        Layout = factory.CreateLayout();
        if (Layout is not null)
        {
            factory.InitLayout(Layout);
        }

        _documents = factory.Documents;
        _inspectorTool = factory.InspectorTool;

        SwitchMode = ReactiveCommand.Create<PowerPointViewMode>(SetMode);
        SetMode(PowerPointViewMode.Normal);
    }

    public string UrlPathSegment { get; } = "powerpoint";
    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public PowerPointViewMode EditorMode
    {
        get => _editorMode;
        private set => this.RaiseAndSetIfChanged(ref _editorMode, value);
    }

    public string FileName { get; } = "BoardDeck.pptx";
    public string StatusMessage { get; } = "Presenter view ready";

    public ReactiveCommand<PowerPointViewMode, Unit> SwitchMode { get; }

    private void SetMode(PowerPointViewMode mode)
    {
        EditorMode = mode;

        foreach (var document in _documents)
        {
            document.SetMode(mode);
        }

        var section = mode switch
        {
            PowerPointViewMode.Normal => "design",
            PowerPointViewMode.SlideSorter => "transitions",
            PowerPointViewMode.Presenter => "animations",
            _ => "design"
        };

        _inspectorTool.SetSection(section);
    }
}
