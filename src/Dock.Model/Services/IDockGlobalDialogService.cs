using System;
using System.ComponentModel;

namespace Dock.Model.Services;

/// <summary>
/// Tracks global dialog state for cross-window overlays.
/// </summary>
public interface IDockGlobalDialogService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether any dialog is open.
    /// </summary>
    bool IsDialogOpen { get; }

    /// <summary>
    /// Gets the global dialog message.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Begins a global dialog scope.
    /// </summary>
    /// <param name="message">The optional message to display.</param>
    /// <returns>A disposable handle that ends the global dialog scope.</returns>
    IDisposable Begin(string? message = null);
}
