using System;

namespace Dock.Model.Services;

/// <summary>
/// Provides a hook for dialog content to close itself with a result.
/// </summary>
public interface IDockDialogContent
{
    /// <summary>
    /// Provides the close action used to dismiss the dialog.
    /// </summary>
    /// <param name="closeAction">The close callback.</param>
    void SetCloseAction(Action<object?> closeAction);
}
