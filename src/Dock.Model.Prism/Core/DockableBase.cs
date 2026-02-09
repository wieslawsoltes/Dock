// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.Prism.Core;

/// <summary>
/// Dockable base class.
/// </summary>
public abstract class DockableBase : ReactiveBase, IDockable, IDockSelectorInfo, IDockableDockingRestrictions
{
    private TrackingAdapter? _trackingAdapter;

    private TrackingAdapter TrackingAdapter => _trackingAdapter ??= new TrackingAdapter();
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
    private DockingWindowState _dockingState = DockingWindowState.Docked;
    private int _column = 0;
    private int _row = 0;
    private int _columnSpan = 1;
    private int _rowSpan = 1;
    private bool _isSharedSizeScope;
    private double _collapsedProportion = double.NaN;
    private bool _canClose = true;
    private bool _canPin = true;
    private bool _keepPinnedDockableVisible;
    private PinnedDockDisplayMode? _pinnedDockDisplayModeOverride;
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private bool _canDockAsDocument = true;
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;
    private bool _isModified;
    private string? _dockGroup;
    private DockOperationMask _allowedDockOperations = DockOperationMask.All;
    private DockOperationMask _allowedDropOperations = DockOperationMask.All;
    private bool _showInSelector = true;
    private string? _selectorTitle;

    /// <summary>
    /// Initializes new instance of the <see cref="DockableBase"/> class.
    /// </summary>
    protected DockableBase()
    {
        _trackingAdapter = new TrackingAdapter();
        _isModified = false;
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
    [IgnoreDataMember]
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
    public DockingWindowState DockingState
    {
        get => _dockingState;
        set => SetProperty(ref _dockingState, value);
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
    public bool KeepPinnedDockableVisible
    {
        get => _keepPinnedDockableVisible;
        set => SetProperty(ref _keepPinnedDockableVisible, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public PinnedDockDisplayMode? PinnedDockDisplayModeOverride
    {
        get => _pinnedDockDisplayModeOverride;
        set => SetProperty(ref _pinnedDockDisplayModeOverride, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockRect? PinnedBounds
    {
        get
        {
            GetPinnedBounds(out var x, out var y, out var width, out var height);
            return IsPinnedBoundsValid(width, height) ? new DockRect(x, y, width, height) : null;
        }
        set
        {
            if (value is null)
            {
                SetPinnedBounds(double.NaN, double.NaN, double.NaN, double.NaN);
                return;
            }

            SetPinnedBounds(value.Value.X, value.Value.Y, value.Value.Width, value.Value.Height);
        }
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
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDockAsDocument
    {
        get => _canDockAsDocument;
        set => SetProperty(ref _canDockAsDocument, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? DockGroup
    {
        get => _dockGroup;
        set => SetProperty(ref _dockGroup, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockOperationMask AllowedDockOperations
    {
        get => _allowedDockOperations;
        set => SetProperty(ref _allowedDockOperations, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockOperationMask AllowedDropOperations
    {
        get => _allowedDropOperations;
        set => SetProperty(ref _allowedDropOperations, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowInSelector
    {
        get => _showInSelector;
        set => SetProperty(ref _showInSelector, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? SelectorTitle
    {
        get => _selectorTitle;
        set => SetProperty(ref _selectorTitle, value);
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
        TrackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetTabBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnTabBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc/>
    public void GetPointerPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerPosition(double x, double y)
    {
        TrackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerPositionChanged(double x, double y)
    {
    }

    /// <inheritdoc/>
    public void GetPointerScreenPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerScreenPosition(double x, double y)
    {
        TrackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerScreenPositionChanged(double x, double y)
    {
    }

    private static bool IsPinnedBoundsValid(double width, double height)
    {
        return !double.IsNaN(width) && !double.IsNaN(height) &&
               !double.IsInfinity(width) && !double.IsInfinity(height);
    }
}
