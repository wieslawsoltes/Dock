using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public sealed class BusyRootDock : RoutableRootDock, IDisposable
{
    private bool _isBusy;
    private string? _busyMessage;
    private bool _isDisposed;

    public BusyRootDock(
        IScreen host,
        IBusyService busyService,
        IGlobalBusyService globalBusyService,
        IDialogService dialogService,
        IGlobalDialogService globalDialogService,
        IConfirmationService confirmationService,
        IGlobalConfirmationService globalConfirmationService)
        : base(host, "root")
    {
        BusyService = busyService;
        GlobalBusyService = globalBusyService;
        DialogService = dialogService;
        GlobalDialogService = globalDialogService;
        ConfirmationService = confirmationService;
        GlobalConfirmationService = globalConfirmationService;
        SyncFromService();
        BusyService.PropertyChanged += OnBusyServiceChanged;
        BusyService.SetReloadHandler(ReloadCurrentAsync);
    }

    public IBusyService BusyService { get; }

    public IGlobalBusyService GlobalBusyService { get; }

    public IDialogService DialogService { get; }

    public IGlobalDialogService GlobalDialogService { get; }

    public IConfirmationService ConfirmationService { get; }

    public IGlobalConfirmationService GlobalConfirmationService { get; }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isBusy, value);
            this.RaisePropertyChanged(nameof(IsDockEnabled));
        }
    }

    public string? BusyMessage
    {
        get => _busyMessage;
        private set => this.RaiseAndSetIfChanged(ref _busyMessage, value);
    }

    public bool IsDockEnabled => !IsBusy;

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        BusyService.PropertyChanged -= OnBusyServiceChanged;
        BusyService.SetReloadHandler(null);
        DialogService.CancelAll();
        ConfirmationService.CancelAll();
    }

    private void OnBusyServiceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IBusyService.IsBusy)
            || e.PropertyName == nameof(IBusyService.Message))
        {
            SyncFromService();
        }
    }

    private void SyncFromService()
    {
        IsBusy = BusyService.IsBusy;
        BusyMessage = BusyService.Message;
    }

    private async Task ReloadCurrentAsync()
    {
        var reloadable = FindReloadable(ActiveDockable);
        if (reloadable is null)
        {
            return;
        }

        await reloadable.ReloadAsync().ConfigureAwait(false);
    }

    private static IReloadable? FindReloadable(IDockable? dockable)
    {
        if (dockable is null)
        {
            return null;
        }

        if (dockable is IDock dock && dock.ActiveDockable is not null)
        {
            return FindReloadable(dock.ActiveDockable);
        }

        if (dockable is IScreen screen)
        {
            var current = screen.Router.NavigationStack.LastOrDefault();
            if (current is IReloadable reloadable)
            {
                return reloadable;
            }
        }

        return dockable as IReloadable;
    }
}
