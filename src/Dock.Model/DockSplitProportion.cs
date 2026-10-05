using System;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>Assigns a split's complete sibling distribution independently of layout timing.</summary>
public static class DockSplitProportion
{
    /// <summary>Sizes the inserted pane and rescales retained siblings, preserving their relative shares.</summary>
    /// <param name="insertedDock">The pane created by the split, rather than the drag surface's original owner.</param>
    /// <param name="proportion">The inserted pane's share of its proportional parent.</param>
    /// <returns>Whether the distribution was assigned.</returns>
    public static bool Apply(IDock insertedDock, double proportion)
    {
        if (!double.IsFinite(proportion) || proportion <= 0 || proportion >= 1
            || insertedDock.Owner is not IProportionalDock { VisibleDockables: { } siblings })
            return false;

        var assigned = 0.0;
        var unassigned = 0;
        var count = 0;
        var containsInserted = false;
        foreach (var sibling in siblings)
        {
            if (ReferenceEquals(sibling, insertedDock)) { containsInserted = true; continue; }
            if (sibling is IProportionalDockSplitter) continue;
            count++;
            if (double.IsFinite(sibling.Proportion) && sibling.Proportion > 0) assigned += sibling.Proportion;
            else unassigned++;
        }
        if (!containsInserted || count == 0) return false;

        if (insertedDock.Owner is IEqualProportionalDock { KeepProportionsEqual: true } equalDock)
        {
            Equalize(equalDock);
            return true;
        }

        // NaN siblings share the unused allocation, just as ProportionalStackPanel does.
        var inferred = unassigned > 0 ? Math.Max(0, 1 - assigned) / unassigned : 0;
        var total = assigned + inferred * unassigned;
        foreach (var sibling in siblings)
        {
            if (ReferenceEquals(sibling, insertedDock) || sibling is IProportionalDockSplitter) continue;
            var share = double.IsFinite(sibling.Proportion) && sibling.Proportion > 0 ? sibling.Proportion : inferred;
            var value = (1 - proportion) * (total > 0 ? share / total : 1.0 / count);
            sibling.Proportion = value;
            sibling.CollapsedProportion = value;
        }
        insertedDock.Proportion = proportion;
        insertedDock.CollapsedProportion = proportion;
        return true;
    }

    /// <summary>Rebalances an opted-in proportional dock after a content change, excluding splitters.</summary>
    /// <param name="dock">The parent whose sizing policy and visible panes are inspected.</param>
    public static void Equalize(IDock dock)
    {
        if (dock is not IEqualProportionalDock { KeepProportionsEqual: true, VisibleDockables: { } children } equal) return;
        var count = 0;
        foreach (var child in children)
            if (child is not IProportionalDockSplitter) count += Units(child, equal.Orientation);
        if (count == 0) return;
        foreach (var child in children)
        {
            if (child is IProportionalDockSplitter) continue;
            var share = (double)Units(child, equal.Orientation) / count;
            child.Proportion = share;
            child.CollapsedProportion = share;
        }
        if (dock.Owner is IDock owner) Equalize(owner);
    }

    private static int Units(IDockable child, Orientation axis)
    {
        if (child is not IProportionalDock { VisibleDockables: { } children } dock || dock.Orientation != axis) return 1;
        var count = 0;
        foreach (var item in children)
            if (item is not IProportionalDockSplitter) count += Units(item, axis);
        return System.Math.Max(1, count);
    }
}
