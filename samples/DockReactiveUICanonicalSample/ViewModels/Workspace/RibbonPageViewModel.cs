using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class RibbonPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly ProjectFileWorkspace _workspace;
    private bool _isLeftPanelOpen;

    public RibbonPageViewModel(
        IScreen hostScreen,
        ProjectFileWorkspace workspace,
        IEnumerable<ToolPanelViewModel> tools)
    {
        HostScreen = hostScreen;
        _workspace = workspace;
        ToolToggles = new ObservableCollection<ToolToggleViewModel>(
            BuildToggles(tools));

        SyncFromWorkspace();

        ToggleLeftPanel = ReactiveCommand.Create(() =>
        {
            _workspace.ToggleLeftPanel();
            IsLeftPanelOpen = _workspace.IsLeftPanelOpen;
        });

        // Toggle commands are handled by ToolToggleViewModel to keep bindings compiled.
    }

    public string UrlPathSegment { get; } = "ribbon";

    public IScreen HostScreen { get; }

    public ObservableCollection<ToolToggleViewModel> ToolToggles { get; }

    public bool IsLeftPanelOpen
    {
        get => _isLeftPanelOpen;
        set => this.RaiseAndSetIfChanged(ref _isLeftPanelOpen, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleLeftPanel { get; }

    private void SyncFromWorkspace()
    {
        IsLeftPanelOpen = _workspace.IsLeftPanelOpen;
        foreach (var tool in ToolToggles)
        {
            tool.IsOpen = _workspace.IsRightToolOpen(tool.ToolId);
        }
    }

    private IEnumerable<ToolToggleViewModel> BuildToggles(IEnumerable<ToolPanelViewModel> tools)
    {
        foreach (var tool in tools)
        {
            var toggle = new ToolToggleViewModel(
                tool.ToolId,
                tool.Title ?? "Tool",
                ToggleToolState);
            toggle.IsOpen = _workspace.IsRightToolOpen(tool.ToolId);
            yield return toggle;
        }
    }

    private void ToggleToolState(string toolId)
    {
        _workspace.ToggleRightTool(toolId);
        foreach (var tool in ToolToggles)
        {
            if (tool.ToolId == toolId)
            {
                tool.IsOpen = _workspace.IsRightToolOpen(toolId);
                break;
            }
        }
    }
}
