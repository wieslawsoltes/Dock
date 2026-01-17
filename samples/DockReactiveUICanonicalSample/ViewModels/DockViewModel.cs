using Dock.Model.Controls;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class DockViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly DockFactory _factory;
    private IRootDock? _layout;
    private IBusyService? _busyService;
    private IGlobalBusyService? _globalBusyService;
    private IDialogService? _dialogService;
    private IGlobalDialogService? _globalDialogService;
    private IConfirmationService? _confirmationService;
    private IGlobalConfirmationService? _globalConfirmationService;

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

    public IBusyService? BusyService
    {
        get => _busyService;
        private set => this.RaiseAndSetIfChanged(ref _busyService, value);
    }

    public IGlobalBusyService? GlobalBusyService
    {
        get => _globalBusyService;
        private set => this.RaiseAndSetIfChanged(ref _globalBusyService, value);
    }

    public IDialogService? DialogService
    {
        get => _dialogService;
        private set => this.RaiseAndSetIfChanged(ref _dialogService, value);
    }

    public IGlobalDialogService? GlobalDialogService
    {
        get => _globalDialogService;
        private set => this.RaiseAndSetIfChanged(ref _globalDialogService, value);
    }

    public IConfirmationService? ConfirmationService
    {
        get => _confirmationService;
        private set => this.RaiseAndSetIfChanged(ref _confirmationService, value);
    }

    public IGlobalConfirmationService? GlobalConfirmationService
    {
        get => _globalConfirmationService;
        private set => this.RaiseAndSetIfChanged(ref _globalConfirmationService, value);
    }
}
