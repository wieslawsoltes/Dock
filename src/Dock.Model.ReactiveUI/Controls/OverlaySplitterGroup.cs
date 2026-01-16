// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Dock.Model.ReactiveUI.Controls;

/// <summary>
/// Overlay splitter group.
/// </summary>
[DataContract(IsReference = true)]
public partial class OverlaySplitterGroup : DockableBase, IOverlaySplitterGroup
{
    private bool _isDragging;
    private IList<IOverlayPanel>? _panels;
    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

    internal IOverlayAdapter Overlay => Factory?.OverlayAdapter ?? s_overlayAdapter;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlayPanel>? Panels
    {
        get => _panels;
        set
        {
            var previous = _panels;
            this.RaiseAndSetIfChanged(ref _panels, value);

            if (Factory is { } factory)
            {
                factory.SetOverlaySplitterGroupPanels(this, previous, value);
            }
            else
            {
                Overlay.UpdateGroupPanels(this, previous, value);
            }
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlaySplitter>? Splitters
    {
        get => _splitters;
        set
        {
            this.RaiseAndSetIfChanged(ref _splitters, value);
            if (Factory is { } factory)
            {
                factory.SetOverlaySplitterGroupSplitters(this, value);
            }
            else
            {
                Overlay.UpdateGroupSplitters(this, value);
            }
        }
    }

    private IList<IOverlaySplitter>? _splitters;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial Orientation Orientation { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double X { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Y { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Width { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Height { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial int ZIndex { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayAnchor Anchor { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsPositionLocked { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => this.RaiseAndSetIfChanged(ref _isDragging, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial OverlayEdgeDock EdgeDock { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool ShowGroupHeader { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial string? GroupTitle { get; set; }
}
