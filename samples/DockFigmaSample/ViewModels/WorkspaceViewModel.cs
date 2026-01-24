using System.Reactive;
using Dock.Model.Controls;
using DockFigmaSample.ViewModels.Documents;
using DockFigmaSample.ViewModels.Tools;
using ReactiveUI;

namespace DockFigmaSample.ViewModels;

public class WorkspaceViewModel : ReactiveObject, IRoutableViewModel
{
    private IRootDock? _layout;
    private WorkspaceMode _editorMode;
    private readonly CanvasDocumentViewModel _canvasDocument;
    private readonly InspectorToolViewModel _inspectorTool;

    public WorkspaceViewModel(IScreen host)
    {
        HostScreen = host;

        var factory = new WorkspaceDockFactory(host);
        Layout = factory.CreateLayout();
        if (Layout is not null)
        {
            factory.InitLayout(Layout);
        }

        _canvasDocument = factory.CanvasDocument;
        _inspectorTool = factory.InspectorTool;

        SwitchMode = ReactiveCommand.Create<WorkspaceMode>(SetMode);
        SetMode(WorkspaceMode.Design);
    }

    public string UrlPathSegment { get; } = "workspace";
    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public WorkspaceMode EditorMode
    {
        get => _editorMode;
        private set => this.RaiseAndSetIfChanged(ref _editorMode, value);
    }

    public string FileName { get; } = "LandingPage.fig";
    public string ProjectName { get; } = "Nimbus";
    public string StatusMessage { get; } = "All changes saved";

    public ReactiveCommand<WorkspaceMode, Unit> SwitchMode { get; }

    private void SetMode(WorkspaceMode mode)
    {
        EditorMode = mode;
        _canvasDocument.SetMode(mode);
        _inspectorTool.SetMode(mode);
    }
}
