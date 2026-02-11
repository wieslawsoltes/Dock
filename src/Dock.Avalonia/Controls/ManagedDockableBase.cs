// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Base implementation of dockable state, selection metadata, and docking restrictions.
/// </summary>
public abstract class ManagedDockableBase : IDockable, IDockSelectorInfo, IDockableDockingRestrictions, INotifyPropertyChanged
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
    private int _column;
    private int _row;
    private int _columnSpan = 1;
    private int _rowSpan = 1;
    private bool _isSharedSizeScope;
    private double _collapsedProportion = double.NaN;
    private double _minWidth = double.NaN;
    private double _maxWidth = double.NaN;
    private double _minHeight = double.NaN;
    private double _maxHeight = double.NaN;
    private bool _canClose = true;
    private bool _canPin = true;
    private bool _keepPinnedDockableVisible;
    private PinnedDockDisplayMode? _pinnedDockDisplayModeOverride;
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private bool _canDockAsDocument = true;
    private DockCapabilityOverrides? _dockCapabilityOverrides;
    private bool _isModified;
    private string? _dockGroup;
    private DockOperationMask _allowedDockOperations = DockOperationMask.All;
    private DockOperationMask _allowedDropOperations = DockOperationMask.All;
    private bool _showInSelector = true;
    private string? _selectorTitle;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <inheritdoc />
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <inheritdoc />
    public object? Context
    {
        get => _context;
        set => SetProperty(ref _context, value);
    }

    /// <inheritdoc />
    public IDockable? Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    /// <inheritdoc />
    public IDockable? OriginalOwner
    {
        get => _originalOwner;
        set => SetProperty(ref _originalOwner, value);
    }

    /// <inheritdoc />
    public IFactory? Factory
    {
        get => _factory;
        set => SetProperty(ref _factory, value);
    }

    /// <inheritdoc />
    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    /// <inheritdoc />
    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => SetProperty(ref _isCollapsable, value);
    }

    /// <inheritdoc />
    public double Proportion
    {
        get => _proportion;
        set => SetProperty(ref _proportion, value);
    }

    /// <inheritdoc />
    public DockMode Dock
    {
        get => _dock;
        set => SetProperty(ref _dock, value);
    }

    /// <inheritdoc />
    public DockingWindowState DockingState
    {
        get => _dockingState;
        set => SetProperty(ref _dockingState, value);
    }

    /// <inheritdoc />
    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    /// <inheritdoc />
    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    /// <inheritdoc />
    public int ColumnSpan
    {
        get => _columnSpan;
        set => SetProperty(ref _columnSpan, value);
    }

    /// <inheritdoc />
    public int RowSpan
    {
        get => _rowSpan;
        set => SetProperty(ref _rowSpan, value);
    }

    /// <inheritdoc />
    public bool IsSharedSizeScope
    {
        get => _isSharedSizeScope;
        set => SetProperty(ref _isSharedSizeScope, value);
    }

    /// <inheritdoc />
    public double CollapsedProportion
    {
        get => _collapsedProportion;
        set => SetProperty(ref _collapsedProportion, value);
    }

    /// <inheritdoc />
    public double MinWidth
    {
        get => _minWidth;
        set => SetProperty(ref _minWidth, value);
    }

    /// <inheritdoc />
    public double MaxWidth
    {
        get => _maxWidth;
        set => SetProperty(ref _maxWidth, value);
    }

    /// <inheritdoc />
    public double MinHeight
    {
        get => _minHeight;
        set => SetProperty(ref _minHeight, value);
    }

    /// <inheritdoc />
    public double MaxHeight
    {
        get => _maxHeight;
        set => SetProperty(ref _maxHeight, value);
    }

    /// <inheritdoc />
    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    /// <inheritdoc />
    public bool CanPin
    {
        get => _canPin;
        set => SetProperty(ref _canPin, value);
    }

    /// <inheritdoc />
    public bool KeepPinnedDockableVisible
    {
        get => _keepPinnedDockableVisible;
        set => SetProperty(ref _keepPinnedDockableVisible, value);
    }

    /// <inheritdoc />
    public PinnedDockDisplayMode? PinnedDockDisplayModeOverride
    {
        get => _pinnedDockDisplayModeOverride;
        set => SetProperty(ref _pinnedDockDisplayModeOverride, value);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool CanFloat
    {
        get => _canFloat;
        set => SetProperty(ref _canFloat, value);
    }

    /// <inheritdoc />
    public bool CanDrag
    {
        get => _canDrag;
        set => SetProperty(ref _canDrag, value);
    }

    /// <inheritdoc />
    public bool CanDrop
    {
        get => _canDrop;
        set => SetProperty(ref _canDrop, value);
    }

    /// <inheritdoc />
    public bool CanDockAsDocument
    {
        get => _canDockAsDocument;
        set => SetProperty(ref _canDockAsDocument, value);
    }

    /// <inheritdoc />
    public DockCapabilityOverrides? DockCapabilityOverrides
    {
        get => _dockCapabilityOverrides;
        set => SetProperty(ref _dockCapabilityOverrides, value);
    }

    /// <inheritdoc />
    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    /// <inheritdoc />
    public string? DockGroup
    {
        get => _dockGroup;
        set => SetProperty(ref _dockGroup, value);
    }

    /// <inheritdoc />
    public DockOperationMask AllowedDockOperations
    {
        get => _allowedDockOperations;
        set => SetProperty(ref _allowedDockOperations, value);
    }

    /// <inheritdoc />
    public DockOperationMask AllowedDropOperations
    {
        get => _allowedDropOperations;
        set => SetProperty(ref _allowedDropOperations, value);
    }

    /// <inheritdoc />
    public bool ShowInSelector
    {
        get => _showInSelector;
        set => SetProperty(ref _showInSelector, value);
    }

    /// <inheritdoc />
    public string? SelectorTitle
    {
        get => _selectorTitle;
        set => SetProperty(ref _selectorTitle, value);
    }

    /// <inheritdoc />
    public string? GetControlRecyclingId() => _id;

    /// <inheritdoc />
    public virtual bool OnClose()
    {
        return true;
    }

    /// <inheritdoc />
    public virtual void OnSelected()
    {
    }

    /// <inheritdoc />
    public void GetVisibleBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc />
    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc />
    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc />
    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc />
    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc />
    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc />
    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc />
    public void SetTabBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc />
    public virtual void OnTabBoundsChanged(double x, double y, double width, double height)
    {
    }

    /// <inheritdoc />
    public void GetPointerPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerPosition(out x, out y);
    }

    /// <inheritdoc />
    public void SetPointerPosition(double x, double y)
    {
        TrackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    /// <inheritdoc />
    public virtual void OnPointerPositionChanged(double x, double y)
    {
    }

    /// <inheritdoc />
    public void GetPointerScreenPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    /// <inheritdoc />
    public void SetPointerScreenPosition(double x, double y)
    {
        TrackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    /// <inheritdoc />
    public virtual void OnPointerScreenPositionChanged(double x, double y)
    {
    }

    private static bool IsPinnedBoundsValid(double width, double height)
    {
        return !double.IsNaN(width) && !double.IsNaN(height) &&
               !double.IsInfinity(width) && !double.IsInfinity(height);
    }

    /// <summary>
    /// Sets a backing field and raises <see cref="PropertyChanged"/> when the value changes.
    /// </summary>
    /// <param name="field">The field to update.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name that changed.</param>
    /// <typeparam name="T">The field type.</typeparam>
    /// <returns><see langword="true" /> if the value changed; otherwise <see langword="false" />.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName ?? string.Empty);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
