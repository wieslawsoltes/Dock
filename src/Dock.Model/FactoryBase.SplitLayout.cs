// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Runtime.CompilerServices;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

public abstract partial class FactoryBase
{
    // Only layouts created by this factory are structural wrappers. Application
    // regions and layouts restored by a new factory retain their identity.
    private readonly ConditionalWeakTable<IDock, object> _generatedSplitLayouts = new();

    private IDock NormalizeRootSplitTarget(IDock target)
    {
        // Keep non-collapsable application regions intact, including their orientation.
        // A generated wrapper with a single child no longer describes a split.
        while (target is IProportionalDock { IsCollapsable: true, VisibleDockables.Count: 1 }
               && _generatedSplitLayouts.TryGetValue(target, out _)
               && target.VisibleDockables[0] is IDock child
               && target.Owner is IRootDock { VisibleDockables: { } siblings } root
               && CanUnwrapRootSplit(target, child, root))
        {
            int index = siblings.IndexOf(target);
            if (index < 0)
            {
                break;
            }

            bool wasActive = root.ActiveDockable == target;
            bool wasDefault = root.DefaultDockable == target;
            child.Proportion = target.Proportion;
            child.CollapsedProportion = target.CollapsedProportion;
            RemoveVisibleDockableAt(target, 0);
            target.ActiveDockable = null;
            target.DefaultDockable = null;
            RemoveVisibleDockableAt(root, index);
            InsertVisibleDockable(root, index, child);
            InitDockable(child, root);
            if (wasActive)
            {
                root.ActiveDockable = child;
            }
            if (wasDefault)
            {
                root.DefaultDockable = child;
            }
            _generatedSplitLayouts.Remove(target);
            target.Owner = null;
            OnDockableRemoved(target);
            target = child;
        }

        return target;
    }

    private static bool CanUnwrapRootSplit(IDock target, IDock child, IRootDock root)
    {
        string? childGroup = string.IsNullOrEmpty(child.DockGroup)
            ? DockGroupValidator.GetEffectiveDockGroup(root)
            : child.DockGroup;
        // Generated wrappers have no policy of their own. Keep application regions
        // that define policies, size constraints, or a group boundary, even with one pane.
        // CreateSplitLayout copies a child's group to its wrapper; that copy is redundant.
        return target.DockCapabilityPolicy is null
            && target.DockCapabilityOverrides is null
            && target.EnableGlobalDocking
            && target.CanDrag
            && target.CanDrop
            && (target is not IDockableDockingRestrictions restrictions
                || (restrictions.AllowedDockOperations == DockOperationMask.All
                    && restrictions.AllowedDropOperations == DockOperationMask.All))
            && double.IsNaN(target.MinWidth) && double.IsNaN(target.MaxWidth)
            && double.IsNaN(target.MinHeight) && double.IsNaN(target.MaxHeight)
            && string.Equals(DockGroupValidator.GetEffectiveDockGroup(target), childGroup, StringComparison.Ordinal);
    }

    private bool TrySplitIntoTarget(IDock target, IDockable dockable, DockOperation operation)
    {
        Orientation orientation = operation is DockOperation.Left or DockOperation.Right
            ? Orientation.Horizontal : Orientation.Vertical;
        if (target is not IProportionalDock proportionalTarget
            || proportionalTarget.Orientation != orientation
            || proportionalTarget.VisibleDockables is not { Count: > 0 } children)
        {
            return false;
        }

        CleanupOrphanedSplitters(proportionalTarget);
        if (children.Count == 0)
        {
            return false;
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
