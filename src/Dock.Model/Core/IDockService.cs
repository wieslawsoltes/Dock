// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Dock.Model.Core;

/// <summary>
/// Dock service contract.
/// </summary>
public interface IDockService
{
    /// <summary>
    /// Moves dockable to the target dock.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="sourceDockableOwner">The owner of the source dockable.</param>
    /// <param name="targetDock">The target dock.</param>
    /// <param name="bExecute">True to execute the move.</param>
    /// <returns>True if the operation is valid.</returns>
    bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute);

    /// <summary>
    /// Swaps dockable with target dock.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="sourceDockableOwner">The owner of the source dockable.</param>
    /// <param name="targetDock">The target dock.</param>
    /// <param name="bExecute">True to execute the swap.</param>
    /// <returns>True if the operation is valid.</returns>
    bool SwapDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute);

    /// <summary>
    /// Splits dockable using the given operation.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="sourceDockableOwner">The owner of the source dockable.</param>
    /// <param name="targetDock">The target dock.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">True to execute the split.</param>
    /// <returns>True if the operation is valid.</returns>
    bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute);

    /// <summary>
    /// Docks the dockable into a window.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="screenPosition">The pointer screen position.</param>
    /// <param name="bExecute">True to execute the operation.</param>
    /// <returns>True if the operation is valid.</returns>
    bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DockPoint screenPosition, bool bExecute);

    /// <summary>
    /// Docks a dockable into another dockable.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="bExecute">True to execute the operation.</param>
    /// <returns>True if the operation is valid.</returns>
    bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, bool bExecute);
}
