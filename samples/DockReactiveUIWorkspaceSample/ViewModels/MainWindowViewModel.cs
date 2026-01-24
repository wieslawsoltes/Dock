using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using ReactiveUI;

namespace DockReactiveUIWorkspaceSample.ViewModels;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
public class MainWindowViewModel : ReactiveObject
{
    private readonly DockWorkspaceManager _workspaceManager;
    private readonly DockFactory _factory;
    private IRootDock? _layout;
    private bool _isDockingEnabled = true;
    private bool _isWorkspaceDirty;
    private string _workspaceStatus = "Workspace: Unsaved";
    private DockWorkspace? _defaultWorkspace;
    private DockWorkspace? _workspaceA;
    private DockWorkspace? _workspaceB;

    public IRootDock? Layout
    {
        get => _layout;
        set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public IFactory Factory { get; }

    public bool IsDockingEnabled
    {
        get => _isDockingEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDockingEnabled, value);
    }

    public bool IsWorkspaceDirty
    {
        get => _isWorkspaceDirty;
        set => this.RaiseAndSetIfChanged(ref _isWorkspaceDirty, value);
    }

    public string WorkspaceStatus
    {
        get => _workspaceStatus;
        set => this.RaiseAndSetIfChanged(ref _workspaceStatus, value);
    }

    public ReactiveCommand<Unit, Unit> SaveWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> LoadWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> SaveWorkspaceB { get; }
    public ReactiveCommand<Unit, Unit> LoadWorkspaceB { get; }
    public ReactiveCommand<Unit, Unit> ResetLayout { get; }

    public MainWindowViewModel()
    {
        _factory = new DockFactory();
        Factory = _factory;
        _workspaceManager = new DockWorkspaceManager(new DockSerializer());
        _workspaceManager.TrackFactory(_factory);
        _workspaceManager.WorkspaceDirtyChanged += (_, _) => UpdateWorkspaceState();

        SaveWorkspaceA = ReactiveCommand.Create(() =>
        {
            _workspaceA = SaveWorkspace("A");
        });
        LoadWorkspaceA = ReactiveCommand.Create(() => LoadWorkspace(_workspaceA));
        SaveWorkspaceB = ReactiveCommand.Create(() =>
        {
            _workspaceB = SaveWorkspace("B");
        });
        LoadWorkspaceB = ReactiveCommand.Create(() => LoadWorkspace(_workspaceB));
        ResetLayout = ReactiveCommand.Create(ResetLayoutImpl);

        Layout = _factory.CreateLayout();
        _defaultWorkspace = SaveWorkspace("Default", "Default");
        UpdateWorkspaceState();
    }

    private DockWorkspace? SaveWorkspace(string id, string? name = null)
    {
        if (Layout is not IDock layout)
        {
            return null;
        }

        var workspaceName = string.IsNullOrWhiteSpace(name) ? $"Workspace {id}" : name;
        var workspace = _workspaceManager.Capture(id, layout, includeState: true, name: workspaceName);
        UpdateWorkspaceState();
        return workspace;
    }

    private void LoadWorkspace(DockWorkspace? workspace)
    {
        if (workspace is null)
        {
            return;
        }

        var restored = _workspaceManager.Restore(workspace);
        if (restored is IRootDock root)
        {
            Layout = root;
        }

        UpdateWorkspaceState();
    }

    private void ResetLayoutImpl()
    {
        if (_defaultWorkspace is not null)
        {
            LoadWorkspace(_defaultWorkspace);
            return;
        }

        Layout = _factory.CreateLayout();
        _workspaceManager.MarkClean();
        UpdateWorkspaceState();
    }

    private void UpdateWorkspaceState()
    {
        var workspace = _workspaceManager.ActiveWorkspace;
        var label = workspace?.Name ?? workspace?.Id ?? "Unsaved";
        var status = _workspaceManager.IsDirty ? $"Workspace: {label} *" : $"Workspace: {label}";

        IsWorkspaceDirty = _workspaceManager.IsDirty;
        WorkspaceStatus = status;
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
}
