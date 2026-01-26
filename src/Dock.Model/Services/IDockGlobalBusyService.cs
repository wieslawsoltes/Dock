using System;
using System.ComponentModel;

namespace Dock.Model.Services;

/// <summary>
/// Tracks global busy state for cross-window overlays.
/// </summary>
public interface IDockGlobalBusyService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether any window is busy.
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Gets the global busy message.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Begins a global busy scope.
    /// </summary>
    /// <param name="message">The optional message to display.</param>
    /// <returns>A disposable handle that ends the global busy scope.</returns>
    IDisposable Begin(string? message = null);
}
