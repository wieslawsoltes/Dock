using System;
using Dock.Model.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Dialogs;

public abstract class DialogViewModelBase : ReactiveObject, IDockDialogContent
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
