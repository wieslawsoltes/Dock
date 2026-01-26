using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dock.Model.Services;

/// <summary>
/// Defines a per-host busy state service used by overlay controls.
/// </summary>
public interface IDockBusyService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether the host is busy.
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Gets the current busy message.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Gets or sets whether the reload button is visible.
    /// </summary>
    bool IsReloadVisible { get; set; }

    /// <summary>
    /// Gets a value indicating whether a reload handler is available.
    /// </summary>
    bool CanReload { get; }

    /// <summary>
    /// Gets the command used by the overlay to trigger reload.
    /// </summary>
    ICommand ReloadCommand { get; }

    /// <summary>
    /// Begins a busy scope with the provided message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>A disposable handle that ends the busy scope.</returns>
    IDisposable Begin(string message);

    /// <summary>
    /// Runs an async action within a busy scope.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="action">The work to run.</param>
    Task RunAsync(string message, Func<Task> action);

    /// <summary>
    /// Updates the current busy message.
    /// </summary>
    /// <param name="message">The new message.</param>
    void UpdateMessage(string? message);

    /// <summary>
    /// Sets the reload handler invoked by <see cref="ReloadCommand"/>.
    /// </summary>
    /// <param name="handler">The async reload handler.</param>
    void SetReloadHandler(Func<Task>? handler);
}
