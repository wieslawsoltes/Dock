// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

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

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlayPanel>? Panels
    {
        get => _panels;
        set
        {
            var previous = _panels;
            Set(ref _panels, value);

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

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IOverlaySplitter>? Splitters
    {
        get => _splitters;
        set
        {
            Set(ref _splitters, value);
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

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => Set(ref _orientation, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double X
    {
        get => _x;
        set => Set(ref _x, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Y
    {
        get => _y;
        set => Set(ref _y, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Width
    {
        get => _width;
        set => Set(ref _width, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Height
    {
        get => _height;
        set => Set(ref _height, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ZIndex
    {
        get => _zIndex;
        set => Set(ref _zIndex, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => Set(ref _anchor, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => Set(ref _isPositionLocked, value);
    }

    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => Set(ref _isDragging, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public OverlayEdgeDock EdgeDock
    {
        get => _edgeDock;
        set => Set(ref _edgeDock, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowGroupHeader
    {
        get => _showGroupHeader;
        set => Set(ref _showGroupHeader, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? GroupTitle
    {
        get => _groupTitle;
        set => Set(ref _groupTitle, value);
    }
}
