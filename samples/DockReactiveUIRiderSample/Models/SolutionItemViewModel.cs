using System.Collections.ObjectModel;
using ReactiveUI;

namespace DockReactiveUIRiderSample.Models;

public class SolutionItemViewModel : ReactiveObject
{
    private bool _isExpanded;
    private bool _isSelected;

    public SolutionItemViewModel(SolutionItemKind kind, string name, string? path)
    {
        Kind = kind;
        Name = name;
        Path = path;
        Children = new ObservableCollection<SolutionItemViewModel>();
    }

    public SolutionItemKind Kind { get; }

    public string Name { get; }

    public string? Path { get; }

    public ObservableCollection<SolutionItemViewModel> Children { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool IsLeaf => Kind == SolutionItemKind.Document;
}
