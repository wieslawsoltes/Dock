using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IConfirmationServiceProvider
{
    IConfirmationService GetConfirmationService(IScreen hostScreen);
}
