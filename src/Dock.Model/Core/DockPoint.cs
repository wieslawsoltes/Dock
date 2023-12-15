using System.Globalization;
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

namespace Dock.Model.Core;

/// <summary>
/// Point structure.
/// </summary>
public readonly struct DockPoint
{
    /// <summary>
    /// Gets X coordinate.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets Y coordinate.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Initialize the new instance of the <see cref="DockPoint"/>.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public DockPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Returns the string representation of the point.
    /// </summary>
    /// <returns>The string representation of the point.</returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}, {1}", X, Y);
    }
}
