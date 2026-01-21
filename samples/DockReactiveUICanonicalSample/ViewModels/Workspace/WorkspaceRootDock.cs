using System;
using Dock.Model.Services;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public sealed class WorkspaceRootDock : RoutableRootDock, IHostOverlayServices
{
    private readonly IHostOverlayServices _hostServices;

    public WorkspaceRootDock(IScreen host, IHostOverlayServices hostServices, string? url = null)
        : base(host, url)
    {
        _hostServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
    }

    public IDockBusyService Busy => _hostServices.Busy;

    public IDockDialogService Dialogs => _hostServices.Dialogs;

    public IDockConfirmationService Confirmations => _hostServices.Confirmations;

    public IDockGlobalBusyService GlobalBusyService => _hostServices.GlobalBusyService;

    public IDockGlobalDialogService GlobalDialogService => _hostServices.GlobalDialogService;

    public IDockGlobalConfirmationService GlobalConfirmationService => _hostServices.GlobalConfirmationService;
}
