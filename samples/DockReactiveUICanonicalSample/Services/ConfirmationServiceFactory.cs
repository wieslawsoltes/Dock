namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationServiceFactory : IConfirmationServiceFactory
{
    private readonly IGlobalConfirmationService _globalConfirmationService;

    public ConfirmationServiceFactory(IGlobalConfirmationService globalConfirmationService)
    {
        _globalConfirmationService = globalConfirmationService;
    }

    public IConfirmationService Create() => new ConfirmationService(_globalConfirmationService);
}
