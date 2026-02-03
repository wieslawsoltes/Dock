using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIWindowRelationsSample.ViewModels.Documents;

public class CaseDocumentViewModel : Document
{
    private string? _content;

    public string? Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }
}
