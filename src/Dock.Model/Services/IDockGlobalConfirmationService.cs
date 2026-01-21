using System;
using System.ComponentModel;

namespace Dock.Model.Services;

/// <summary>
/// Tracks global confirmation state for cross-window overlays.
/// </summary>
public interface IDockGlobalConfirmationService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether any confirmation is open.
    /// </summary>
    bool IsConfirmationOpen { get; }

    /// <summary>
    /// Gets the global confirmation message.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Begins a global confirmation scope.
    /// </summary>
    /// <param name="message">The optional message to display.</param>
    /// <returns>A disposable handle that ends the global confirmation scope.</returns>
    IDisposable Begin(string? message = null);
}
