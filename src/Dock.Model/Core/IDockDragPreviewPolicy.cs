// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>Optional factory policy for showing dockable content while dragging.</summary>
/// <remarks>Factories without this policy use the application's drag preview setting.
/// This policy affects presentation only; docking permissions and operations are unchanged.</remarks>
public interface IDockDragPreviewPolicy
{
    /// <summary>Determines whether the drag preview includes the dockable's content.</summary>
    /// <param name="dockable">The item or pane represented by the drag preview.</param>
    /// <returns>True to show full content; false to show only the drag caption and status.</returns>
    bool ShowDockablePreviewOnDrag(IDockable dockable);
}
