namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogServiceFactory : IDialogServiceFactory
{
    private readonly IGlobalDialogService _globalDialogService;

    public DialogServiceFactory(IGlobalDialogService globalDialogService)
    {
        _globalDialogService = globalDialogService;
    }

    public IDialogService Create() => new DialogService(_globalDialogService);
}
