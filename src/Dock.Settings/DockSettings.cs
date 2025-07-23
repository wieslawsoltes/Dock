// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;

namespace Dock.Settings;

/// <summary>
/// Dock settings.
/// </summary>
public static class DockSettings
{
    /// <summary>
    /// Minimum horizontal drag distance to initiate drag operation.
    /// </summary>
    public static double MinimumHorizontalDragDistance = 4;

    /// <summary>
    /// Minimum vertical drag distance to initiate drag operation.
    /// </summary>
    public static double MinimumVerticalDragDistance = 4;

    /// <summary>
    /// Show dock adorners using floating transparent window.
    /// </summary>
    public static bool UseFloatingDockAdorner = false;

    /// <summary>
    /// Show auto-hidden dockables inside a floating window.
    /// </summary>
    public static bool UsePinnedDockWindow = false;

    /// <summary>
    /// Allow docking between different dock control instances.
    /// </summary>
    public static bool EnableGlobalDocking = true;
    
    /// <summary>
    /// Floating windows use the main window as their owner so they stay in front.
    /// </summary>
    public static bool UseOwnerForFloatingWindows = true;

    /// <summary>
    /// Snap floating windows to nearby windows when dragging.
    /// </summary>
    public static bool EnableWindowMagnetism = false;

    /// <summary>
    /// Distance in pixels within which windows snap together.
    /// </summary>
    public static double WindowMagnetDistance = 16;

    /// <summary>
    /// Bring all windows to the front when a window starts being dragged.
    /// </summary>
    public static bool BringWindowsToFrontOnDrag = true;


    /// <summary>
    /// Checks if the drag distance is greater than the minimum required distance to initiate a drag operation.
    /// </summary>
    /// <param name="diff">The drag delta.</param>
    /// <returns>True if drag delta is above minimum drag distance threshold.</returns>
    public static bool IsMinimumDragDistance(Vector diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    /// <summary>
    /// Checks if the drag distance is greater than the minimum required distance to initiate a drag operation.
    /// </summary>
    /// <param name="diff">The drag delta.</param>
    /// <returns>True if drag delta is above minimum drag distance threshold.</returns>
    public static bool IsMinimumDragDistance(PixelPoint diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }
}

