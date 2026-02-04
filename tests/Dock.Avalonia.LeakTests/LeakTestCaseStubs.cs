using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Dock.Model.Services;

namespace Dock.Avalonia.LeakTests;

internal sealed class NoOpDisposable : IDisposable
{
    public void Dispose()
    {
    }
}

internal sealed class NoOpCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
    }
}

internal sealed class StubBusyService : IDockBusyService
{
    private readonly NoOpDisposable _scope = new();
    private Func<Task>? _reloadHandler;

    public bool IsBusy { get; private set; }

    public string? Message { get; private set; }

    public bool IsReloadVisible { get; set; }

    public bool CanReload { get; private set; }

    public ICommand ReloadCommand { get; } = new NoOpCommand();

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Begin(string message)
    {
        IsBusy = true;
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        return _scope;
    }

    public Task RunAsync(string message, Func<Task> action)
    {
        IsBusy = true;
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        return action();
    }

    public void UpdateMessage(string? message)
    {
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
    }

    public void SetReloadHandler(Func<Task>? handler)
    {
        _reloadHandler = handler;
        CanReload = handler is not null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReload)));
    }
}

internal sealed class StubGlobalBusyService : IDockGlobalBusyService
{
    public bool IsBusy { get; private set; }

    public string? Message { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Begin(string? message = null)
    {
        IsBusy = true;
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        return new NoOpDisposable();
    }
}

internal sealed class StubDialogService : IDockDialogService
{
    private readonly ObservableCollection<DialogRequest> _dialogs = new();
    private readonly ReadOnlyObservableCollection<DialogRequest> _readonlyDialogs;

    public StubDialogService()
    {
        _readonlyDialogs = new ReadOnlyObservableCollection<DialogRequest>(_dialogs);
    }

    public ReadOnlyObservableCollection<DialogRequest> Dialogs => _readonlyDialogs;

    public DialogRequest? ActiveDialog => null;

    public bool HasDialogs => false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Task<T?> ShowAsync<T>(object content, string? title = null) => Task.FromResult<T?>(default);

    public void Close(DialogRequest request, object? result = null)
    {
    }

    public void CancelAll()
    {
    }
}

internal sealed class StubGlobalDialogService : IDockGlobalDialogService
{
    public bool IsDialogOpen { get; private set; }

    public string? Message { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Begin(string? message = null)
    {
        IsDialogOpen = true;
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDialogOpen)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        return new NoOpDisposable();
    }
}

internal sealed class StubConfirmationService : IDockConfirmationService
{
    private readonly ObservableCollection<ConfirmationRequest> _confirmations = new();
    private readonly ReadOnlyObservableCollection<ConfirmationRequest> _readonlyConfirmations;

    public StubConfirmationService()
    {
        _readonlyConfirmations = new ReadOnlyObservableCollection<ConfirmationRequest>(_confirmations);
    }

    public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations => _readonlyConfirmations;

    public ConfirmationRequest? ActiveConfirmation => null;

    public bool HasConfirmations => false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Task<bool> ConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
        => Task.FromResult(false);

    public void Close(ConfirmationRequest request, bool result)
    {
    }

    public void CancelAll()
    {
    }
}

internal sealed class StubGlobalConfirmationService : IDockGlobalConfirmationService
{
    public bool IsConfirmationOpen { get; private set; }

    public string? Message { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Begin(string? message = null)
    {
        IsConfirmationOpen = true;
        Message = message;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConfirmationOpen)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        return new NoOpDisposable();
    }
}
