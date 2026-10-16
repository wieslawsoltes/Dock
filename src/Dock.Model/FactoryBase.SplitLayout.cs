// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

public abstract partial class FactoryBase
{
    private bool TrySplitIntoTarget(IDock target, IDockable dockable, DockOperation operation)
    {
        Orientation orientation = operation is DockOperation.Left or DockOperation.Right
            ? Orientation.Horizontal : Orientation.Vertical;
        if (target is not IProportionalDock proportionalTarget
            || proportionalTarget.VisibleDockables is not { Count: > 0 } children)
        {
            return false;
        }

        CleanupOrphanedSplitters(proportionalTarget);
        if (children.Count == 0)
        {
            return false;
        }

        if (proportionalTarget.Orientation != orientation)
        {
            if (!proportionalTarget.IsCollapsable || children.Count != 1)
            {
                return false;
            }

            // With one pane the axis has no visual effect. Reuse the region for
            // the next split, retaining its identity, policies, and constraints.
            proportionalTarget.Orientation = orientation;
        }

        IDock split;
        if (dockable is IDock dock)
        {
            split = dock;
        }
        else
        {
            split = CreateProportionalDock();
            split.Title = nameof(IProportionalDock);
            split.VisibleDockables = CreateList<IDockable>();
            CopyDockGroup(dockable, split);
            AddVisibleDockable(split, dockable);
            OnDockableAdded(dockable);
            split.ActiveDockable = dockable;
        }

        IProportionalDockSplitter splitter = CreateProportionalDockSplitter();
        splitter.Title = nameof(IProportionalDockSplitter);
        if (operation is DockOperation.Left or DockOperation.Top)
        {
            InsertDockable(proportionalTarget, splitter, 0);
            InsertDockable(proportionalTarget, split, 0);
        }
        else
        {
            AddDockable(proportionalTarget, splitter);
            AddDockable(proportionalTarget, split);
        }
        // Reserve half for the incoming dock while retaining existing child ratios.
        DockSplitProportion.Apply(split, 0.5);
        proportionalTarget.ActiveDockable = split;
        OnDockableUndocked(dockable, operation);
        OnDockableDocked(dockable, operation);
        return true;
    }
}
