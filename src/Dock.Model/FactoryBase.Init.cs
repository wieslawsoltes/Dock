// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
    public virtual void InitLayout(IDockable layout)
    {
        InitDockable(layout, null);

        if (layout is IDock dock)
        {
            if (dock.DefaultDockable is not null)
            {
                dock.ActiveDockable = dock.DefaultDockable;
            }
        }

        if (layout is IRootDock rootDock)
        {
            if (rootDock.ShowWindows.CanExecute(null))
            {
                rootDock.ShowWindows.Execute(null);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void InitDockable(IDockable dockable, IDockable? owner)
    {
        if (dockable.Context is null)
        {
            if (GetContext(dockable.Id) is { } context)
            {
                dockable.Context = context;
            }
        }
 
        dockable.Owner = owner;

        if (dockable is IDock dock)
        {
            dock.Factory = this;

            if (dock.VisibleDockables is not null)
            {
                InitDockables(dockable, dock.VisibleDockables);

                UpdateIsEmpty(dock);
            }
        }

        if (dockable is IRootDock rootDock)
        {
            if (rootDock.HiddenDockables is not null)
            {
                InitDockables(dockable, rootDock.HiddenDockables);
            }

            if (rootDock.LeftPinnedDockables is not null)
            {
                InitDockables(dockable, rootDock.LeftPinnedDockables);
            }

            if (rootDock.RightPinnedDockables is not null)
            {
                InitDockables(dockable, rootDock.RightPinnedDockables);
            }

            if (rootDock.TopPinnedDockables is not null)
            {
                InitDockables(dockable, rootDock.TopPinnedDockables);
            }

            if (rootDock.BottomPinnedDockables is not null)
            {
                InitDockables(dockable, rootDock.BottomPinnedDockables);
            }

            if (rootDock.Windows is not null)
            {
                foreach (var child in rootDock.Windows)
                {
                    InitDockWindow(child, dockable);
                }
            }
        }

        OnDockableInit(dockable);
    }

    private void InitDockables(IDockable dockable, IList<IDockable> dockables)
    {
        foreach (var child in dockables)
        {
            InitDockable(child, dockable);
        }
    }

    /// <inheritdoc/>
    public virtual void InitDockWindow(IDockWindow window, IDockable? owner)
    {
        window.Host = GetHostWindow(window.Id);
        if (window.Host is not null)
        {
            window.Host.Window = window;
        }

        window.Owner = owner;

        window.Factory = this;

        if (window.Layout is not null)
        {
            InitDockable(window.Layout, window.Layout.Owner);
        }
    }

    /// <inheritdoc/>
    public virtual void InitActiveDockable(IDockable? dockable, IDock owner)
    {
        OnActiveDockableChanged(dockable);

        if (dockable is { })
        {
            InitDockable(dockable, owner);
            dockable.OnSelected();
        }

        if (dockable is { })
        {
            SetFocusedDockable(owner, dockable);
        } 
    }
    
    /// <inheritdoc/>
    public virtual void SetActiveDockable(IDockable dockable)
    {
        if (dockable.Owner is IDock dock)
        {
            dock.ActiveDockable = dockable;
        }
    }

    private void SetIsActive(IDockable dockable, bool active)
    {
        if (dockable is IDock dock)
        {
            dock.IsActive = active;
        }
    }

    /// <inheritdoc />
    public virtual void SetFocusedDockable(IDock dock, IDockable? dockable)
    {
        if (dock.ActiveDockable is not null && FindRoot(dock.ActiveDockable, x => x.IsFocusableRoot) is { } root)
        {
            if (dockable is not null)
            {
                var results = Find(x => x is IRootDock);

                foreach (var result in results)
                {
                    if (result is IRootDock rootDock 
                        && rootDock.IsFocusableRoot
                        && rootDock != root)
                    {
                        if (rootDock.FocusedDockable?.Owner is not null)
                        {
                            SetIsActive(rootDock.FocusedDockable.Owner, false);
                        }
                    }
                }
            }

            if (root.FocusedDockable?.Owner is not null)
            {
                SetIsActive(root.FocusedDockable.Owner, false);
            }

            if (dockable is not null)
            {
                if (root.FocusedDockable != dockable)
                {
                    root.FocusedDockable = dockable;
                }
            }

            if (root.FocusedDockable?.Owner is not null)
            {
                SetIsActive(root.FocusedDockable.Owner, true);
            }
        }
    }
}
