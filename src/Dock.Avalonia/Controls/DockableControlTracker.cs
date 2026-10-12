// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>Connects an existing control to Dock's model, bounds and pointer tracking.</summary>
/// <remarks>The control must implement <see cref="IDockableControl"/>. Tracking starts while
/// attached to a visual tree and stops on detachment. Dispose the tracker when replacing it.</remarks>
public sealed class DockableControlTracker : IDisposable
{
    private readonly Control _control;
    private readonly IDockableControl _dockableControl;
    private IDisposable? _boundsDisposable;
    private IDisposable? _dataContextDisposable;
    private IDockable? _currentDockable;
    private bool _attached;
    private bool _disposed;
    private TrackingMode TrackingMode { get; }

    /// <summary>Creates tracking for a control and its current or future native model.</summary>
    /// <param name="control">The control implementing the native docking control contract.</param>
    /// <param name="trackingMode">The native registry and bounds updated by this control.</param>
    /// <exception cref="ArgumentNullException">The control is null.</exception>
    /// <exception cref="ArgumentException">The control does not implement IDockableControl.</exception>
    public DockableControlTracker(Control control, TrackingMode trackingMode)
    {
        ArgumentNullException.ThrowIfNull(control);
        _control = control;
        _dockableControl = control as IDockableControl
            ?? throw new ArgumentException("The control must implement IDockableControl.", nameof(control));
        TrackingMode = trackingMode;
        control.AttachedToVisualTree += Attached;
        control.DetachedFromVisualTree += Detached;
        if (control.GetVisualRoot() is not null) Attach();
    }

    private void Attached(object? sender, VisualTreeAttachmentEventArgs args) => Attach();
    private void Detached(object? sender, VisualTreeAttachmentEventArgs args) => Detach();

    private void Attach()
    {
        if (_attached || _disposed) return;
        _attached = true;
        _dataContextDisposable = _control.GetObservable(Control.DataContextProperty).Subscribe(
            new AnonymousObserver<object?>(DataContextTracking));
        _control.AddHandler(InputElement.PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        _control.AddHandler(InputElement.PointerMovedEvent, MovedHandler, RoutingStrategies.Tunnel);
        _boundsDisposable = _control.GetObservable(Visual.BoundsProperty).Subscribe(
            new AnonymousObserver<Rect>(SetBoundsTracking));
    }

    private void Detach()
    {
        if (!_attached) return;
        _attached = false;
        if (_currentDockable is not null) UnRegister(_currentDockable);
        _currentDockable = null;
        _control.RemoveHandler(InputElement.PointerPressedEvent, PressedHandler);
        _control.RemoveHandler(InputElement.PointerMovedEvent, MovedHandler);
        _boundsDisposable?.Dispose();
        _dataContextDisposable?.Dispose();
        _boundsDisposable = null;
        _dataContextDisposable = null;
    }

    /// <summary>Releases event subscriptions and unregisters the tracked model.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _control.AttachedToVisualTree -= Attached;
        _control.DetachedFromVisualTree -= Detached;
        Detach();
    }

    private void DataContextTracking(object? dataContext)
    {
        if (_currentDockable is not null)
        {
            UnRegister(_currentDockable);
            _currentDockable = null;
        }

        if (dataContext is IDockable dockableChanged)
        {
            _currentDockable = dockableChanged;
            Register(dockableChanged);
        }
    }

    private void Register(IDockable dockable)
    {
        var root = _control.TemplatedParent ?? _control;
        switch (TrackingMode)
        {
            case TrackingMode.Visible:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.VisibleDockableControls[dockable] = _dockableControl;
                    dockable.Factory.VisibleRootControls[dockable] = root;
                }
                break;
            case TrackingMode.Pinned:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.PinnedDockableControls[dockable] = _dockableControl;
                    dockable.Factory.PinnedRootControls[dockable] = root;
                }
                break;
            case TrackingMode.Tab:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.TabDockableControls[dockable] = _dockableControl;
                    dockable.Factory.TabRootControls[dockable] = root;
                }
                break;
        }
    }

    private void UnRegister(IDockable dockable)
    {
        switch (TrackingMode)
        {
            case TrackingMode.Visible:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.VisibleDockableControls.Remove(dockable);
                    dockable.Factory.VisibleRootControls.Remove(dockable);
                }
                break;
            case TrackingMode.Pinned:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.PinnedDockableControls.Remove(dockable);
                    dockable.Factory.PinnedRootControls.Remove(dockable);
                }
                break;
            case TrackingMode.Tab:
                if (dockable.Factory is not null)
                {
                    dockable.Factory.TabDockableControls.Remove(dockable);
                    dockable.Factory.TabRootControls.Remove(dockable);
                }
                break;
        }
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        SetPointerTracking(e);
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        SetPointerTracking(e);
    }

    private void SetBoundsTracking(Rect bounds)
    {
        if (_control.DataContext is not IDockable dockable)
        {
            return;
        }

        if (TrackingMode == TrackingMode.Pinned && _control.FindAncestorOfType<ToolPinItemControl>() is not null)
        {
            return;
        }

        var x = bounds.X;
        var y = bounds.Y;
        var width = bounds.Width;
        var height = bounds.Height;

        if (_control.GetVisualRoot() is Visual relativeTo)
        {
            var translatedPosition = _control.TranslatePoint(bounds.Position, relativeTo);
            if (translatedPosition.HasValue)
            {
                x = translatedPosition.Value.X;
                y = translatedPosition.Value.Y;
            }
        }

        switch (TrackingMode)
        {
            case TrackingMode.Visible:
                dockable.SetVisibleBounds(x, y, width, height);
                break;
            case TrackingMode.Pinned:
                dockable.SetPinnedBounds(x, y, width, height);
                break;
            case TrackingMode.Tab:
                dockable.SetTabBounds(x, y, width, height);
                break;
        }
    }

    private void SetPointerTracking(PointerEventArgs e)
    {
        if (_control.DataContext is not IDockable dockable)
        {
            return;
        }

        var position = e.GetPosition(_control);

        if (_control.GetVisualRoot() is null)
        {
            return;
        }

        var screenPoint = DockHelpers.GetScreenPoint(_control, position);
        var pointerX = screenPoint.X;
        var pointerY = screenPoint.Y;

        if (DockHelpers.IsManagedWindowHostingEnabled(_control) && ManagedWindowLayer.TryGetLayer(_control) is { } layer)
        {
            var layerPoint = _control.TranslatePoint(position, layer);
            if (layerPoint.HasValue)
            {
                pointerX = layerPoint.Value.X;
                pointerY = layerPoint.Value.Y;
            }
        }

        dockable.SetPointerPosition(position.X, position.Y);
        dockable.SetPointerScreenPosition(pointerX, pointerY);
    }
}
