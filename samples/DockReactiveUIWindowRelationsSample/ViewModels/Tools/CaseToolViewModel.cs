using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIWindowRelationsSample.ViewModels.Tools;

public class CaseToolViewModel : Tool
{
    private string? _description;

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }
}
