﻿using System.Collections.Generic;
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
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> VisibleDockableControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> PinnedDockableControls { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override IDictionary<IDockable, IDockableControl> TabDockableControls { get; }

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
    public override IProportionalDockSplitter CreateProportionalDockSplitter() => new ProportionalDockSplitter();

    /// <inheritdoc/>
    public override IPixelDock CreatePixelDock() => new PixelDock();

    /// <inheritdoc/>
    public override IPixelDockSplitter CreatePixelDockSplitter() => new PixelDockSplitter();

    /// <inheritdoc/>
    public override IToolDock CreateToolDock() => new ToolDock();

    /// <inheritdoc/>
    public override IDocumentDock CreateDocumentDock() => new DocumentDock();

    /// <inheritdoc/>
    public override IDockWindow CreateDockWindow() => new DockWindow();

    /// <inheritdoc/>
    public override IRootDock CreateLayout() => CreateRootDock();
}
