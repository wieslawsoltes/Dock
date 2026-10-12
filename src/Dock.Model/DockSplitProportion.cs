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
}
