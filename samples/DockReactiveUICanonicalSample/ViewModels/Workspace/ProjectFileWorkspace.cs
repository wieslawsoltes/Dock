using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class ProjectFileWorkspace
{
    private readonly Factory _factory;
    private readonly IToolDock _leftDock;
    private readonly IDockable _fileActionsTool;
    private readonly IToolDock _rightDock;
    private readonly IReadOnlyDictionary<string, ToolPanelViewModel> _rightTools;

    public ProjectFileWorkspace(
        Factory factory,
        IToolDock leftDock,
        IDockable fileActionsTool,
        IToolDock rightDock,
        IReadOnlyDictionary<string, ToolPanelViewModel> rightTools)
    {
        _factory = factory;
        _leftDock = leftDock;
        _fileActionsTool = fileActionsTool;
        _rightDock = rightDock;
        _rightTools = rightTools;
    }

    public IRootDock Layout { get; internal set; } = null!;

    public bool IsLeftPanelOpen => _leftDock.VisibleDockables?.Contains(_fileActionsTool) == true;

    public bool IsRightToolOpen(string toolId)
    {
        if (!_rightTools.TryGetValue(toolId, out var tool))
        {
            return false;
        }

        return _rightDock.VisibleDockables?.Contains(tool) == true;
    }

    public void ToggleLeftPanel()
    {
        _leftDock.VisibleDockables ??= _factory.CreateList<IDockable>();

        if (_leftDock.VisibleDockables.Contains(_fileActionsTool))
        {
            _factory.RemoveDockable(_fileActionsTool, false);
        }
        else
        {
            _leftDock.AddTool(_fileActionsTool);
        }
    }

    public void ToggleRightTool(string toolId)
    {
        if (!_rightTools.TryGetValue(toolId, out var tool))
        {
            return;
        }

        _rightDock.VisibleDockables ??= _factory.CreateList<IDockable>();

        if (_rightDock.VisibleDockables.Contains(tool))
        {
            _factory.RemoveDockable(tool, false);
        }
        else
        {
            _rightDock.AddTool(tool);
            _rightDock.IsExpanded = true;
        }

        if (_rightDock.VisibleDockables.Count == 0)
        {
            _rightDock.IsExpanded = false;
        }
    }

    public IReadOnlyList<ToolPanelViewModel> GetRightTools() => _rightTools.Values.ToList();
}
