// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.CaliburMicro.Core;

/// <summary>
/// Dockable base class.
/// </summary>
[DataContract(IsReference = true)]
public abstract class DockableBase : CaliburMicroBase, IDockable
{
    private readonly TrackingAdapter _trackingAdapter;
    private string _id = string.Empty;
    private string _title = string.Empty;
    private object? _context;
    private IDockable? _owner;
    private IDockable? _originalOwner;
    private IFactory? _factory;
    private bool _isEmpty;
    private bool _isCollapsable = true;
    private double _proportion = double.NaN;
    private DockMode _dock = DockMode.Center;
    private int _column;
    private int _columnSpan = 1;
    private int _row;
    private int _rowSpan = 1;
    private bool _isSharedSizeScope;
    private double _collapsedProportion = double.NaN;
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;
    private bool _canClose = true;
    private bool _canPin = true;
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private bool _isModified;
    private string? _dockGroup;

    /// <summary>
    /// Initializes new instance of the <see cref="DockableBase"/> class.
    /// </summary>
    protected DockableBase()
    {
        _trackingAdapter = new TrackingAdapter();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Id
    {
        get => _id;
        set => Set(ref _id, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public object? Context
    {
        get => _context;
        set => Set(ref _context, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? Owner
    {
        get => _owner;
        set => Set(ref _owner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? OriginalOwner
    {
        get => _originalOwner;
        set => Set(ref _originalOwner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IFactory? Factory
    {
        get => _factory;
        set => Set(ref _factory, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsEmpty
    {
        get => _isEmpty;
        set => Set(ref _isEmpty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => Set(ref _isCollapsable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Proportion
    {
        get => _proportion;
        set => Set(ref _proportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockMode Dock
    {
        get => _dock;
        set => Set(ref _dock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Column
    {
        get => _column;
        set => Set(ref _column, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Row
    {
        get => _row;
        set => Set(ref _row, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ColumnSpan
    {
        get => _columnSpan;
        set => Set(ref _columnSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int RowSpan
    {
        get => _rowSpan;
        set => Set(ref _rowSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsSharedSizeScope
    {
        get => _isSharedSizeScope;
        set => Set(ref _isSharedSizeScope, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CollapsedProportion
    {
        get => _collapsedProportion;
        set => Set(ref _collapsedProportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinWidth
    {
        get => _minWidth;
        set => Set(ref _minWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxWidth
    {
        get => _maxWidth;
        set => Set(ref _maxWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinHeight
    {
        get => _minHeight;
        set => Set(ref _minHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxHeight
    {
        get => _maxHeight;
        set => Set(ref _maxHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanClose
    {
        get => _canClose;
        set => Set(ref _canClose, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanPin
    {
        get => _canPin;
        set => Set(ref _canPin, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanFloat
    {
        get => _canFloat;
        set => Set(ref _canFloat, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrag
    {
        get => _canDrag;
        set => Set(ref _canDrag, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrop
    {
        get => _canDrop;
        set => Set(ref _canDrop, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsModified
    {
        get => _isModified;
        set => Set(ref _isModified, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? DockGroup
    {
        get => _dockGroup;
        set => Set(ref _dockGroup, value);
    }

    /// <inheritdoc/>
    public string? GetControlRecyclingId() => _id;

    /// <inheritdoc/>
    public virtual bool OnClose()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void OnSelected()
    {
    }

    /// <inheritdoc/>
    public void GetVisibleBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetTabBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnTabBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPointerPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerPosition(double x, double y)
    {
        _trackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerPositionChanged(double x, double y)
    {
    }

    /// <inheritdoc/>
    public void GetPointerScreenPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerScreenPosition(double x, double y)
    {
        _trackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerScreenPositionChanged(double x, double y)
    {
    }
}