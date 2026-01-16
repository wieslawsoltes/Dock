// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Collections;
using Dock.Model.Adapters;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Overlay splitter group.
/// </summary>
[DataContract(IsReference = true)]
public class OverlaySplitterGroup : DockableBase, IOverlaySplitterGroup
{
    /// <summary>
    /// Defines the <see cref="Panels"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, IList<IOverlayPanel>?> PanelsProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, IList<IOverlayPanel>?>(nameof(Panels), o => o.Panels, (o, v) => o.Panels = v);

    /// <summary>
    /// Defines the <see cref="Splitters"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, IList<IOverlaySplitter>?> SplittersProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, IList<IOverlaySplitter>?>(nameof(Splitters), o => o.Splitters, (o, v) => o.Splitters = v);

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    /// <summary>
    /// Defines the <see cref="X"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, double> XProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, double>(nameof(X), o => o.X, (o, v) => o.X = v);

    /// <summary>
    /// Defines the <see cref="Y"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, double> YProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, double>(nameof(Y), o => o.Y, (o, v) => o.Y = v);

    /// <summary>
    /// Defines the <see cref="Width"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, double> WidthProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, double>(nameof(Width), o => o.Width, (o, v) => o.Width = v);

    /// <summary>
    /// Defines the <see cref="Height"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, double> HeightProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, double>(nameof(Height), o => o.Height, (o, v) => o.Height = v);

    /// <summary>
    /// Defines the <see cref="ZIndex"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, int> ZIndexProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, int>(nameof(ZIndex), o => o.ZIndex, (o, v) => o.ZIndex = v);

    /// <summary>
    /// Defines the <see cref="Anchor"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, OverlayAnchor> AnchorProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, OverlayAnchor>(nameof(Anchor), o => o.Anchor, (o, v) => o.Anchor = v);

    /// <summary>
    /// Defines the <see cref="IsPositionLocked"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, bool> IsPositionLockedProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, bool>(nameof(IsPositionLocked), o => o.IsPositionLocked, (o, v) => o.IsPositionLocked = v);

    /// <summary>
    /// Defines the <see cref="IsDragging"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, bool>(nameof(IsDragging), o => o.IsDragging, (o, v) => o.IsDragging = v);

    /// <summary>
    /// Defines the <see cref="EdgeDock"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, OverlayEdgeDock> EdgeDockProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, OverlayEdgeDock>(nameof(EdgeDock), o => o.EdgeDock, (o, v) => o.EdgeDock = v);

    /// <summary>
    /// Defines the <see cref="ShowGroupHeader"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, bool> ShowGroupHeaderProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, bool>(nameof(ShowGroupHeader), o => o.ShowGroupHeader, (o, v) => o.ShowGroupHeader = v);

    /// <summary>
    /// Defines the <see cref="GroupTitle"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitterGroup, string?> GroupTitleProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitterGroup, string?>(nameof(GroupTitle), o => o.GroupTitle, (o, v) => o.GroupTitle = v);

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

    /// <summary>
    /// Initializes new instance of the <see cref="OverlaySplitterGroup"/> class.
    /// </summary>
    public OverlaySplitterGroup()
    {
        _panels = new AvaloniaList<IOverlayPanel>();
        _splitters = new AvaloniaList<IOverlaySplitter>();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Panels")]
    public IList<IOverlayPanel>? Panels
    {
        get => _panels;
        set
        {
            var previous = _panels;
            SetAndRaise(PanelsProperty, ref _panels, value);

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
    [JsonPropertyName("Splitters")]
    public IList<IOverlaySplitter>? Splitters
    {
        get => _splitters;
        set
        {
            SetAndRaise(SplittersProperty, ref _splitters, value);
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
    [JsonPropertyName("Orientation")]
    public Orientation Orientation
    {
        get => _orientation;
        set => SetAndRaise(OrientationProperty, ref _orientation, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("X")]
    public double X
    {
        get => _x;
        set => SetAndRaise(XProperty, ref _x, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Y")]
    public double Y
    {
        get => _y;
        set => SetAndRaise(YProperty, ref _y, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Width")]
    public double Width
    {
        get => _width;
        set => SetAndRaise(WidthProperty, ref _width, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Height")]
    public double Height
    {
        get => _height;
        set => SetAndRaise(HeightProperty, ref _height, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ZIndex")]
    public int ZIndex
    {
        get => _zIndex;
        set => SetAndRaise(ZIndexProperty, ref _zIndex, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("Anchor")]
    public OverlayAnchor Anchor
    {
        get => _anchor;
        set => SetAndRaise(AnchorProperty, ref _anchor, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("IsPositionLocked")]
    public bool IsPositionLocked
    {
        get => _isPositionLocked;
        set => SetAndRaise(IsPositionLockedProperty, ref _isPositionLocked, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("EdgeDock")]
    public OverlayEdgeDock EdgeDock
    {
        get => _edgeDock;
        set => SetAndRaise(EdgeDockProperty, ref _edgeDock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ShowGroupHeader")]
    public bool ShowGroupHeader
    {
        get => _showGroupHeader;
        set => SetAndRaise(ShowGroupHeaderProperty, ref _showGroupHeader, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("GroupTitle")]
    public string? GroupTitle
    {
        get => _groupTitle;
        set => SetAndRaise(GroupTitleProperty, ref _groupTitle, value);
    }
}
