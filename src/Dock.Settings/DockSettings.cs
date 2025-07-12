// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;
using Dock.Model.Core;

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
    /// Type of the Ctrl+Tab document switcher.
    /// </summary>
    public static DocumentSwitcherType DocumentSwitcherType = DocumentSwitcherType.Simple;

    /// <summary>
    /// Key gesture that triggers the document switcher.
    /// </summary>
    public static KeyGesture DocumentSwitcherGesture =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.Tab, KeyModifiers.Control)
            : new KeyGesture(Key.Tab, KeyModifiers.Control);

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
