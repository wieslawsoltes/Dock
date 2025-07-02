﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
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
    public abstract IDictionary<IDockable, double> DockableProportions { get; }

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
