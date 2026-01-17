using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationService : ReactiveObject, IConfirmationService
{
    private readonly IGlobalConfirmationService _globalConfirmationService;
    private readonly ObservableCollection<ConfirmationRequest> _confirmations;
    private readonly ReadOnlyObservableCollection<ConfirmationRequest> _readOnlyConfirmations;
    private ConfirmationRequest? _activeConfirmation;

    public ConfirmationService(IGlobalConfirmationService globalConfirmationService)
    {
        _globalConfirmationService = globalConfirmationService;
        _confirmations = new ObservableCollection<ConfirmationRequest>();
        _readOnlyConfirmations = new ReadOnlyObservableCollection<ConfirmationRequest>(_confirmations);
    }

    public ReadOnlyObservableCollection<ConfirmationRequest> Confirmations => _readOnlyConfirmations;

    public ConfirmationRequest? ActiveConfirmation
    {
        get => _activeConfirmation;
        private set => this.RaiseAndSetIfChanged(ref _activeConfirmation, value);
    }

    public bool HasConfirmations => _confirmations.Count > 0;

    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        var request = new ConfirmationRequest(title, message, confirmText, cancelText, Close);
        request.GlobalHandle = _globalConfirmationService.Begin();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _confirmations.Add(request);
            ActiveConfirmation = request;
            this.RaisePropertyChanged(nameof(HasConfirmations));
        });

        return await request.Task.ConfigureAwait(false);
    }

    public void Close(ConfirmationRequest request, bool result)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CloseOnUiThread(request, result);
            return;
        }

        Dispatcher.UIThread.Post(() => CloseOnUiThread(request, result));
    }

    private void CloseOnUiThread(ConfirmationRequest request, bool result)
    {
        if (!_confirmations.Contains(request))
        {
            return;
        }

        _confirmations.Remove(request);
        request.GlobalHandle?.Dispose();
        request.Complete(result);
        ActiveConfirmation = _confirmations.LastOrDefault();
        this.RaisePropertyChanged(nameof(HasConfirmations));
    }
}
