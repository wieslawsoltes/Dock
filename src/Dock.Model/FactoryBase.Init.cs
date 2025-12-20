// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

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
    public virtual void InitLayout(IDockable layout)
    {
        // Set up default HostWindowLocator if not already defined
        if (HostWindowLocator is null && DefaultHostWindowLocator is not null)
        {
            HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
            {
                [nameof(IDockWindow)] = DefaultHostWindowLocator
            };
        }

        InitDockable(layout, null);

        if (layout is IDock dock)
        {
            if (dock.DefaultDockable is not null)
            {
                dock.ActiveDockable = dock.DefaultDockable;
            }
        }

        // Auto-set ActiveDockable for RootDock if not already set
        if (layout is IRootDock rootDock)
        {
            if (rootDock.ActiveDockable is null && rootDock.VisibleDockables is not null)
            {
                // Find first visible dockable that is not a splitter
                var firstVisible = FindFirstVisibleDockable(rootDock);
                if (firstVisible is not null)
                {
                    rootDock.ActiveDockable = firstVisible;
                }
            }

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
            }

            UpdateIsEmpty(dock);
        }

        if (dockable is ISplitViewDock splitViewDock)
        {
            InitSplitViewDockables(splitViewDock);
        }

        if (dockable is IRootDock rootDock)
        {
            if (rootDock.HiddenDockables is not null)
            {
                InitDockables(dockable, rootDock.HiddenDockables);
            }

            if (rootDock.LeftPinnedDockables is not null)
            {
                InitPinnedDockables(rootDock.LeftPinnedDockables);
            }

            if (rootDock.RightPinnedDockables is not null)
            {
                InitPinnedDockables(rootDock.RightPinnedDockables);
            }

            if (rootDock.TopPinnedDockables is not null)
            {
                InitPinnedDockables(rootDock.TopPinnedDockables);
            }

            if (rootDock.BottomPinnedDockables is not null)
            {
                InitPinnedDockables(rootDock.BottomPinnedDockables);
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

    /// <summary>
    /// Finds the first visible dockable in the hierarchy that is not a splitter.
    /// </summary>
    /// <param name="dock">The dock to search in.</param>
    /// <returns>The first visible dockable or null if none found.</returns>
    private IDockable? FindFirstVisibleDockable(IDock dock)
    {
        if (dock.VisibleDockables is null)
        {
            return null;
        }

        // First look for direct visible dockables that are not splitters
        foreach (var dockable in dock.VisibleDockables)
        {
            if (dockable is not ISplitter)
            {
                return dockable;
            }
        }

        // If no direct dockables found, recursively search in child docks
        foreach (var dockable in dock.VisibleDockables)
        {
            if (dockable is IDock childDock)
            {
                var result = FindFirstVisibleDockable(childDock);
                if (result is not null)
                {
                    return result;
                }
            }
        }

        return null;
    }

    private void InitDockables(IDockable dockable, IList<IDockable> dockables)
    {
        foreach (var child in dockables)
        {
            InitDockable(child, dockable);
        }
    }

    private void InitSplitViewDockables(ISplitViewDock splitViewDock)
    {
        var dock = (IDock)splitViewDock;
        var paneDockable = splitViewDock.PaneDockable;
        if (paneDockable is not null && dock.VisibleDockables?.Contains(paneDockable) != true)
        {
            InitDockable(paneDockable, splitViewDock);
        }

        var contentDockable = splitViewDock.ContentDockable;
        if (contentDockable is not null &&
            !ReferenceEquals(contentDockable, paneDockable) &&
            dock.VisibleDockables?.Contains(contentDockable) != true)
        {
            InitDockable(contentDockable, splitViewDock);
        }
    }

    private void InitPinnedDockables(IList<IDockable> dockables)
    {
        foreach (var child in dockables)
        {
            InitDockable(child, child.Owner);
        }
    }

    /// <inheritdoc/>
    public virtual void InitDockWindow(IDockWindow window, IDockable? owner)
    {
        InitDockWindow(window, owner, GetHostWindow(window.Id));
    }

    /// <inheritdoc/>
    public virtual void InitDockWindow(IDockWindow window, IDockable? owner, IHostWindow? hostWindow)
    {
        window.Host = hostWindow;
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
    public void ActivateWindow(IDockable dockable)
    {
        var root = FindRoot(dockable);

        if (root is { Window: not null })
        {
            root.Window.SetActive();
            OnWindowActivated(root.Window);
        }
    }
    
    
    /// <inheritdoc/>
    public virtual void SetActiveDockable(IDockable dockable)
    {
        if (dockable.Owner is IDock dock)
        {
            dock.ActiveDockable = dockable;
            OnDockableActivated(dockable);
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
                            // Trigger deactivation event for the dockable that lost focus
                            OnDockableDeactivated(rootDock.FocusedDockable);
                        }
                    }
                }
            }

            if (root.FocusedDockable?.Owner is not null)
            {
                SetIsActive(root.FocusedDockable.Owner, false);
                // Trigger deactivation event for the dockable that lost focus
                OnDockableDeactivated(root.FocusedDockable);
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
