using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer.Newtonsoft;
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

        SaveWorkspaceA = ReactiveCommand.Create(() => _workspaceA = SaveWorkspace("A"));
        LoadWorkspaceA = ReactiveCommand.Create(() => LoadWorkspace(_workspaceA));
        SaveWorkspaceB = ReactiveCommand.Create(() => _workspaceB = SaveWorkspace("B"));
        LoadWorkspaceB = ReactiveCommand.Create(() => LoadWorkspace(_workspaceB));
        ResetLayout = ReactiveCommand.Create(ResetLayoutImpl);

        Layout = _factory.CreateLayout();
    }

    private DockWorkspace? SaveWorkspace(string id)
    {
        if (Layout is not IDock layout)
        {
            return null;
        }

        return _workspaceManager.Capture(id, layout, includeState: true, name: $"Workspace {id}");
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
    }

    private void ResetLayoutImpl()
    {
        Layout = _factory.CreateLayout();
    }

    public void CloseLayout()
    {
        if (Layout is IDock dock && dock.Close.CanExecute(null))
        {
            dock.Close.Execute(null);
        }
    }
}
