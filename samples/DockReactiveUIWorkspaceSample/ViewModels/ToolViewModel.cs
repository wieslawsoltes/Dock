using Dock.Model.ReactiveUI.Controls;
using ReactiveUI.Reactive;

namespace DockReactiveUIWorkspaceSample.ViewModels;

public class ToolViewModel : Tool
{
    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}
