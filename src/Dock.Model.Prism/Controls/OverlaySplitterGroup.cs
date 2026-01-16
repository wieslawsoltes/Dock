// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Prism.Core;

namespace Dock.Model.Prism.Controls;

/// <summary>
/// Overlay splitter group.
/// </summary>
[DataContract(IsReference = true)]
public class OverlaySplitterGroup : DockableBase, IOverlaySplitterGroup
{
    private static readonly IOverlayAdapter s_overlayAdapter = new OverlayAdapter();

    private IOverlayAdapter Overlay => Factory?.OverlayAdapter ?? s_overlayAdapter;

    private IList<IOverlayPanel>? _panels;
    private IList<IOverlaySplitter>? _splitters;
    private Orientation _orientation = Orientation.Horizontal;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private int _zIndex;
    private OverlayAnchor _anchor;
    private bool _isPositionLocked;
    private bool _isDragging;
    private OverlayEdgeDock _edgeDock;
    private bool _showGroupHeader;
    private string? _groupTitle;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlayPanel>? Panels
    {
        get => _panels;
        set
        {
            var previous = _panels;
            SetProperty(ref _panels, value);

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
            SetProperty(ref _splitters, value);
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

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetProperty(ref _orientation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ZIndex
    {
        get => _zIndex;
        set => SetProperty(ref _zIndex, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => SetProperty(ref _anchor, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => SetProperty(ref _isPositionLocked, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetProperty(ref _isDragging, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayEdgeDock EdgeDock
    {
        get => _edgeDock;
        set => SetProperty(ref _edgeDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowGroupHeader
    {
        get => _showGroupHeader;
        set => SetProperty(ref _showGroupHeader, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? GroupTitle
    {
        get => _groupTitle;
        set => SetProperty(ref _groupTitle, value);
    }
}
