// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

namespace Dock.Model.Core;

/// <summary>
/// Helper class for validating docking groups compatibility.
/// Provides shared validation logic used by DockService and DockManager.
/// </summary>
public static class DockGroupValidator
{
    /// <summary>
    /// Validates if two dockables can be docked together based on their docking groups.
    /// 
    /// Business Rules:
    /// - Non-grouped dockables can dock anywhere EXCEPT grouped dockables (prevents contamination)
    /// - Grouped dockables can only dock into same group (can't break out to global targets)
    /// - Different groups are incompatible
    /// 
    /// This method considers group inheritance through the dock hierarchy.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDockable">The target dockable or dock.</param>
    /// <returns>True if the docking groups are compatible; otherwise false.</returns>
    public static bool ValidateDockingGroups(IDockable sourceDockable, IDockable targetDockable)
    {
        var sourceGroup = GetEffectiveDockGroup(sourceDockable);
        var targetGroup = GetEffectiveDockGroup(targetDockable);

        // Correct docking group compatibility rules:
        // 1. Non-grouped can dock anywhere EXCEPT grouped dockables
        // 2. Grouped can dock into same group only (can't dock into global targets)
        // 3. Different groups are incompatible
        
        var sourceHasGroup = !string.IsNullOrEmpty(sourceGroup);
        var targetHasGroup = !string.IsNullOrEmpty(targetGroup);

        if (!sourceHasGroup && !targetHasGroup)
        {
            return true; // Both non-grouped - always compatible
        }

        if (!sourceHasGroup && targetHasGroup)
        {
            return false; // Non-grouped can't dock into grouped dockables
        }

        if (sourceHasGroup && !targetHasGroup)
        {
            return false; // Grouped can't dock into global targets
        }

        // Both are grouped - they must have the same group
        return string.Equals(sourceGroup, targetGroup, StringComparison.Ordinal);
    }

    /// <summary>
    /// Validates if a dockable can be docked into a dock based on docking groups.
    /// The source must be compatible with all existing dockables in the target dock.
    /// This method considers group inheritance through the dock hierarchy.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDock">The target dock.</param>
    /// <returns>True if the docking groups are compatible; otherwise false.</returns>
    public static bool ValidateDockingGroupsInDock(IDockable sourceDockable, IDock targetDock)
    {
        // If the target dock has no visible dockables, allow the operation
        if (targetDock.VisibleDockables?.Count == 0)
        {
            return true;
        }

        // Check compatibility with each existing dockable in the target dock
        if (targetDock.VisibleDockables != null)
        {
            foreach (var existingDockable in targetDock.VisibleDockables)
            {
                if (!ValidateDockingGroups(sourceDockable, existingDockable))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the effective docking group for a dockable, considering inheritance.
    /// If the dockable has its own group, that is used. Otherwise, walks up the
    /// ownership hierarchy to find an inherited group.
    /// </summary>
    /// <param name="dockable">The dockable to get the effective group for.</param>
    /// <returns>The effective docking group, or null if none is found.</returns>
    public static string? GetEffectiveDockGroup(IDockable dockable)
    {
        var current = dockable;
        
        // Walk up the hierarchy until we find a group or reach the root
        while (current != null)
        {
            if (!string.IsNullOrEmpty(current.DockGroup))
            {
                return current.DockGroup;
            }
            
            current = current.Owner;
        }
        
        return null;
    }
}