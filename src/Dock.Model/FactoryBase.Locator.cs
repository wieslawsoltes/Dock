using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual Func<object?>? DefaultContextLocator { get; set; }

    /// <inheritdoc/>
    public virtual Func<IHostWindow?>? DefaultHostWindowLocator { get; set; }

    /// <inheritdoc/>
    public virtual Dictionary<string, Func<object?>>? ContextLocator { get; set; }

    /// <inheritdoc/>
    public virtual Dictionary<string, Func<IHostWindow?>>? HostWindowLocator { get; set; }

    /// <inheritdoc/>
    public virtual IDictionary<string, Func<IDockable?>>? DockableLocator { get; set; }

    /// <inheritdoc/>
    public virtual object? GetContext(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (ContextLocator?.TryGetValue(id, out var locator) == true)
        {
            return locator?.Invoke();
        }

        return DefaultContextLocator?.Invoke();
    }

    /// <inheritdoc/>
    public virtual IHostWindow? GetHostWindow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (HostWindowLocator?.TryGetValue(id, out var locator) == true)
        {
            return locator?.Invoke();
        }

        return DefaultHostWindowLocator?.Invoke();
    }

    /// <inheritdoc/>
    public virtual T? GetDockable<T>(string id) where T: class, IDockable
    {
        if (string.IsNullOrEmpty(id))
        {
            return default;
        }

        if (DockableLocator?.TryGetValue(id, out var locator) == true)
        {
            return locator?.Invoke() as T;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual IRootDock? FindRoot(IDockable dockable, Func<IRootDock, bool>? predicate = null)
    {
        if (dockable.Owner is null)
        {
            return null;
        }
        if (dockable.Owner is IRootDock rootDock && (predicate?.Invoke(rootDock) ?? true))
        {
            return rootDock;
        }
        return FindRoot(dockable.Owner, predicate);
    }

    /// <inheritdoc/>
    public virtual IDockable? FindDockable(IDock dock, Func<IDockable, bool> predicate)
    {
        if (predicate(dock))
        {
            return dock;
        }

        if (dock.VisibleDockables is not null)
        {
            foreach (var dockable in dock.VisibleDockables)
            {
                if (predicate(dockable))
                {
                    return dockable;
                }

                if (dockable is IDock childDock)
                {
                    var result = FindDockable(childDock, predicate);
                    if (result is not null)
                    {
                        return result;
                    }
                }
            }
        }

        if (dock is IRootDock rootDock && rootDock.Windows is not null)
        {
            foreach (var window in rootDock.Windows)
            {
                if (window.Layout is null)
                {
                    continue;
                }

                if (predicate(window.Layout))
                {
                    return window.Layout;
                }

                var result = FindDockable(window.Layout, predicate);
                if (result is not null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public IEnumerable<IDockable> Find(Func<IDockable, bool> predicate)
    {
        foreach (var dockControl in DockControls)
        {
            var dock = dockControl.Layout;
            if (dock is null)
            {
                continue;
            }

            foreach (var result in Find(dock, predicate))
            {
                yield return result;
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IDockable> Find(IDock dock, Func<IDockable, bool> predicate)
    {
        if (predicate(dock))
        {
            yield return dock;
        }

        if (dock.VisibleDockables is not null)
        {
            foreach (var dockable in dock.VisibleDockables)
            {
                if (predicate(dockable))
                {
                    yield return dockable;
                }

                if (dockable is IDock childDock)
                {
                    foreach (var result in Find(childDock, predicate))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
