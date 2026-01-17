using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IBusyServiceProvider
{
    IBusyService GetBusyService(IScreen hostScreen);
}
