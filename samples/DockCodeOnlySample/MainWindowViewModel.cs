using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using ReactiveUI;

namespace DockCodeOnlySample;

[RequiresUnreferencedCode("Requires unreferenced code for RaiseAndSetIfChanged.")]
[RequiresDynamicCode("Requires dynamic code for RaiseAndSetIfChanged.")]
public sealed class MainWindowViewModel : ReactiveObject
{
    private readonly DockCodeOnlyFactory _factory;
    private readonly DockWorkspaceManager _workspaceManager;
    private IRootDock? _layout;
    private bool _isDockingEnabled = true;
    private string _workspaceStatus = "Workspace: Unsaved";
    private bool _hasWorkspaceA;
    private bool _hasWorkspaceB;
    private DockWorkspace? _workspaceA;
    private DockWorkspace? _workspaceB;

    public MainWindowViewModel()
    {
        _factory = new DockCodeOnlyFactory();
        Factory = _factory;
        _workspaceManager = new DockWorkspaceManager(new DockSerializer());
        _workspaceManager.TrackFactory(_factory);
        _workspaceManager.WorkspaceDirtyChanged += (_, _) => UpdateWorkspaceStatus();

        Layout = _factory.CreateLayout();
        _factory.InitLayout(Layout);

        var canLoadWorkspaceA = this.WhenAnyValue(x => x.HasWorkspaceA);
        var canLoadWorkspaceB = this.WhenAnyValue(x => x.HasWorkspaceB);

        SaveWorkspaceA = ReactiveCommand.Create(() =>
        {
            _workspaceA = CaptureWorkspace("A");
        });
        LoadWorkspaceA = ReactiveCommand.Create(() => RestoreWorkspace(_workspaceA), canLoadWorkspaceA);
        SaveWorkspaceB = ReactiveCommand.Create(() =>
        {
            _workspaceB = CaptureWorkspace("B");
        });
        LoadWorkspaceB = ReactiveCommand.Create(() => RestoreWorkspace(_workspaceB), canLoadWorkspaceB);

        UpdateWorkspaceSlotState();
        UpdateWorkspaceStatus();
    }

    public IFactory Factory { get; }

    public IRootDock? Layout
    {
        get => _layout;
        private set => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    public bool IsDockingEnabled
    {
        get => _isDockingEnabled;
        set => this.RaiseAndSetIfChanged(ref _isDockingEnabled, value);
    }

    public string WorkspaceStatus
    {
        get => _workspaceStatus;
        private set => this.RaiseAndSetIfChanged(ref _workspaceStatus, value);
    }

    public bool HasWorkspaceA
    {
        get => _hasWorkspaceA;
        private set => this.RaiseAndSetIfChanged(ref _hasWorkspaceA, value);
    }

    public bool HasWorkspaceB
    {
        get => _hasWorkspaceB;
        private set => this.RaiseAndSetIfChanged(ref _hasWorkspaceB, value);
    }

    public ReactiveCommand<Unit, Unit> SaveWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> LoadWorkspaceA { get; }
    public ReactiveCommand<Unit, Unit> SaveWorkspaceB { get; }
    public ReactiveCommand<Unit, Unit> LoadWorkspaceB { get; }

    public void CloseLayout()
    {
        if (Layout is IDock layout && layout.Close.CanExecute(null))
        {
            layout.Close.Execute(null);
        }
    }

    private DockWorkspace? CaptureWorkspace(string id)
    {
        if (Layout is not IDock layout)
        {
            return null;
        }

        DockWorkspace workspace = _workspaceManager.Capture(id, layout, includeState: true, name: $"Workspace {id}");
        UpdateWorkspaceSlotState();
        UpdateWorkspaceStatus();
        return workspace;
    }

    private void RestoreWorkspace(DockWorkspace? workspace)
    {
        if (workspace is null)
        {
            return;
        }

        IDock? restored = _workspaceManager.Restore(workspace);
        if (restored is not IRootDock root)
        {
            return;
        }

        _factory.InitLayout(root);
        Layout = root;
        UpdateWorkspaceStatus();
    }

    private void UpdateWorkspaceSlotState()
    {
        HasWorkspaceA = _workspaceA is not null;
        HasWorkspaceB = _workspaceB is not null;
    }

    private void UpdateWorkspaceStatus()
    {
        DockWorkspace? workspace = _workspaceManager.ActiveWorkspace;
        string label = workspace?.Name ?? workspace?.Id ?? "Unsaved";
        WorkspaceStatus = _workspaceManager.IsDirty ? $"Workspace: {label} *" : $"Workspace: {label}";
    }
}
