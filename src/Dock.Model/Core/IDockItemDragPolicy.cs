// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optional factory policy for tab and pinned-item pointer dragging.</summary>
/// <remarks>Factories without this policy retain the default item drag behavior. Direct factory operations are unaffected.</remarks>
public interface IDockItemDragPolicy
{
    /// <summary>Determines whether a pointer gesture can drag this item.</summary>
    /// <param name="dockable">The item represented by the pressed tab or pin.</param>
    /// <returns>True to allow dragging within or outside its current strip.</returns>
    bool CanDragItem(IDockable dockable);

    /// <summary>Determines whether dragging can reorder this item within its strip.</summary>
    /// <param name="dockable">The item being dragged.</param>
    /// <returns>True to allow reordering; false still permits dragging outside the strip when dragging is allowed.</returns>
    bool CanReorderItem(IDockable dockable);
}
