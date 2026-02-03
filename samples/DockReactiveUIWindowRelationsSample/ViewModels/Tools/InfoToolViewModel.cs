using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIWindowRelationsSample.ViewModels.Tools;

public class InfoToolViewModel : Tool
{
    private string? _message;

    public string? Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }
}
