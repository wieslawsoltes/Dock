// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

public abstract class ManagedDockableBase : IDockable, IDockSelectorInfo, IDockableDockingRestrictions, INotifyPropertyChanged
{
    private readonly TrackingAdapter _trackingAdapter = new();
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
    private bool _canFloat = true;
    private bool _canDrag = true;
    private bool _canDrop = true;
    private bool _canDockAsDocument = true;
    private bool _isModified;
    private string? _dockGroup;
    private DockOperationMask _allowedDockOperations = DockOperationMask.All;
    private DockOperationMask _allowedDropOperations = DockOperationMask.All;
    private bool _showInSelector = true;
    private string? _selectorTitle;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public object? Context
    {
        get => _context;
        set => SetProperty(ref _context, value);
    }

    public IDockable? Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    public IDockable? OriginalOwner
    {
        get => _originalOwner;
        set => SetProperty(ref _originalOwner, value);
    }

    public IFactory? Factory
    {
        get => _factory;
        set => SetProperty(ref _factory, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public bool IsCollapsable
    {
        get => _isCollapsable;
        set => SetProperty(ref _isCollapsable, value);
    }

    public double Proportion
    {
        get => _proportion;
        set => SetProperty(ref _proportion, value);
    }

    public DockMode Dock
    {
        get => _dock;
        set => SetProperty(ref _dock, value);
    }

    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    public int ColumnSpan
    {
        get => _columnSpan;
        set => SetProperty(ref _columnSpan, value);
    }

    public int RowSpan
    {
        get => _rowSpan;
        set => SetProperty(ref _rowSpan, value);
    }

    public bool IsSharedSizeScope
    {
        get => _isSharedSizeScope;
        set => SetProperty(ref _isSharedSizeScope, value);
    }

    public double CollapsedProportion
    {
        get => _collapsedProportion;
        set => SetProperty(ref _collapsedProportion, value);
    }

    public double MinWidth
    {
        get => _minWidth;
        set => SetProperty(ref _minWidth, value);
    }

    public double MaxWidth
    {
        get => _maxWidth;
        set => SetProperty(ref _maxWidth, value);
    }

    public double MinHeight
    {
        get => _minHeight;
        set => SetProperty(ref _minHeight, value);
    }

    public double MaxHeight
    {
        get => _maxHeight;
        set => SetProperty(ref _maxHeight, value);
    }

    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    public bool CanPin
    {
        get => _canPin;
        set => SetProperty(ref _canPin, value);
    }

    public bool KeepPinnedDockableVisible
    {
        get => _keepPinnedDockableVisible;
        set => SetProperty(ref _keepPinnedDockableVisible, value);
    }

    public bool CanFloat
    {
        get => _canFloat;
        set => SetProperty(ref _canFloat, value);
    }

    public bool CanDrag
    {
        get => _canDrag;
        set => SetProperty(ref _canDrag, value);
    }

    public bool CanDrop
    {
        get => _canDrop;
        set => SetProperty(ref _canDrop, value);
    }

    public bool CanDockAsDocument
    {
        get => _canDockAsDocument;
        set => SetProperty(ref _canDockAsDocument, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    public string? DockGroup
    {
        get => _dockGroup;
        set => SetProperty(ref _dockGroup, value);
    }

    public DockOperationMask AllowedDockOperations
    {
        get => _allowedDockOperations;
        set => SetProperty(ref _allowedDockOperations, value);
    }

    public DockOperationMask AllowedDropOperations
    {
        get => _allowedDropOperations;
        set => SetProperty(ref _allowedDropOperations, value);
    }

    public bool ShowInSelector
    {
        get => _showInSelector;
        set => SetProperty(ref _showInSelector, value);
    }

    public string? SelectorTitle
    {
        get => _selectorTitle;
        set => SetProperty(ref _selectorTitle, value);
    }

    public string? GetControlRecyclingId() => _id;

    public virtual bool OnClose()
    {
        return true;
    }

    public virtual void OnSelected()
    {
    }

    public void GetVisibleBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height)
    {
    }

    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height)
    {
    }

    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        _trackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    public void SetTabBounds(double x, double y, double width, double height)
    {
        _trackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    public virtual void OnTabBoundsChanged(double x, double y, double width, double height)
    {
    }

    public void GetPointerPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerPosition(out x, out y);
    }

    public void SetPointerPosition(double x, double y)
    {
        _trackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    public virtual void OnPointerPositionChanged(double x, double y)
    {
    }

    public void GetPointerScreenPosition(out double x, out double y)
    {
        _trackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    public void SetPointerScreenPosition(double x, double y)
    {
        _trackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    public virtual void OnPointerScreenPositionChanged(double x, double y)
    {
    }

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

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
