using System.Collections.Generic;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Services.Overlays.Hosting;

/// <summary>
/// Resolves services by walking the screen and owner chains.
/// </summary>
public sealed class OwnerChainHostServiceResolver : IHostServiceResolver
{
    /// <inheritdoc />
    public TService? Resolve<TService>(IScreen screen) where TService : class
    {
        var visited = new HashSet<IScreen>();
        var current = screen;

        while (current is not null && visited.Add(current))
        {
            var resolved = ResolveFromScreen<TService>(current);
            if (resolved is not null)
            {
                return NormalizeRoot(resolved);
            }

            var routerResolved = ResolveFromRouter<TService>(current);
            if (routerResolved is not null)
            {
                return NormalizeRoot(routerResolved);
            }

            if (current is IRoutableViewModel routable && routable.HostScreen is { } nextScreen)
            {
                current = nextScreen;
                continue;
            }

            break;
        }

        return null;
    }

    private static TService? ResolveFromRouter<TService>(IScreen screen) where TService : class
    {
        var stack = screen.Router?.NavigationStack;
        if (stack is null)
        {
            return null;
        }

        for (var index = stack.Count - 1; index >= 0; index--)
        {
            if (stack[index] is TService service)
            {
                return service;
            }
        }

        return null;
    }

    private static TService? ResolveFromScreen<TService>(IScreen screen) where TService : class
    {
        if (screen is IDockable dockable)
        {
            var hostWindowResolved = FindInHostWindows<TService>(dockable);
            if (hostWindowResolved is not null)
            {
                return hostWindowResolved;
            }

            var ownerResolved = FindInOwnerChainLast<TService>(dockable);
            if (ownerResolved is not null)
            {
                return ownerResolved;
            }

            return FindInFactory<TService>(dockable);
        }

        if (screen is TService direct)
        {
            return direct;
        }

        return null;
    }

    private static TService? FindInOwnerChain<TService>(IDockable dockable) where TService : class
    {
        var current = dockable;
        while (current is not null)
        {
            if (current is TService service)
            {
                return service;
            }

            current = current.Owner;
        }

        return null;
    }

    private static TService? FindInOwnerChainLast<TService>(IDockable dockable) where TService : class
    {
        TService? result = null;
        var current = dockable;
        while (current is not null)
        {
            if (current is TService service)
            {
                result = service;
            }

            current = current.Owner;
        }

        return result;
    }

    private static TService? FindInHostWindows<TService>(IDockable dockable) where TService : class
    {
        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return null;
        }

        foreach (var hostWindow in factory.HostWindows)
        {
            var window = hostWindow.Window;
            if (window?.Layout is not IDock layout)
            {
                continue;
            }

            if (ReferenceEquals(layout, dockable)
                || factory.FindDockable(layout, d => ReferenceEquals(d, dockable)) is not null)
            {
                if (window.Layout is TService service)
                {
                    return service;
                }
            }
        }

        return null;
    }

    private static TService? FindInFactory<TService>(IDockable dockable) where TService : class
    {
        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return null;
        }

        foreach (var root in factory.Find(d => d is IRootDock).OfType<IRootDock>())
        {
            if (root is not IDock dock)
            {
                continue;
            }

            if (ReferenceEquals(root, dockable) || ContainsDockable(factory, dock, dockable))
            {
                if (root is TService service)
                {
                    return service;
                }
            }
        }

        foreach (var root in factory.Find(d => d is IRootDock).OfType<IRootDock>())
        {
            if (root.Windows is null)
            {
                continue;
            }

            foreach (var window in root.Windows)
            {
                if (window.Layout is not IDock windowLayout)
                {
                    continue;
                }

                if (ReferenceEquals(windowLayout, dockable)
                    || factory.FindDockable(windowLayout, d => ReferenceEquals(d, dockable)) is not null)
                {
                    if (windowLayout is TService service)
                    {
                        return service;
                    }
                }
            }
        }

        return null;
    }

    private static bool ContainsDockable(IFactory factory, IDock dock, IDockable target)
    {
        foreach (var _ in factory.Find(dock, dockable => ReferenceEquals(dockable, target)))
        {
            return true;
        }

        return false;
    }

    private static IFactory? FindFactory(IDockable dockable)
    {
        var current = dockable;
        while (current is not null)
        {
            if (current.Factory is IFactory factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }

    private static TService NormalizeRoot<TService>(TService service) where TService : class
    {
        if (service is not IRootDock root || root.Window is not null)
        {
            return service;
        }

        if (service is IRoutableViewModel routable && routable.HostScreen is IDockable hostDockable)
        {
            var outer = FindInOwnerChainLast<TService>(hostDockable)
                ?? FindInHostWindows<TService>(hostDockable)
                ?? FindInFactory<TService>(hostDockable);
            if (outer is IRootDock outerRoot && !ReferenceEquals(outerRoot, root))
            {
                return outer;
            }
        }

        return service;
    }

    // Window-specific roots are resolved via the owner chain or by scanning window layouts.
}
