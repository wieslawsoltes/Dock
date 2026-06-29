using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace DockReactiveUIFlatSample.ViewModels;

internal static class DockingDefaults
{
    public static T EnableDocking<T>(T dockable) where T : IDockable
    {
        dockable.CanFloat = true;
        dockable.CanDrag = true;
        dockable.CanDrop = true;
        dockable.CanDockAsDocument = true;

        if (dockable is IDockableDockingRestrictions restrictions)
        {
            restrictions.AllowedDockOperations = DockOperationMask.All;
            restrictions.AllowedDropOperations = DockOperationMask.All;
        }

        if (dockable is IDock dock)
        {
            dock.EnableGlobalDocking = true;
        }

        return dockable;
    }

    public static T EnableDockingTree<T>(T dockable) where T : IDockable
    {
        EnableDockingTree(dockable, new HashSet<IDockable>());
        return dockable;
    }

    private static void EnableDockingTree(IDockable dockable, ISet<IDockable> visited)
    {
        if (!visited.Add(dockable))
        {
            return;
        }

        EnableDocking(dockable);

        if (dockable is IDock dock && dock.VisibleDockables is { } visibleDockables)
        {
            EnableDockingTree(visibleDockables, visited);
        }

        if (dockable is IDock dockWithSelection)
        {
            if (dockWithSelection.ActiveDockable is { } activeDockable)
            {
                EnableDockingTree(activeDockable, visited);
            }

            if (dockWithSelection.DefaultDockable is { } defaultDockable)
            {
                EnableDockingTree(defaultDockable, visited);
            }

            if (dockWithSelection.FocusedDockable is { } focusedDockable)
            {
                EnableDockingTree(focusedDockable, visited);
            }
        }

        if (dockable is ISplitViewDock splitViewDock)
        {
            if (splitViewDock.PaneDockable is { } paneDockable)
            {
                EnableDockingTree(paneDockable, visited);
            }

            if (splitViewDock.ContentDockable is { } contentDockable)
            {
                EnableDockingTree(contentDockable, visited);
            }
        }

        if (dockable is IRootDock rootDock)
        {
            if (rootDock.HiddenDockables is { } hiddenDockables)
            {
                EnableDockingTree(hiddenDockables, visited);
            }

            if (rootDock.LeftPinnedDockables is { } leftPinnedDockables)
            {
                EnableDockingTree(leftPinnedDockables, visited);
            }

            if (rootDock.RightPinnedDockables is { } rightPinnedDockables)
            {
                EnableDockingTree(rightPinnedDockables, visited);
            }

            if (rootDock.TopPinnedDockables is { } topPinnedDockables)
            {
                EnableDockingTree(topPinnedDockables, visited);
            }

            if (rootDock.BottomPinnedDockables is { } bottomPinnedDockables)
            {
                EnableDockingTree(bottomPinnedDockables, visited);
            }

            if (rootDock.PinnedDock is { } pinnedDock)
            {
                EnableDockingTree(pinnedDock, visited);
            }

            if (rootDock.Windows is { } windows)
            {
                foreach (var window in windows)
                {
                    if (window.Layout is { } windowLayout)
                    {
                        EnableDockingTree(windowLayout, visited);
                    }
                }
            }
        }
    }

    private static void EnableDockingTree(IEnumerable<IDockable> dockables, ISet<IDockable> visited)
    {
        foreach (var dockable in dockables)
        {
            EnableDockingTree(dockable, visited);
        }
    }
}
