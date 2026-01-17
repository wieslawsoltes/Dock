using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DockReactiveUICanonicalSample.Services;

public interface IDialogService : INotifyPropertyChanged
{
    ReadOnlyObservableCollection<DialogRequest> Dialogs { get; }

    DialogRequest? ActiveDialog { get; }

    bool HasDialogs { get; }

    Task<T?> ShowAsync<T>(object content, string? title = null);

    void Close(DialogRequest request, object? result = null);
}
