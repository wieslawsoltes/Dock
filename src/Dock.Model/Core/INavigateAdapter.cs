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
/// Navigate adapter contract for the <see cref="IDock"/>.
/// </summary>
public interface INavigateAdapter
{
    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in back navigation history.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets a value that indicates whether there is at least one entry in forward navigation history.
    /// </summary>
    bool CanGoForward { get; }

    /// <summary>
    /// Navigates to the most recent entry in back navigation history, if there is one.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Navigate to the most recent entry in forward navigation history, if there is one.
    /// </summary>
    void GoForward();

    /// <summary>
    /// Implementation of the <see cref="IDock.Navigate"/> method.
    /// </summary>
    /// <param name="root">An object that contains the content to navigate to.</param>
    /// <param name="bSnapshot">The flag indicating whether to make snapshot.</param>
    void Navigate(object? root, bool bSnapshot);

    /// <summary>
    /// Implementation of the <see cref="IRootDock.ShowWindows"/> method.
    /// </summary>
    void ShowWindows();

    /// <summary>
    /// Implementation of the <see cref="IRootDock.ExitWindows"/> method.
    /// </summary>
    void ExitWindows();

    /// <summary>
    /// Implementation of the <see cref="IDock.Close"/> method.
    /// </summary>
    void Close();
}
