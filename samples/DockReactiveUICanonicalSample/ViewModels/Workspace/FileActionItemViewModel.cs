using System.Reactive;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class FileActionItemViewModel
{
    public FileActionItemViewModel(string title, string description, ReactiveCommand<Unit, Unit> command)
    {
        Title = title;
        Description = description;
        Command = command;
    }

    public string Title { get; }

    public string Description { get; }

    public ReactiveCommand<Unit, Unit> Command { get; }
}
