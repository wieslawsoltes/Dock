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
    /// Both source and target must be in the same "group state":
    /// - Both have null/empty groups (unrestricted docking)
    /// - Both have the same non-null group (restricted docking within group)
    /// Mixed states (one grouped, one ungrouped) are rejected to prevent contamination.
    /// This method considers group inheritance through the dock hierarchy.
    /// </summary>
    /// <param name="sourceDockable">The source dockable being dragged.</param>
    /// <param name="targetDockable">The target dockable or dock.</param>
    /// <returns>True if the docking groups are compatible; otherwise false.</returns>
    public static bool ValidateDockingGroups(IDockable sourceDockable, IDockable targetDockable)
    {
        var sourceGroup = GetEffectiveDockGroup(sourceDockable);
        var targetGroup = GetEffectiveDockGroup(targetDockable);

        // Both must be in the same "group state"
        var sourceHasGroup = !string.IsNullOrEmpty(sourceGroup);
        var targetHasGroup = !string.IsNullOrEmpty(targetGroup);

        // If both have no group, allow unrestricted docking
        if (!sourceHasGroup && !targetHasGroup)
        {
            return true;
        }

        // If both have groups, they must match exactly
        if (sourceHasGroup && targetHasGroup)
        {
            return string.Equals(sourceGroup, targetGroup, StringComparison.Ordinal);
        }

        // Mixed states (one grouped, one ungrouped) are not allowed
        return false;
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