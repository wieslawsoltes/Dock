using System.Collections.Generic;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class ConfirmationServiceProvider : IConfirmationServiceProvider
{
    private readonly IConfirmationService _fallback;

    public ConfirmationServiceProvider(IConfirmationServiceFactory confirmationServiceFactory)
    {
        _fallback = confirmationServiceFactory.Create();
    }

    public IConfirmationService GetConfirmationService(IScreen hostScreen)
    {
        return FindBusyRootDock(hostScreen)?.ConfirmationService ?? _fallback;
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
