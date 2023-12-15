/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Dock.Model.Controls;

namespace Dock.Model.Core;

/// <summary>
/// Docking manager contract.
/// </summary>
public interface IDockManager
{
    /// <summary>
    /// Gets or sets pointer position relative to event source.
    /// </summary>
    DockPoint Position { get; set; }

    /// <summary>
    /// Gets or sets pointer position relative to event source and translated to screen coordinates.
    /// </summary>
    DockPoint ScreenPosition { get; set; }

    /// <summary>
    /// Validates tool docking operation.
    /// </summary>
    /// <param name="sourceTool">The source tool.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates document docking operation.
    /// </summary>
    /// <param name="sourceDocument">The source document.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates dock docking operation.
    /// </summary>
    /// <param name="sourceDock">The source dock.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);

    /// <summary>
    /// Validates dockable docking operation.
    /// </summary>
    /// <param name="sourceDockable">The source dockable.</param>
    /// <param name="targetDockable">The target dockable.</param>
    /// <param name="action">The drag action.</param>
    /// <param name="operation">The dock operation.</param>
    /// <param name="bExecute">The flag indicating whether to execute.</param>
    /// <returns>True if docking operation can be executed otherwise false.</returns>
    bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute);
}
