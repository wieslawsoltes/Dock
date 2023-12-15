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
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public virtual void AddWindow(IRootDock rootDock, IDockWindow window)
    {
        rootDock.Windows ??= CreateList<IDockWindow>();
        rootDock.Windows.Add(window);
        OnWindowAdded(window);
        InitDockWindow(window, rootDock);
    }

    /// <inheritdoc/>
    public virtual void InsertWindow(IRootDock rootDock, IDockWindow window, int index)
    {
        if (index >= 0)
        {
            rootDock.Windows ??= CreateList<IDockWindow>();
            rootDock.Windows.Insert(index, window);
            OnWindowAdded(window);
            InitDockWindow(window, rootDock);
        }
    }

    /// <inheritdoc/>
    public virtual void RemoveWindow(IDockWindow window)
    {
        if (window.Owner is IRootDock rootDock)
        {
            window.Exit();
            rootDock.Windows?.Remove(window);
            OnWindowRemoved(window);
        }
    }
}
