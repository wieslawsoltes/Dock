// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia;

/// <summary>
/// Factory.
/// </summary>
public class Factory : FactoryBase
{
    private readonly Func<IRootDock>? _createLayoutFunc;

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

    /// <summary>
    /// Initializes the new instance of <see cref="Factory"/> class with a layout creation function.
    /// </summary>
    /// <param name="createLayoutFunc">Function to create the root dock layout.</param>
    public Factory(Func<IRootDock> createLayoutFunc) : this()
    {
        _createLayoutFunc = createLayoutFunc ?? throw new ArgumentNullException(nameof(createLayoutFunc));
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, object> VisibleRootControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, object> PinnedRootControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, object> TabRootControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, object> ToolControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, object> DocumentControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IList<IDockControl> DockControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IList<IHostWindow> HostWindows { get; }

    /// <inheritdoc/>
    public override IList<T> CreateList<T>(params T[] items) => new AvaloniaList<T>(items);

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
    public override IRootDock CreateLayout() => _createLayoutFunc?.Invoke() ?? CreateRootDock();
}
