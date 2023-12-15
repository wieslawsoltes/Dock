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

using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.Core;

namespace Dock.Model.Controls;

/// <summary>
/// Root dock contract.
/// </summary>
public interface IRootDock : IDock
{
    /// <summary>
    /// Gets or sets if root dock is focusable.
    /// </summary>
    bool IsFocusableRoot { get; set; }

    /// <summary>
    /// Gets or sets hidden dockables.
    /// </summary>
    IList<IDockable>? HiddenDockables { get; set; }

    /// <summary>
    /// Gets or sets left pinned dockables.
    /// </summary>
    IList<IDockable>? LeftPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets right pinned dockables.
    /// </summary>
    IList<IDockable>? RightPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets top pinned dockables.
    /// </summary>
    IList<IDockable>? TopPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets bottom pinned dockables.
    /// </summary>
    IList<IDockable>? BottomPinnedDockables { get; set; }

    /// <summary>
    /// Gets or sets owner window.
    /// </summary>
    IDockWindow? Window { get; set; }

    /// <summary>
    /// Gets or sets windows.
    /// </summary>
    IList<IDockWindow>? Windows { get; set; }

    /// <summary>
    /// Show windows.
    /// </summary>
    ICommand ShowWindows { get; }

    /// <summary>
    /// Exit windows.
    /// </summary>
    ICommand ExitWindows { get; }
}
