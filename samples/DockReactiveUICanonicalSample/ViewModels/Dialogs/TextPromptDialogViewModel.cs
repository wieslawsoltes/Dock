using System.Reactive;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Dialogs;

public sealed class TextPromptDialogViewModel : DialogViewModelBase
{
    private string? _input;

    public TextPromptDialogViewModel(
        string message,
        string placeholder,
        string primaryText = "Save",
        string secondaryText = "Cancel",
        string? initialValue = null)
    {
        Message = message;
        Placeholder = placeholder;
        PrimaryText = primaryText;
        SecondaryText = secondaryText;
        _input = initialValue;

        Confirm = ReactiveCommand.Create(() => Close(Input ?? string.Empty));
        Cancel = ReactiveCommand.Create(() => Close(null));
    }

    public string Message { get; }

    public string Placeholder { get; }

    public string PrimaryText { get; }

    public string SecondaryText { get; }

    public string? Input
    {
        get => _input;
        set => this.RaiseAndSetIfChanged(ref _input, value);
    }

    public ReactiveCommand<Unit, Unit> Confirm { get; }

    public ReactiveCommand<Unit, Unit> Cancel { get; }
}
