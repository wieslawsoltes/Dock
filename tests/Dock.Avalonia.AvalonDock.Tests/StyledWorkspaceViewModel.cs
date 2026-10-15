using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Dock.Avalonia.AvalonDock.Tests;

public partial class StyledContentViewModel : ReactiveObject
{
    [Reactive] private string _title = string.Empty;
    [Reactive] private bool _canClose = true;
    [Reactive] private bool _canFloat = true;
    [Reactive] private bool _canHide = true;

    public StyledContentViewModel(string title)
    {
        Identifier = title;
        Title = title;
        CloseCommand = ReactiveCommand.Create(() => { CloseRequests++; }, outputScheduler: ImmediateScheduler.Instance);
    }

    public string Identifier { get; }
    public int CloseRequests { get; private set; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
}

public sealed class StyledWorkspaceViewModel : ReactiveObject
{
    public ObservableCollection<StyledContentViewModel> Documents { get; } = new() { new("Styled document") };
    public ObservableCollection<StyledContentViewModel> Tools { get; } = new() { new("Styled tool") };
}
