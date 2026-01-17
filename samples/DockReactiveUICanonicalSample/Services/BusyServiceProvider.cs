using System.Collections.Generic;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class BusyServiceProvider : IBusyServiceProvider
{
    private readonly IBusyService _fallback;

    public BusyServiceProvider(IBusyServiceFactory busyServiceFactory)
    {
        _fallback = busyServiceFactory.Create();
    }

    public IBusyService GetBusyService(IScreen hostScreen)
    {
        return FindBusyRootDock(hostScreen)?.BusyService ?? _fallback;
    }

    private static BusyRootDock? FindBusyRootDock(IScreen hostScreen)
    {
        var visited = new HashSet<IScreen>();
        var currentScreen = hostScreen;

        while (currentScreen is not null && visited.Add(currentScreen))
        {
            if (currentScreen is BusyRootDock busyRoot)
            {
                return busyRoot;
            }

            if (currentScreen is IDockable dockable)
            {
                var currentDockable = dockable;
                while (currentDockable is not null)
                {
                    if (currentDockable is BusyRootDock busyRootDock)
                    {
                        return busyRootDock;
                    }

                    currentDockable = currentDockable.Owner;
                }
            }

            if (currentScreen is IRoutableViewModel routable && routable.HostScreen is { } nextScreen)
            {
                currentScreen = nextScreen;
                continue;
            }

            break;
        }

        return null;
    }
}
