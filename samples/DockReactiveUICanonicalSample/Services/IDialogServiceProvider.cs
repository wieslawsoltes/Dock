using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IDialogServiceProvider
{
    IDialogService GetDialogService(IScreen hostScreen);
}
