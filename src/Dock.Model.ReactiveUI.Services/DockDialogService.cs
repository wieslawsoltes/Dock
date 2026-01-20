using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Default dialog service implementation.
/// </summary>
public sealed partial class DockDialogService : ReactiveObject, IDockDialogService
{
    private readonly IDockGlobalDialogService? _globalDialogService;
    private readonly ServiceDispatcher _dispatcher;
    private readonly ObservableCollection<DialogRequest> _dialogs;
    private readonly ReadOnlyObservableCollection<DialogRequest> _readOnlyDialogs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockDialogService"/> class.
    /// </summary>
    /// <param name="globalDialogService">The optional global dialog service.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockDialogService(
        IDockGlobalDialogService? globalDialogService = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _globalDialogService = globalDialogService;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
        _dialogs = new ObservableCollection<DialogRequest>();
        _readOnlyDialogs = new ReadOnlyObservableCollection<DialogRequest>(_dialogs);
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<DialogRequest> Dialogs => _readOnlyDialogs;

    /// <inheritdoc />
    [Reactive]
    public partial DialogRequest? ActiveDialog { get; private set; }

    /// <inheritdoc />
    public bool HasDialogs => _dialogs.Count > 0;

    /// <inheritdoc />
    public async Task<T?> ShowAsync<T>(object content, string? title = null)
    {
        var request = new DialogRequest(content, title, Close)
        {
            GlobalHandle = _globalDialogService?.Begin()
        };

        if (content is IDockDialogContent dialogContent)
        {
            dialogContent.SetCloseAction(result => Close(request, result));
        }

        await _dispatcher.InvokeAsync(() =>
        {
            if (request.Task.IsCompleted)
            {
                return;
            }

            _dialogs.Add(request);
            ActiveDialog = request;
            this.RaisePropertyChanged(nameof(HasDialogs));
        }).ConfigureAwait(false);

        var result = await request.Task.ConfigureAwait(false);
        return result is T typed ? typed : default;
    }

    /// <inheritdoc />
    public void Close(DialogRequest request, object? result = null)
    {
        if (_dispatcher.CheckAccess())
        {
            CloseOnDispatcher(request, result);
            return;
        }

        _dispatcher.Post(() => CloseOnDispatcher(request, result));
    }

    /// <inheritdoc />
    public void CancelAll()
    {
        if (_dispatcher.CheckAccess())
        {
            CancelAllOnDispatcher();
            return;
        }

        _dispatcher.Post(CancelAllOnDispatcher);
    }

    private void CloseOnDispatcher(DialogRequest request, object? result)
    {
        if (!_dialogs.Contains(request))
        {
            request.GlobalHandle?.Dispose();
            request.Complete(result);
            return;
        }

        _dialogs.Remove(request);
        request.GlobalHandle?.Dispose();
        request.Complete(result);
        ActiveDialog = _dialogs.LastOrDefault();
        this.RaisePropertyChanged(nameof(HasDialogs));
    }

    private void CancelAllOnDispatcher()
    {
        if (_dialogs.Count == 0)
        {
            return;
        }

        var pending = _dialogs.ToArray();
        foreach (var request in pending)
        {
            CloseOnDispatcher(request, null);
        }
    }
}
