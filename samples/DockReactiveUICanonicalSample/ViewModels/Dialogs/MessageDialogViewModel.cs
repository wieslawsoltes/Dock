using System.Reactive;
using Unit = ReactiveUI.Primitives.RxVoid;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Dialogs;

public sealed class MessageDialogViewModel : DialogViewModelBase
{
    public MessageDialogViewModel(string message, string primaryText = "OK")
    {
        Message = message;
        PrimaryText = primaryText;
        CloseCommand = ReactiveCommand.Create(() => Close(true));
    }

    public string Message { get; }

    public string PrimaryText { get; }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
}
