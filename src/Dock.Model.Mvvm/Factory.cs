﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;

namespace Dock.Model.Mvvm;

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
        VisibleRootControls = new Dictionary<IDockable, object>();
        PinnedRootControls = new Dictionary<IDockable, object>();
        TabRootControls = new Dictionary<IDockable, object>();
        ToolControls = new Dictionary<IDockable, object>();
        DocumentControls = new Dictionary<IDockable, object>();
        DockControls = new ObservableCollection<IDockControl>();
        HostWindows = new ObservableCollection<IHostWindow>();
    }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, object> VisibleRootControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, object> PinnedRootControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, object> TabRootControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, object> ToolControls { get; }

    /// <inheritdoc/>
    public override IDictionary<IDockable, object> DocumentControls { get; }

    /// <inheritdoc/>
    public override IList<IDockControl> DockControls { get; }

    /// <inheritdoc/>
    public override IList<IHostWindow> HostWindows { get; }

    /// <inheritdoc/>
    public override IList<T> CreateList<T>(params T[] items) => new ObservableCollection<T>(items);

    /// <inheritdoc/>
    public override IRootDock CreateRootDock() => new RootDock
    {
        LeftPinnedDockables = CreateList<IDockable>(),
        RightPinnedDockables = CreateList<IDockable>(),
        TopPinnedDockables = CreateList<IDockable>(),
        BottomPinnedDockables = CreateList<IDockable>()
    };

    /// <inheritdoc/>
    public override IProportionalDock CreateProportionalDock() => new ProportionalDock();

    /// <inheritdoc/>
    public override IDockDock CreateDockDock() => new DockDock();

    /// <inheritdoc/>
    public override IStackDock CreateStackDock() => new StackDock();

    /// <inheritdoc/>
    public override IGridDock CreateGridDock() => new GridDock();

    /// <inheritdoc/>
    public override IWrapDock CreateWrapDock() => new WrapDock();

    /// <inheritdoc/>
    public override IUniformGridDock CreateUniformGridDock() => new UniformGridDock();

    /// <inheritdoc/>
    public override IProportionalDockSplitter CreateProportionalDockSplitter() => new ProportionalDockSplitter();

    /// <inheritdoc/>
    public override IGridDockSplitter CreateGridDockSplitter() => new GridDockSplitter();

    /// <inheritdoc/>
    public override IToolDock CreateToolDock() => new ToolDock();

    /// <inheritdoc/>
    public override IDocumentDock CreateDocumentDock() => new DocumentDock();

    /// <inheritdoc/>
    public override IDockWindow CreateDockWindow() => new DockWindow();

    /// <inheritdoc/>
    public override IRootDock CreateLayout() => CreateRootDock();
}
