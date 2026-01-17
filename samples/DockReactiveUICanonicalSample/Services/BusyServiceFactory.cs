namespace DockReactiveUICanonicalSample.Services;

public sealed class BusyServiceFactory : IBusyServiceFactory
{
    private readonly IGlobalBusyService _globalBusyService;

    public BusyServiceFactory(IGlobalBusyService globalBusyService)
    {
        _globalBusyService = globalBusyService;
    }

    public IBusyService Create() => new BusyService(_globalBusyService);
}
