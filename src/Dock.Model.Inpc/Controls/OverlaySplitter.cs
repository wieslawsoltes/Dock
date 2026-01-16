// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Inpc.Core;

namespace Dock.Model.Inpc.Controls;

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
        set => SetProperty(ref _orientation, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Thickness
    {
        get => _thickness;
        set => SetProperty(ref _thickness, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanResize
    {
        get => _canResize;
        set => SetProperty(ref _canResize, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ResizePreview
    {
        get => _resizePreview;
        set => SetProperty(ref _resizePreview, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IOverlayPanel? PanelBefore
    {
        get => _panelBefore;
        set => SetProperty(ref _panelBefore, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IOverlayPanel? PanelAfter
    {
        get => _panelAfter;
        set => SetProperty(ref _panelAfter, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinSizeBefore
    {
        get => _minSizeBefore;
        set => SetProperty(ref _minSizeBefore, value);
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinSizeAfter
    {
        get => _minSizeAfter;
        set => SetProperty(ref _minSizeAfter, value);
    }

    [IgnoreDataMember]
    public bool IsDragging
    {
        get => _isDragging;
        set => SetProperty(ref _isDragging, value);
    }
}
