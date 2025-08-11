// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Helper class for computing inherited dock properties through the ownership hierarchy.
/// Provides utilities for determining effective property values that cascade down the dock tree.
/// </summary>
public static class DockInheritanceHelper
{
    /// <summary>
    /// Computes the effective EnableGlobalDocking setting for the given dock by walking up
    /// the ownership chain. If any ancestor dock has EnableGlobalDocking set to false,
    /// the effective value is false. This allows disabling global docking for an entire subtree.
    /// </summary>
    /// <param name="dockable">The dock or dockable to evaluate.</param>
    /// <returns>True if global docking is effectively enabled; otherwise false.</returns>
    public static bool GetEffectiveEnableGlobalDocking(IDockable dockable)
    {
        var current = dockable;
        while (current != null)
        {
            if (current is IDock dock && !dock.EnableGlobalDocking)
            {
                return false;
            }
            current = current.Owner;
        }
        return true;
    }
}