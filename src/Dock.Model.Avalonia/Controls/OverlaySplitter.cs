// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Avalonia;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Avalonia.Controls;

/// <summary>
/// Overlay splitter.
/// </summary>
[DataContract(IsReference = true)]
public class OverlaySplitter : DockableBase, IOverlaySplitter
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, Orientation> OrientationProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, Orientation>(nameof(Orientation), o => o.Orientation, (o, v) => o.Orientation = v);

    /// <summary>
    /// Defines the <see cref="Thickness"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, double> ThicknessProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, double>(nameof(Thickness), o => o.Thickness, (o, v) => o.Thickness = v);

    /// <summary>
    /// Defines the <see cref="CanResize"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, bool> CanResizeProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, bool>(nameof(CanResize), o => o.CanResize, (o, v) => o.CanResize = v);

    /// <summary>
    /// Defines the <see cref="ResizePreview"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, bool> ResizePreviewProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, bool>(nameof(ResizePreview), o => o.ResizePreview, (o, v) => o.ResizePreview = v);

    /// <summary>
    /// Defines the <see cref="PanelBefore"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, IOverlayPanel?> PanelBeforeProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, IOverlayPanel?>(nameof(PanelBefore), o => o.PanelBefore, (o, v) => o.PanelBefore = v);

    /// <summary>
    /// Defines the <see cref="PanelAfter"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, IOverlayPanel?> PanelAfterProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, IOverlayPanel?>(nameof(PanelAfter), o => o.PanelAfter, (o, v) => o.PanelAfter = v);

    /// <summary>
    /// Defines the <see cref="MinSizeBefore"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, double> MinSizeBeforeProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, double>(nameof(MinSizeBefore), o => o.MinSizeBefore, (o, v) => o.MinSizeBefore = v);

    /// <summary>
    /// Defines the <see cref="MinSizeAfter"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, double> MinSizeAfterProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, double>(nameof(MinSizeAfter), o => o.MinSizeAfter, (o, v) => o.MinSizeAfter = v);

    /// <summary>
    /// Defines the <see cref="IsDragging"/> property.
    /// </summary>
    public static readonly DirectProperty<OverlaySplitter, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<OverlaySplitter, bool>(nameof(IsDragging), o => o.IsDragging, (o, v) => o.IsDragging = v);

    private Orientation _orientation = Orientation.Vertical;
    private double _thickness = 4.0;
    private bool _canResize = true;
    private bool _resizePreview;
    private IOverlayPanel? _panelBefore;
    private IOverlayPanel? _panelAfter;
    private double _minSizeBefore = 80.0;
    private double _minSizeAfter = 80.0;
    private bool _isDragging;

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
    [JsonPropertyName("Thickness")]
    public double Thickness
    {
        get => _thickness;
        set => SetAndRaise(ThicknessProperty, ref _thickness, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("CanResize")]
    public bool CanResize
    {
        get => _canResize;
        set => SetAndRaise(CanResizeProperty, ref _canResize, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("ResizePreview")]
    public bool ResizePreview
    {
        get => _resizePreview;
        set => SetAndRaise(ResizePreviewProperty, ref _resizePreview, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("PanelBefore")]
    public IOverlayPanel? PanelBefore
    {
        get => _panelBefore;
        set => SetAndRaise(PanelBeforeProperty, ref _panelBefore, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("PanelAfter")]
    public IOverlayPanel? PanelAfter
    {
        get => _panelAfter;
        set => SetAndRaise(PanelAfterProperty, ref _panelAfter, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MinSizeBefore")]
    public double MinSizeBefore
    {
        get => _minSizeBefore;
        set => SetAndRaise(MinSizeBeforeProperty, ref _minSizeBefore, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [JsonPropertyName("MinSizeAfter")]
    public double MinSizeAfter
    {
        get => _minSizeAfter;
        set => SetAndRaise(MinSizeAfterProperty, ref _minSizeAfter, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [JsonIgnore]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }
}
