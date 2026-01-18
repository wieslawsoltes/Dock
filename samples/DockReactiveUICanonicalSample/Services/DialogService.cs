using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogService : ReactiveObject, IDialogService
{
    private readonly IGlobalDialogService _globalDialogService;
    private readonly ObservableCollection<DialogRequest> _dialogs;
    private readonly ReadOnlyObservableCollection<DialogRequest> _readOnlyDialogs;
    private DialogRequest? _activeDialog;

    public DialogService(IGlobalDialogService globalDialogService)
    {
        _globalDialogService = globalDialogService;
        _dialogs = new ObservableCollection<DialogRequest>();
        _readOnlyDialogs = new ReadOnlyObservableCollection<DialogRequest>(_dialogs);
    }

    public ReadOnlyObservableCollection<DialogRequest> Dialogs => _readOnlyDialogs;

    public DialogRequest? ActiveDialog
    {
        get => _activeDialog;
        private set => this.RaiseAndSetIfChanged(ref _activeDialog, value);
    }

    public bool HasDialogs => _dialogs.Count > 0;

    public async Task<T?> ShowAsync<T>(object content, string? title = null)
    {
        var request = new DialogRequest(content, title, Close);
        request.GlobalHandle = _globalDialogService.Begin();

        if (content is IDialogContent dialogContent)
        {
            dialogContent.SetCloseAction(result => Close(request, result));
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _dialogs.Add(request);
            ActiveDialog = request;
            this.RaisePropertyChanged(nameof(HasDialogs));
        });

        var result = await request.Task.ConfigureAwait(false);
        return result is T typed ? typed : default;
    }

    public void Close(DialogRequest request, object? result = null)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CloseOnUiThread(request, result);
            return;
        }

        Dispatcher.UIThread.Post(() => CloseOnUiThread(request, result));
    }

    public void CancelAll()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            CancelAllOnUiThread();
            return;
        }

        Dispatcher.UIThread.Post(CancelAllOnUiThread);
    }

    private void CloseOnUiThread(DialogRequest request, object? result)
    {
        if (!_dialogs.Contains(request))
        {
            return;
        }

        _dialogs.Remove(request);
        request.GlobalHandle?.Dispose();
        request.Complete(result);
        ActiveDialog = _dialogs.LastOrDefault();
        this.RaisePropertyChanged(nameof(HasDialogs));
    }

    private void CancelAllOnUiThread()
    {
        if (_dialogs.Count == 0)
        {
            return;
        }

        var pending = _dialogs.ToArray();
        foreach (var request in pending)
        {
            CloseOnUiThread(request, null);
        }
    }
}
