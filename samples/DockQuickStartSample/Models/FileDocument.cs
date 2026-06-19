using ReactiveUI;

namespace DockQuickStartSample.Models;

public sealed class FileDocument : ReactiveObject
{
    private string _title = string.Empty;
    private string _content = string.Empty;

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

    public bool CanClose { get; set; } = true;
}
