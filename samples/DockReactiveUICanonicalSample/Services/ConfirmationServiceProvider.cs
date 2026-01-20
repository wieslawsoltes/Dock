using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationServiceProvider : IConfirmationServiceProvider
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public ConfirmationServiceProvider(IHostOverlayServicesProvider overlayServicesProvider)
    {
        _overlayServicesProvider = overlayServicesProvider;
    }

    public IDockConfirmationService GetConfirmationService(IScreen hostScreen)
    {
        return _overlayServicesProvider.GetServices(hostScreen).Confirmations;
    }
}
