using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DockReactiveUICanonicalSample.Services;

public interface IConfirmationService : INotifyPropertyChanged
{
    ReadOnlyObservableCollection<ConfirmationRequest> Confirmations { get; }

    ConfirmationRequest? ActiveConfirmation { get; }

    bool HasConfirmations { get; }

    Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel");

    void Close(ConfirmationRequest request, bool result);
}
