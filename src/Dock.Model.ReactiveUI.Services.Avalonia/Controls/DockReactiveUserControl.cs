using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Services.Lifecycle;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;

namespace Dock.Model.ReactiveUI.Services.Avalonia.Controls;

/// <summary>
/// ReactiveUI view base that bridges view activation to view-model activation.
/// </summary>
/// <typeparam name="TViewModel">The view model type.</typeparam>
public abstract class DockReactiveUserControl<TViewModel> : ReactiveUserControl<TViewModel>
    where TViewModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DockReactiveUserControl{TViewModel}"/> class.
    /// </summary>
    protected DockReactiveUserControl()
    {
        this.WhenActivated(disposables =>
        {
            var activationBridge = Locator.Current.GetService<IActivationBridge>();
            var activationHandle = new SerialDisposable();
            disposables.Add(activationHandle);

            var subscription = this.WhenAnyValue(x => x.ViewModel)
                .Subscribe(Observer.Create<TViewModel?>(viewModel =>
                {
                    if (viewModel is IActivatableViewModel activatable)
                    {
                        activationHandle.Disposable = activationBridge?.Track(activatable, this)
                            ?? activatable.Activator.Activate();
                    }
                    else
                    {
                        activationHandle.Disposable = Disposable.Empty;
                    }
                }));

            disposables.Add(subscription);
        });
    }
}
