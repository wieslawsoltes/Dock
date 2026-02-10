using ReactiveUI;

namespace DockXamlReactiveUISample.Models;

public class DocumentItem : ReactiveObject
{
    private string _title = string.Empty;
    private string _content = string.Empty;
    private string _editableContent = string.Empty;
    private string _status = string.Empty;
    private bool _canClose = true;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public string EditableContent
    {
        get => _editableContent;
        set => this.RaiseAndSetIfChanged(ref _editableContent, value);
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
}
