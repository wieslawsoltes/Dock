using ReactiveUI;

namespace DockXamlReactiveUISample.Models;

public class ToolItem : ReactiveObject
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _status = string.Empty;
    private bool _canClose = true;
    private bool? _closeOverride;
    private bool? _floatOverride;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public bool CanClose
    {
        get => _canClose;
        set => this.RaiseAndSetIfChanged(ref _canClose, value);
    }

    public bool? CloseOverride
    {
        get => _closeOverride;
        set => this.RaiseAndSetIfChanged(ref _closeOverride, value);
    }

    public bool? FloatOverride
    {
        get => _floatOverride;
        set => this.RaiseAndSetIfChanged(ref _floatOverride, value);
    }
}
