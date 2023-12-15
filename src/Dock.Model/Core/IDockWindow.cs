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
/// Dock window contract.
/// </summary>
public interface IDockWindow
{
    /// <summary>
    /// Gets or sets id.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Gets or sets window X coordinate.
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// Gets or sets window X coordinate.
    /// </summary>
    double Y { get; set; }

    /// <summary>
    /// Gets or sets window width.
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Gets or sets window height.
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Gets or sets whether this window appears on top of all other windows.
    /// </summary>
    bool Topmost { get; set; }

    /// <summary>
    /// Gets or sets window title.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets or sets window owner dockable.
    /// </summary>
    IDockable? Owner { get; set; }

    /// <summary>
    /// Gets or sets dock factory.
    /// </summary>
    IFactory? Factory { get; set; }

    /// <summary>
    /// Gets or sets layout.
    /// </summary>
    IRootDock? Layout { get; set; }

    /// <summary>
    /// Gets or sets dock window.
    /// </summary>
    IHostWindow? Host { get; set; }

    /// <summary>
    /// Called when the window is closed.
    /// </summary>
    /// <returns>True to accept the close, and false to cancel the close.</returns>
    bool OnClose();

    /// <summary>
    /// Called before the window dragging start.
    /// </summary>
    /// <returns>True to accept the dragging, and false to cancel the dragging.</returns>
    bool OnMoveDragBegin();

    /// <summary>
    /// Called when the window is dragged.
    /// </summary>
    void OnMoveDrag();

    /// <summary>
    /// Called after the window dragging ended.
    /// </summary>
    void OnMoveDragEnd();

    /// <summary>
    /// Saves window properties.
    /// </summary>
    void Save();

    /// <summary>
    /// Presents window.
    /// </summary>
    /// <param name="isDialog">The value that indicates whether window is dialog.</param>
    void Present(bool isDialog);

    /// <summary>
    /// Exits window.
    /// </summary>
    void Exit();
}
