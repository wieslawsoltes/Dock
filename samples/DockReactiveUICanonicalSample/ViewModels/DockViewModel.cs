using System;
using Dock.Model.Controls;
using Dock.Model.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class DockViewModel : ReactiveObject, IRoutableViewModel, IHostOverlayServices
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;

    public DockViewModel(IScreen hostScreen, DockFactory factory)
    {
        HostScreen = hostScreen;
        _factory = factory;

        var layout = _factory.CreateLayout();
        if (layout is not null)
        {
            _factory.InitLayout(layout);
        }

        Layout = layout;
    }

    public string UrlPathSegment { get; } = "dock";

    public IScreen HostScreen { get; }

    public IRootDock? Layout
    {
        get => _layout;
        set
        => this.RaiseAndSetIfChanged(ref _layout, value);
    }

    private IHostOverlayServices OverlayServices
        => Layout as IHostOverlayServices
           ?? throw new InvalidOperationException("Overlay services are not available.");

    IDockBusyService IHostOverlayServices.Busy => OverlayServices.Busy;

    IDockDialogService IHostOverlayServices.Dialogs => OverlayServices.Dialogs;

    IDockConfirmationService IHostOverlayServices.Confirmations => OverlayServices.Confirmations;

    IDockGlobalBusyService IHostOverlayServices.GlobalBusyService => OverlayServices.GlobalBusyService;

    IDockGlobalDialogService IHostOverlayServices.GlobalDialogService => OverlayServices.GlobalDialogService;

    IDockGlobalConfirmationService IHostOverlayServices.GlobalConfirmationService => OverlayServices.GlobalConfirmationService;
}
