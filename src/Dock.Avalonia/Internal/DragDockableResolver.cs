// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Resolves the model dockable represented by a drag surface.
/// </summary>
internal static class DragDockableResolver
{
    /// <summary>
    /// Resolves a pinned preview dock to its active dockable while preserving
    /// regular dock drags as whole-dock operations.
    /// </summary>
    /// <param name="dockable">The dockable supplied by the drag surface.</param>
    /// <returns>The dockable that should participate in the drag operation.</returns>
    public static IDockable Resolve(IDockable dockable)
    {
        if (dockable is IToolDock { Owner: IRootDock rootDock, ActiveDockable: { } activeDockable } previewDock
            && ReferenceEquals(rootDock.PinnedDock, previewDock))
        {
            return activeDockable;
        }

        return dockable;
    }

    /// <summary>
    /// Determines whether a drop targets the dragged dockable or its current owner.
    /// </summary>
    /// <param name="sourceDockable">The dockable supplied by the drag surface.</param>
    /// <param name="targetDockable">The proposed drop target.</param>
    /// <returns><c>true</c> when the operation is a self-drop; otherwise <c>false</c>.</returns>
    public static bool IsSelfDrop(IDockable sourceDockable, IDockable targetDockable)
    {
        var resolvedSource = Resolve(sourceDockable);
        return ReferenceEquals(resolvedSource, targetDockable)
               || ReferenceEquals(resolvedSource.Owner, targetDockable);
    }
}
