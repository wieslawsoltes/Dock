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
/// 
/// </summary>
public interface IControlRecycling
{
    /// <summary>
    /// 
    /// </summary>
    public bool TryToUseIdAsKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    /// <returns></returns>
    bool TryGetValue(object? data, out object? control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="control"></param>
    void Add(object data, object control);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="existing"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    object? Build(object? data, object? existing, object? parent);

    /// <summary>
    /// 
    /// </summary>
    void Clear();
}
