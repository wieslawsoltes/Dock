using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace DockReactiveUIRiderSample.ViewModels.Tools;

public class StructureToolViewModel : Tool
{
    private string _summary;

    public StructureToolViewModel()
    {
        Id = "Structure";
        Title = "Structure";
        _summary = "Select a document to inspect its structure.";
    }

    public string Summary
    {
        get => _summary;
        set => this.RaiseAndSetIfChanged(ref _summary, value);
    }
}
