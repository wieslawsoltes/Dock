using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class BusyServiceProvider : IBusyServiceProvider
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public BusyServiceProvider(IHostOverlayServicesProvider overlayServicesProvider)
    {
        _overlayServicesProvider = overlayServicesProvider;
    }

    public IDockBusyService GetBusyService(IScreen hostScreen)
    {
        return _overlayServicesProvider.GetServices(hostScreen).Busy;
    }
}
