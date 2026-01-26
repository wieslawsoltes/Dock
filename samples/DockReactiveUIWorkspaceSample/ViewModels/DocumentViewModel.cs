using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIWorkspaceSample.ViewModels;

public class DocumentViewModel : Document
{
    private string _body = string.Empty;

    public string Body
    {
        get => _body;
        set => this.RaiseAndSetIfChanged(ref _body, value);
    }
}
