using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogServiceProvider : IDialogServiceProvider
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public DialogServiceProvider(IHostOverlayServicesProvider overlayServicesProvider)
    {
        _overlayServicesProvider = overlayServicesProvider;
    }

    public IDockDialogService GetDialogService(IScreen hostScreen)
    {
        return _overlayServicesProvider.GetServices(hostScreen).Dialogs;
    }
}
