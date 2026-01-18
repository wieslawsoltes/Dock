using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IDialogServiceProvider
{
    IDockDialogService GetDialogService(IScreen hostScreen);
}
