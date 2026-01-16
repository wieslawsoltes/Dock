// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Core;

namespace Dock.Model.CaliburMicro.Controls;

/// <summary>
/// Overlay splitter.
/// </summary>
[DataContract(IsReference = true)]
public class OverlaySplitter : DockableBase, IOverlaySplitter
{
    private Orientation _orientation = Orientation.Vertical;
    private double _thickness = 4.0;
    private bool _canResize = true;
    private bool _resizePreview;
    private IOverlayPanel? _panelBefore;
    private IOverlayPanel? _panelAfter;
    private double _minSizeBefore = 80.0;
    private double _minSizeAfter = 80.0;
    private bool _isDragging;

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Orientation Orientation
    {
        get => _orientation;
        set => Set(ref _orientation, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Thickness
    {
        get => _thickness;
        set => Set(ref _thickness, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanResize
    {
        get => _canResize;
        set => Set(ref _canResize, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ResizePreview
    {
        get => _resizePreview;
        set => Set(ref _resizePreview, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IOverlayPanel? PanelBefore
    {
        get => _panelBefore;
        set => Set(ref _panelBefore, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IOverlayPanel? PanelAfter
    {
        get => _panelAfter;
        set => Set(ref _panelAfter, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinSizeBefore
    {
        get => _minSizeBefore;
        set => Set(ref _minSizeBefore, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinSizeAfter
    {
        get => _minSizeAfter;
        set => Set(ref _minSizeAfter, value);
    }

    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => Set(ref _isDragging, value);
    }
}
