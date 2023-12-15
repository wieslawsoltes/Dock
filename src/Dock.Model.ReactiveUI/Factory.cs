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
using System.Collections.ObjectModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;

namespace Dock.Model.ReactiveUI;

/// <summary>
/// Factory.
/// </summary>
public class Factory : FactoryBase
{
    /// <summary>
    /// Initializes the new instance of <see cref="Factory"/> class.
    /// </summary>
    public Factory()
    {
        VisibleDockableControls = new Dictionary<IDockable, IDockableControl>();
        PinnedDockableControls = new Dictionary<IDockable, IDockableControl>();
        TabDockableControls = new Dictionary<IDockable, IDockableControl>();
        DockControls = new ObservableCollection<IDockControl>();
        HostWindows = new ObservableCollection<IHostWindow>();
    }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

    /// <inheritdoc/>
    public override IList<IDockControl> DockControls { get; }

    /// <inheritdoc/>
    public override IList<IHostWindow> HostWindows { get; }

    /// <inheritdoc/>
    public override IList<T> CreateList<T>(params T[] items) => new ObservableCollection<T>(items);

    /// <inheritdoc/>
    public override IRootDock CreateRootDock() => new RootDock();

    /// <inheritdoc/>
    public override IProportionalDock CreateProportionalDock() => new ProportionalDock();

    /// <inheritdoc/>
    public override IDockDock CreateDockDock() => new DockDock();

    /// <inheritdoc/>
    public override IProportionalDockSplitter CreateProportionalDockSplitter() => new ProportionalDockSplitter();

    /// <inheritdoc/>
    public override IToolDock CreateToolDock() => new ToolDock();

    /// <inheritdoc/>
    public override IDocumentDock CreateDocumentDock() => new DocumentDock();

    /// <inheritdoc/>
    public override IDockWindow CreateDockWindow() => new DockWindow();

    /// <inheritdoc/>
    public override IRootDock CreateLayout() => CreateRootDock();
}
