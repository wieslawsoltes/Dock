using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using Dock.Model.ReactiveUI.Services.Lifecycle;
using Dock.Model.ReactiveUI.Services.Overlays.Hosting;
using Dock.Model.Services;
using ReactiveUI;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class DockWindowLifecycleServiceTests
{
    private sealed class TestRootDock : RootDock, IScreen, IDisposable
    {
        public RoutingState Router { get; } = new();

        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    private sealed class TestOverlayServices : IHostOverlayServices
    {
        public TestOverlayServices()
        {
            Busy = new TestBusyService();
            Dialogs = new TestDialogService();
            Confirmations = new TestConfirmationService();
            GlobalBusyService = new TestGlobalBusyService();
            GlobalDialogService = new TestGlobalDialogService();
            GlobalConfirmationService = new TestGlobalConfirmationService();
        }

        public IDockBusyService Busy { get; }

        public IDockDialogService Dialogs { get; }

        public IDockConfirmationService Confirmations { get; }

        public IDockGlobalBusyService GlobalBusyService { get; }

        public IDockGlobalDialogService GlobalDialogService { get; }

        public IDockGlobalConfirmationService GlobalConfirmationService { get; }
    }

    private sealed class TestOverlayProvider : IHostOverlayServicesProvider
    {
        private readonly IHostOverlayServices _services;

        public TestOverlayProvider(IHostOverlayServices services)
        {
            _services = services;
        }

        public IHostOverlayServices GetServices(IScreen hostScreen)
        {
            return _services;
        }

        public IHostOverlayServices GetServices(IScreen hostScreen, IDockable dockable)
        {
            return _services;
        }
    }

    [Fact]
    public void OnWindowClosed_CleansOverlays_AndDisposesLayout()
    {
        var overlays = new TestOverlayServices();
        var provider = new TestOverlayProvider(overlays);
        var lifecycle = new DockWindowLifecycleService(provider);
        var root = new TestRootDock();
        var window = new DockWindow { Layout = root };

        lifecycle.OnWindowClosed(window);

        Assert.Equal(1, ((TestDialogService)overlays.Dialogs).CancelAllCalls);
        Assert.Equal(1, ((TestConfirmationService)overlays.Confirmations).CancelAllCalls);
        Assert.Equal(1, root.DisposeCount);
    }

    [Fact]
    public void OnWindowClosed_IsIdempotent()
    {
        var overlays = new TestOverlayServices();
        var provider = new TestOverlayProvider(overlays);
        var lifecycle = new DockWindowLifecycleService(provider);
        var root = new TestRootDock();
        var window = new DockWindow { Layout = root };

        lifecycle.OnWindowClosed(window);
        lifecycle.OnWindowRemoved(window);

        Assert.Equal(1, ((TestDialogService)overlays.Dialogs).CancelAllCalls);
        Assert.Equal(1, ((TestConfirmationService)overlays.Confirmations).CancelAllCalls);
        Assert.Equal(1, root.DisposeCount);
    }

    private sealed class TestDialogService : IDockDialogService
    {
        private readonly ObservableCollection<DialogRequest> _dialogs = new();

        public TestDialogService()
        {
            Dialogs = new ReadOnlyObservableCollection<DialogRequest>(_dialogs);
        }

        public int CancelAllCalls { get; private set; }

        public ReadOnlyObservableCollection<DialogRequest> Dialogs { get; }

        public DialogRequest? ActiveDialog => _dialogs.Count > 0 ? _dialogs[0] : null;

        public bool HasDialogs => _dialogs.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Task<T?> ShowAsync<T>(object content, string? title = null)
        {
            return Task.FromResult<T?>(default);
        }

        public void Close(DialogRequest request, object? result = null)
        {
        }

        public void CancelAll()
        {
            CancelAllCalls++;
            _dialogs.Clear();
            RaisePropertyChanged(nameof(HasDialogs));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class TestConfirmationService : IDockConfirmationService
    {
        private readonly ObservableCollection<ConfirmationRequest> _confirmations = new();

        public TestConfirmationService()
        {
            Confirmations = new ReadOnlyObservableCollection<ConfirmationRequest>(_confirmations);
        }

        public int CancelAllCalls { get; private set; }

        public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations { get; }

        public ConfirmationRequest? ActiveConfirmation => _confirmations.Count > 0 ? _confirmations[0] : null;

        public bool HasConfirmations => _confirmations.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Task<bool> ConfirmAsync(
            string title,
            string message,
            string confirmText = "Confirm",
            string cancelText = "Cancel")
        {
            return Task.FromResult(false);
        }

        public void Close(ConfirmationRequest request, bool result)
        {
        }

        public void CancelAll()
        {
            CancelAllCalls++;
            _confirmations.Clear();
            RaisePropertyChanged(nameof(HasConfirmations));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class TestBusyService : IDockBusyService
    {
        public bool IsBusy { get; private set; }

        public string? Message { get; private set; }

        public bool IsReloadVisible { get; set; }

        public bool CanReload { get; private set; }

        public System.Windows.Input.ICommand ReloadCommand { get; } = ReactiveCommand.Create(() => { });

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string message)
        {
            IsBusy = true;
            Message = message;
            RaisePropertyChanged(nameof(IsBusy));
            RaisePropertyChanged(nameof(Message));
            return new EmptyDisposable();
        }

        public Task RunAsync(string message, Func<Task> action)
        {
            return action();
        }

        public void UpdateMessage(string? message)
        {
            Message = message;
            RaisePropertyChanged(nameof(Message));
        }

        public void SetReloadHandler(Func<Task>? handler)
        {
            CanReload = handler is not null;
            RaisePropertyChanged(nameof(CanReload));
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class TestGlobalBusyService : IDockGlobalBusyService
    {
        public bool IsBusy { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsBusy = true;
            Message = message;
            RaisePropertyChanged(nameof(IsBusy));
            RaisePropertyChanged(nameof(Message));
            return new EmptyDisposable();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class TestGlobalDialogService : IDockGlobalDialogService
    {
        public bool IsDialogOpen { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsDialogOpen = true;
            Message = message;
            RaisePropertyChanged(nameof(IsDialogOpen));
            RaisePropertyChanged(nameof(Message));
            return new EmptyDisposable();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class TestGlobalConfirmationService : IDockGlobalConfirmationService
    {
        public bool IsConfirmationOpen { get; private set; }

        public string? Message { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IDisposable Begin(string? message = null)
        {
            IsConfirmationOpen = true;
            Message = message;
            RaisePropertyChanged(nameof(IsConfirmationOpen));
            RaisePropertyChanged(nameof(Message));
            return new EmptyDisposable();
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
