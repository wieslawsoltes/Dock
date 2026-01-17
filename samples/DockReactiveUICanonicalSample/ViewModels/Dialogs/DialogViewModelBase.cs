using System;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Dialogs;

public abstract class DialogViewModelBase : ReactiveObject, IDialogContent
{
    private Action<object?>? _closeAction;

    public void SetCloseAction(Action<object?> closeAction)
    {
        _closeAction = closeAction;
    }

    protected void Close(object? result)
    {
        _closeAction?.Invoke(result);
    }
}
