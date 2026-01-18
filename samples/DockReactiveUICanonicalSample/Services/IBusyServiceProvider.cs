using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IBusyServiceProvider
{
    IDockBusyService GetBusyService(IScreen hostScreen);
}
