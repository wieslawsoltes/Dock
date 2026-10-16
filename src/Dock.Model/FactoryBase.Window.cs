// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;
using System.Collections.Generic;
using System.Linq;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <summary>
    /// Resolves the root dock that should own floating window registrations.
    /// </summary>
    /// <param name="rootDock">The root where the floating operation originated.</param>
    /// <returns>The top-level workspace root dock.</returns>
    protected virtual IRootDock ResolveWindowCollectionRoot(IRootDock rootDock)
    {
        var current = rootDock;
        var visited = new HashSet<IRootDock> { current };

        while (current.Window?.Owner is IRootDock parentRoot)
        {
            if (!visited.Add(parentRoot))
            {
                break;
            }

            current = parentRoot;
        }

        return current;
    }

    /// <inheritdoc/>
    public virtual void AddWindow(IRootDock rootDock, IDockWindow window)
    {
        rootDock.Windows ??= CreateList<IDockWindow>();
        rootDock.Windows.Add(window);
        InitializeAddedWindow(rootDock, window);
    }

    /// <inheritdoc/>
    public virtual void InsertWindow(IRootDock rootDock, IDockWindow window, int index)
    {
        if (index >= 0)
        {
            rootDock.Windows ??= CreateList<IDockWindow>();
            rootDock.Windows.Insert(index, window);
            InitializeAddedWindow(rootDock, window);
        }
    }

    private void InitializeAddedWindow(IRootDock rootDock, IDockWindow window)
    {
        // Collection and factory callbacks can cancel registration or transfer
        // the window to another root before initialization creates its host.
        if (rootDock.Windows?.Contains(window) != true)
        {
            return;
        }

        OnWindowAdded(window);
        if (rootDock.Windows?.Contains(window) == true)
        {
            InitDockWindow(window, rootDock);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveWindow(IDockWindow window)
    {
        if (window.Owner is IRootDock rootDock)
        {
            var layout = window.Layout;
            window.Exit();
            if (!ReferenceEquals(window.Owner, rootDock) ||
                window.Host is IHostWindowCloseState { IsClosed: false })
            {
                return;
            }
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);

            if (layout is not null && ReferenceEquals(layout.Window, window))
            {
                layout.Window = null;
            }

            window.ParentWindow = null;
            window.Owner = null;
            window.Factory = null;
            window.Layout = null;
        }
    }

    /// <inheritdoc/>
    public virtual void CloseWindow(IDockWindow window)
    {
        if (window.Layout is IDock layout)
        {
            void CloseDock(IDock dock)
            {
                if (dock.VisibleDockables is null)
                {
                    return;
                }

                foreach (var item in dock.VisibleDockables.ToList())
                {
                    if (item is IDock subDock)
                    {
                        CloseDock(subDock);
                    }

                    CloseDockable(item);
                }
            }

            CloseDock(layout);
        }
    }
}
