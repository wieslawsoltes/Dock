// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.Inpc.Core;

/// <summary>
/// Dockable base class.
/// </summary>
[DataContract(IsReference = true)]
public abstract class DockableBase : ReactiveBase, IDockable
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
    private int _column = 0;
    private int _row = 0;
    private int _columnSpan = 1;
    private int _rowSpan = 1;
    private bool _isSharedSizeScope;
    private double _collapsedProportion = double.NaN;
    private bool _canClose = true;
    private bool _canPin = true;
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;

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
        set => SetProperty(ref _id, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public object? Context
    {
        get => _context;
        set => SetProperty(ref _context, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? OriginalOwner
    {
        get => _originalOwner;
        set => SetProperty(ref _originalOwner, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IFactory? Factory
    {
        get => _factory;
        set => SetProperty(ref _factory, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => SetProperty(ref _isCollapsable, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Proportion
    {
        get => _proportion;
        set => SetProperty(ref _proportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockMode Dock
    {
        get => _dock;
        set => SetProperty(ref _dock, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ColumnSpan
    {
        get => _columnSpan;
        set => SetProperty(ref _columnSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int RowSpan
    {
        get => _rowSpan;
        set => SetProperty(ref _rowSpan, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsSharedSizeScope
    {
        get => _isSharedSizeScope;
        set => SetProperty(ref _isSharedSizeScope, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CollapsedProportion
    {
        get => _collapsedProportion;
        set => SetProperty(ref _collapsedProportion, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinWidth
    {
        get => _minWidth;
        set => SetProperty(ref _minWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxWidth
    {
        get => _maxWidth;
        set => SetProperty(ref _maxWidth, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinHeight
    {
        get => _minHeight;
        set => SetProperty(ref _minHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxHeight
    {
        get => _maxHeight;
        set => SetProperty(ref _maxHeight, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanPin
    {
        get => _canPin;
        set => SetProperty(ref _canPin, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanFloat
    {
        get => _canFloat;
        set => SetProperty(ref _canFloat, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrag
    {
        get => _canDrag;
        set => SetProperty(ref _canDrag, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrop
    {
        get => _canDrop;
        set => SetProperty(ref _canDrop, value);
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
