using System;
using System.Threading.Tasks;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Provides access to a UI-thread dispatcher without UI framework dependencies.
/// </summary>
public interface IDockDispatcher
{
    /// <summary>
    /// Executes the action on the dispatcher thread and returns a task that completes when done.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task that completes when the action finishes.</returns>
    Task InvokeAsync(Action action);
}
