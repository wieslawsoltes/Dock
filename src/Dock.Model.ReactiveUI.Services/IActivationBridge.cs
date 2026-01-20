using System;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Bridges view activation to view-model activation.
/// </summary>
public interface IActivationBridge
{
    /// <summary>
    /// Creates an activation scope for the provided view model and view.
    /// </summary>
    /// <param name="viewModel">The activatable view model.</param>
    /// <param name="view">The view hosting the view model.</param>
    /// <returns>A disposable that deactivates the view model.</returns>
    IDisposable Track(IActivatableViewModel viewModel, IViewFor view);
}
