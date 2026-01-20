using System;
using Dock.Model.Controls;
using Dock.Model.Services;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class DockViewModel : ReactiveObject, IRoutableViewModel, IHostOverlayServices
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;
    private IDockBusyService? _busyService;
    private IDockGlobalBusyService? _globalBusyService;
    private IDockDialogService? _dialogService;
    private IDockGlobalDialogService? _globalDialogService;
    private IDockConfirmationService? _confirmationService;
    private IDockGlobalConfirmationService? _globalConfirmationService;

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
        {
            this.RaiseAndSetIfChanged(ref _layout, value);
            if (value is BusyRootDock busyRoot)
            {
                BusyService = busyRoot.BusyService;
                GlobalBusyService = busyRoot.GlobalBusyService;
                DialogService = busyRoot.DialogService;
                GlobalDialogService = busyRoot.GlobalDialogService;
                ConfirmationService = busyRoot.ConfirmationService;
                GlobalConfirmationService = busyRoot.GlobalConfirmationService;
            }
            else
            {
                BusyService = null;
                GlobalBusyService = null;
                DialogService = null;
                GlobalDialogService = null;
                ConfirmationService = null;
                GlobalConfirmationService = null;
            }
        }
    }

    public IDockBusyService? BusyService
    {
        get => _busyService;
        private set => this.RaiseAndSetIfChanged(ref _busyService, value);
    }

    public IDockGlobalBusyService? GlobalBusyService
    {
        get => _globalBusyService;
        private set => this.RaiseAndSetIfChanged(ref _globalBusyService, value);
    }

    public IDockDialogService? DialogService
    {
        get => _dialogService;
        private set => this.RaiseAndSetIfChanged(ref _dialogService, value);
    }

    public IDockGlobalDialogService? GlobalDialogService
    {
        get => _globalDialogService;
        private set => this.RaiseAndSetIfChanged(ref _globalDialogService, value);
    }

    public IDockConfirmationService? ConfirmationService
    {
        get => _confirmationService;
        private set => this.RaiseAndSetIfChanged(ref _confirmationService, value);
    }

    public IDockGlobalConfirmationService? GlobalConfirmationService
    {
        get => _globalConfirmationService;
        private set => this.RaiseAndSetIfChanged(ref _globalConfirmationService, value);
    }

    IDockBusyService IHostOverlayServices.Busy => BusyService ?? throw new InvalidOperationException("Busy service is not available.");

    IDockDialogService IHostOverlayServices.Dialogs => DialogService ?? throw new InvalidOperationException("Dialog service is not available.");

    IDockConfirmationService IHostOverlayServices.Confirmations => ConfirmationService ?? throw new InvalidOperationException("Confirmation service is not available.");

    IDockGlobalBusyService IHostOverlayServices.GlobalBusyService
        => _globalBusyService ?? throw new InvalidOperationException("Global busy service is not available.");

    IDockGlobalDialogService IHostOverlayServices.GlobalDialogService
        => _globalDialogService ?? throw new InvalidOperationException("Global dialog service is not available.");

    IDockGlobalConfirmationService IHostOverlayServices.GlobalConfirmationService
        => _globalConfirmationService ?? throw new InvalidOperationException("Global confirmation service is not available.");
}
