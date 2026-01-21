using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Dock.Model.Services;

namespace Dock.Model.ReactiveUI.Services;

/// <summary>
/// Default confirmation service implementation.
/// </summary>
public sealed partial class DockConfirmationService : ReactiveObject, IDockConfirmationService
{
    private readonly IDockGlobalConfirmationService? _globalConfirmationService;
    private readonly ServiceDispatcher _dispatcher;
    private readonly ObservableCollection<ConfirmationRequest> _confirmations;
    private readonly ReadOnlyObservableCollection<ConfirmationRequest> _readOnlyConfirmations;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockConfirmationService"/> class.
    /// </summary>
    /// <param name="globalConfirmationService">The optional global confirmation service.</param>
    /// <param name="synchronizationContext">The synchronization context used for notifications.</param>
    public DockConfirmationService(
        IDockGlobalConfirmationService? globalConfirmationService = null,
        SynchronizationContext? synchronizationContext = null)
    {
        _globalConfirmationService = globalConfirmationService;
        _dispatcher = new ServiceDispatcher(synchronizationContext);
        _confirmations = new ObservableCollection<ConfirmationRequest>();
        _readOnlyConfirmations = new ReadOnlyObservableCollection<ConfirmationRequest>(_confirmations);
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations => _readOnlyConfirmations;

    /// <inheritdoc />
    [Reactive]
    public partial ConfirmationRequest? ActiveConfirmation { get; private set; }

    /// <inheritdoc />
    public bool HasConfirmations => _confirmations.Count > 0;

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        var request = new ConfirmationRequest(title, message, confirmText, cancelText, Close)
        {
            GlobalHandle = _globalConfirmationService?.Begin()
        };

        await _dispatcher.InvokeAsync(() =>
        {
            _confirmations.Add(request);
            ActiveConfirmation = request;
            this.RaisePropertyChanged(nameof(HasConfirmations));
        }).ConfigureAwait(false);

        return await request.Task.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Close(ConfirmationRequest request, bool result)
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

    private void CloseOnDispatcher(ConfirmationRequest request, bool result)
    {
        if (!_confirmations.Contains(request))
        {
            request.GlobalHandle?.Dispose();
            request.Complete(result);
            return;
        }

        _confirmations.Remove(request);
        request.GlobalHandle?.Dispose();
        request.Complete(result);
        ActiveConfirmation = _confirmations.LastOrDefault();
        this.RaisePropertyChanged(nameof(HasConfirmations));
    }

    private void CancelAllOnDispatcher()
    {
        if (_confirmations.Count == 0)
        {
            return;
        }

        var pending = _confirmations.ToArray();
        foreach (var request in pending)
        {
            CloseOnDispatcher(request, false);
        }
    }
}
