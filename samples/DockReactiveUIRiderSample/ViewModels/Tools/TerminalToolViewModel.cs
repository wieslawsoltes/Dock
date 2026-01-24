using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels.Tools;

public class TerminalToolViewModel : Tool
{
    private string _output;

    public TerminalToolViewModel()
    {
        Id = "Terminal";
        Title = "Terminal";
        _output = "Terminal ready.\n";
    }

    public string Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }
}
