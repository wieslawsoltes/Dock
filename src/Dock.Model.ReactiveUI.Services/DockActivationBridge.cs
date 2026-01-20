using System;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Default activation bridge for ReactiveUI view models.
/// </summary>
public sealed class DockActivationBridge : IActivationBridge
{
    /// <inheritdoc />
    public IDisposable Track(IActivatableViewModel viewModel, IViewFor view)
    {
        if (viewModel is null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        if (view is null)
        {
            throw new ArgumentNullException(nameof(view));
        }

        return viewModel.Activator.Activate();
    }
}
