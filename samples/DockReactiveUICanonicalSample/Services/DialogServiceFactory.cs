namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogServiceFactory : IDialogServiceFactory
{
    private readonly IDockGlobalDialogService _globalDialogService;

    public DialogServiceFactory(IDockGlobalDialogService globalDialogService)
    {
        _globalDialogService = globalDialogService;
    }

    public IDockDialogService Create() => new DockDialogService(_globalDialogService);
}
