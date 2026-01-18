namespace DockReactiveUICanonicalSample.Services;

public sealed class BusyServiceFactory : IBusyServiceFactory
{
    private readonly IDockGlobalBusyService _globalBusyService;

    public BusyServiceFactory(IDockGlobalBusyService globalBusyService)
    {
        _globalBusyService = globalBusyService;
    }

    public IDockBusyService Create() => new DockBusyService(_globalBusyService);
}
