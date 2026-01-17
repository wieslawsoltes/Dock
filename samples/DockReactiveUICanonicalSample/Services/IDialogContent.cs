using System;

namespace DockReactiveUICanonicalSample.Services;

public interface IDialogContent
{
    void SetCloseAction(Action<object?> closeAction);
}
