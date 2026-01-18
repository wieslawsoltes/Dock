using System.Collections.Generic;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DialogServiceProvider : IDialogServiceProvider
{
    private readonly IDialogService _fallback;

    public DialogServiceProvider(IDialogServiceFactory dialogServiceFactory)
    {
        _fallback = dialogServiceFactory.Create();
    }

    public IDialogService GetDialogService(IScreen hostScreen)
    {
        return FindBusyRootDock(hostScreen)?.DialogService ?? _fallback;
    }

    private static BusyRootDock? FindBusyRootDock(IScreen hostScreen)
    {
        var visited = new HashSet<IScreen>();
        var currentScreen = hostScreen;

        while (currentScreen is not null && visited.Add(currentScreen))
        {
            if (currentScreen is BusyRootDock busyRoot)
            {
                return ResolveBusyRootDock(busyRoot);
            }

            if (currentScreen is IDockable dockable)
            {
                if (FindBusyRootDockInOwnerChain(dockable) is { } busyRootDock)
                {
                    return ResolveBusyRootDock(busyRootDock);
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

    private static BusyRootDock? FindBusyRootDockInOwnerChain(IDockable dockable)
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

        return null;
    }

    private static BusyRootDock ResolveBusyRootDock(BusyRootDock busyRoot)
    {
        if (busyRoot.Window is not null)
        {
            return busyRoot;
        }

        if (busyRoot.HostScreen is IDockable hostDockable)
        {
            if (FindBusyRootDockInOwnerChain(hostDockable) is { } outerRoot
                && !ReferenceEquals(outerRoot, busyRoot))
            {
                return outerRoot;
            }
        }

        return busyRoot;
    }

}
