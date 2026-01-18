namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationServiceFactory : IConfirmationServiceFactory
{
    private readonly IDockGlobalConfirmationService _globalConfirmationService;

    public ConfirmationServiceFactory(IDockGlobalConfirmationService globalConfirmationService)
    {
        _globalConfirmationService = globalConfirmationService;
    }

    public IDockConfirmationService Create() => new DockConfirmationService(_globalConfirmationService);
}
