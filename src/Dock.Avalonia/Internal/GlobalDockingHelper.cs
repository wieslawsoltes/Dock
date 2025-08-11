// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Helper class for determining when global docking operations make sense
/// </summary>
internal static class GlobalDockingHelper
{
    /// <summary>
    /// Determines if global docking should be available for the specified target dock
    /// </summary>
    /// <param name="targetDock">The target dock to evaluate</param>
    /// <returns>A tuple indicating whether global docking makes sense horizontally and vertically</returns>
    public static (bool horizontalValid, bool verticalValid) CanGlobalDock(IDock targetDock)
    {
        int horizontalCount = 0;
        int verticalCount = 0;
        
        CountProportionalDockChildren(targetDock, ref horizontalCount, ref verticalCount);
        
        return (verticalCount >= 2, horizontalCount >= 2);
    }
    
    /// <summary>
    /// Recursively counts children in horizontal and vertical proportional docks
    /// </summary>
    /// <param name="dock">The dock to analyze</param>
    /// <param name="horizontalCount">Count of items in horizontal proportional docks</param>
    /// <param name="verticalCount">Count of items in vertical proportional docks</param>
    private static void CountProportionalDockChildren(IDock dock, ref int horizontalCount, ref int verticalCount)
    {
        // Early exit if we already have enough counts
        if (horizontalCount >= 2 && verticalCount >= 2)
        {
            return;
        }
        
        if (dock is IProportionalDock proportionalDock && proportionalDock.VisibleDockables != null)
        {
            // Count only non-proportional dock children
            int nonProportionalChildren = 0;
            foreach (var dockable in proportionalDock.VisibleDockables)
            {
                if (dockable is not IProportionalDock)
                {
                    nonProportionalChildren++;
                }
            }
            
            // Add the count to the appropriate orientation
            if (proportionalDock.Orientation == Orientation.Horizontal)
            {
                horizontalCount += nonProportionalChildren;
            }
            else // Vertical
            {
                verticalCount += nonProportionalChildren;
            }
            
            // Early exit if we've reached the threshold
            if (horizontalCount >= 2 && verticalCount >= 2)
            {
                return;
            }
            
            // Recursively check child proportional docks only
            foreach (var dockable in proportionalDock.VisibleDockables)
            {
                if (dockable is IProportionalDock childProportionalDock)
                {
                    CountProportionalDockChildren(childProportionalDock, ref horizontalCount, ref verticalCount);
                    
                    // Early exit if we've reached the threshold
                    if (horizontalCount >= 2 && verticalCount >= 2)
                    {
                        return;
                    }
                }
            }
        }
        else if (dock.VisibleDockables != null)
        {
            // For non-proportional docks, recursively check child proportional docks only
            foreach (var dockable in dock.VisibleDockables)
            {
                if (dockable is IProportionalDock childProportionalDock)
                {
                    CountProportionalDockChildren(childProportionalDock, ref horizontalCount, ref verticalCount);
                    
                    // Early exit if we've reached the threshold
                    if (horizontalCount >= 2 && verticalCount >= 2)
                    {
                        return;
                    }
                }
            }
        }
    }
}
