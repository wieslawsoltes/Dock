using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolToggleViewModel : ReactiveObject
{
    private bool _isOpen;

    public ToolToggleViewModel(string toolId, string title, Action<string> toggleAction)
    {
        ToolId = toolId;
        Title = title;
        ToggleCommand = ReactiveCommand.Create(() => toggleAction(toolId));
    }

    public string ToolId { get; }

    public string Title { get; }

    public bool IsOpen
    {
        get => _isOpen;
        set => this.RaiseAndSetIfChanged(ref _isOpen, value);
    }

    public ReactiveCommand<Unit, Unit> ToggleCommand { get; }
}
