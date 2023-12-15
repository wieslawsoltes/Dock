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
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    /// <inheritdoc/>
    public abstract IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <inheritdoc/>
    public abstract IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <inheritdoc/>
    public abstract IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

    /// <inheritdoc/>
    public abstract IList<IDockControl> DockControls { get; }

    /// <inheritdoc/>
    public abstract IList<IHostWindow> HostWindows { get; }

    /// <inheritdoc/>
    public abstract IList<T> CreateList<T>(params T[] items);

    /// <inheritdoc/>
    public abstract IRootDock CreateRootDock();

    /// <inheritdoc/>
    public abstract IProportionalDock CreateProportionalDock();

    /// <inheritdoc/>
    public abstract IDockDock CreateDockDock();

    /// <inheritdoc/>
    public abstract IProportionalDockSplitter CreateProportionalDockSplitter();

    /// <inheritdoc/>
    public abstract IToolDock CreateToolDock();

    /// <inheritdoc/>
    public abstract IDocumentDock CreateDocumentDock();

    /// <inheritdoc/>
    public abstract IDockWindow CreateDockWindow();

    /// <inheritdoc/>
    public abstract IRootDock? CreateLayout();
}
