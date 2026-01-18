using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IConfirmationServiceProvider
{
    IDockConfirmationService GetConfirmationService(IScreen hostScreen);
}
