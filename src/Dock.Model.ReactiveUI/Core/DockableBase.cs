﻿using System.Runtime.Serialization;
using Dock.Model.Adapters;
using Dock.Model.Core;
using ReactiveUI;

namespace Dock.Model.ReactiveUI.Core;

/// <summary>
/// Dockable base class.
/// </summary>
[DataContract(IsReference = true)]
public abstract partial class DockableBase : ReactiveObject, IDockable
{
    private readonly TrackingAdapter _trackingAdapter;

    /// <summary>
    /// Initializes new instance of the <see cref="DockableBase"/> class.
    /// </summary>
    protected DockableBase()
    {
        _id = string.Empty;
        _title = string.Empty;
        _isCollapsable = true;
        _proportion = double.NaN;
        _canClose = true;
        _canPin = true;
        _canFloat = true;
        _trackingAdapter = new TrackingAdapter();
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial string Id { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial string Title { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [Reactive]
    public partial object? Context { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [Reactive]
    public partial IDockable? Owner { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [Reactive]
    public partial IDockable? OriginalOwner { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [Reactive]
    public partial IFactory? Factory { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsEmpty { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool IsCollapsable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial double Proportion { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool CanClose { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool CanPin { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    [Reactive]
    public partial bool CanFloat { get; set; }

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
