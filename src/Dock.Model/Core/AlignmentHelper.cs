using System;
using System.Collections.Generic;
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Helper methods for working with alignment based lists in <see cref="IRootDock"/>.
/// </summary>
internal static class AlignmentHelper
{
    /// <summary>
    /// Returns the list associated with the specified alignment.
    /// </summary>
    /// <param name="root">The root dock.</param>
    /// <param name="alignment">The alignment.</param>
    /// <returns>The list matching the alignment or <c>null</c> if none exists.</returns>
    public static IList<IDockable>? GetAlignmentList(IRootDock root, Alignment alignment)
    {
        return alignment switch
        {
            Alignment.Unset or Alignment.Left => root.LeftPinnedDockables,
            Alignment.Right => root.RightPinnedDockables,
            Alignment.Top => root.TopPinnedDockables,
            Alignment.Bottom => root.BottomPinnedDockables,
            _ => null,
        };
    }

    /// <summary>
    /// Adds a dockable to the list associated with the specified alignment.
    /// </summary>
    /// <param name="root">The root dock.</param>
    /// <param name="alignment">The alignment.</param>
    /// <param name="dockable">The dockable.</param>
    public static void AddToAlignmentList(IRootDock root, Alignment alignment, IDockable dockable)
    {
        var list = alignment switch
        {
            Alignment.Unset or Alignment.Left => root.LeftPinnedDockables ??= new List<IDockable>(),
            Alignment.Right => root.RightPinnedDockables ??= new List<IDockable>(),
            Alignment.Top => root.TopPinnedDockables ??= new List<IDockable>(),
            Alignment.Bottom => root.BottomPinnedDockables ??= new List<IDockable>(),
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null),
        };

        list.Add(dockable);
    }

    /// <summary>
    /// Removes a dockable from the list associated with the specified alignment.
    /// </summary>
    /// <param name="root">The root dock.</param>
    /// <param name="alignment">The alignment.</param>
    /// <param name="dockable">The dockable.</param>
    public static void RemoveFromAlignmentList(IRootDock root, Alignment alignment, IDockable dockable)
    {
        var list = GetAlignmentList(root, alignment);
        list?.Remove(dockable);
    }
}

