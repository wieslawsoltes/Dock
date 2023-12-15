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
using System;

namespace Dock.Model.Core;

/// <summary>
/// Dock control contract.
/// </summary>
public interface IDockControl
{
    /// <summary>
    /// Gets dock manager.
    /// </summary>
    IDockManager DockManager { get; }

    /// <summary>
    /// Gets dock control state.
    /// </summary>
    IDockControlState DockControlState { get; }

    /// <summary>
    /// Gets or sets the dock layout.
    /// </summary>
    IDock? Layout { get; set; }

    /// <summary>
    /// Gets or sets default context.
    /// </summary>
    object? DefaultContext { get; set; }

    /// <summary>
    /// Gets or sets the flag indicating whether to initialize layout.
    /// </summary>
    bool InitializeLayout { get; set; }

    /// <summary>
    /// Gets or sets the flag indicating whether to initialize factory.
    /// </summary>
    bool InitializeFactory { get; set; }

    /// <summary>
    /// Gets or sets the factory.
    /// </summary>
    IFactory? Factory { get; set; }
}
