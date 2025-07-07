// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Settings;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Helper that enables starting window drag operations from custom controls.
/// </summary>
internal class WindowDragHelper
{
    private readonly Control _owner;
    private readonly Func<bool> _isEnabled;
    private readonly Func<Control?, bool> _canStartDrag;
    private Point _dragStartPoint;
    private bool _pointerPressed;
    private bool _isDragging;
    private PointerPressedEventArgs? _lastPointerPressedArgs;
    private HostWindow? _dragHostWindow;
    private EventHandler<PixelPointEventArgs>? _positionChangedHandler;

    public WindowDragHelper(Control owner, Func<bool> isEnabled, Func<Control?, bool> canStartDrag)
    {
        _owner = owner;
        _isEnabled = isEnabled;
        _canStartDrag = canStartDrag;
    }

    public void Attach()
    {
        _owner.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        _owner.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        _owner.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
    }

    public void Detach()
    {
        _owner.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        _owner.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _owner.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_isEnabled())
        {
            return;
        }

        _lastPointerPressedArgs = e;

        if (!e.GetCurrentPoint(_owner).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Control;
        if (_canStartDrag(source))
        {
            _dragStartPoint = e.GetPosition(_owner);
            _pointerPressed = true;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_pointerPressed && !_isDragging)
        {
            return;
        }

        _pointerPressed = false;
        _isDragging = false;

        if (_dragHostWindow is { } host)
        {
            if (_positionChangedHandler is { })
            {
                host.PositionChanged -= _positionChangedHandler;
                _positionChangedHandler = null;
            }

            if (host.HostWindowState is HostWindowState state)
            {
                var point = host.PointToScreen(e.GetPosition(host)) - host.PointToScreen(new Point(0, 0));
                state.Process(new PixelPoint((int)point.X, (int)point.Y), EventType.Released);
            }

            host.Window?.Factory?.OnWindowMoveDragEnd(host.Window);
        }

        _dragHostWindow = null;

        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_pointerPressed || _isDragging)
        {
            return;
        }

        var currentPoint = e.GetPosition(_owner);
        var delta = currentPoint - _dragStartPoint;

        if (!(Math.Abs(delta.X) > DockSettings.MinimumHorizontalDragDistance)
            && !(Math.Abs(delta.Y) > DockSettings.MinimumVerticalDragDistance))
        {
            return;
        }

        _isDragging = true;
        _pointerPressed = false;

        if (_lastPointerPressedArgs is null)
        {
            return;
        }

        var root = _owner.GetVisualRoot();

        if (root is not HostWindow hostWindow)
        {
            if (root is Window window)
            {
                window.BeginMoveDrag(_lastPointerPressedArgs);
            }
            return;
        }

        var dockWindow = hostWindow.Window;
        if (dockWindow?.Factory?.OnWindowMoveDragBegin(dockWindow) != true)
        {
            _isDragging = false;
            return;
        }

        if (hostWindow.HostWindowState is HostWindowState state)
        {
            var start = hostWindow.PointToScreen(_lastPointerPressedArgs.GetPosition(hostWindow)) - hostWindow.PointToScreen(new Point(0, 0));
            state.Process(new PixelPoint((int)start.X, (int)start.Y), EventType.Pressed);
        }

        _dragHostWindow = hostWindow;

        _positionChangedHandler = (_, args) =>
        {
            if (_dragHostWindow?.Window is { } dw)
            {
                dw.Factory?.OnWindowMoveDrag(dw);
            }

            if (_dragHostWindow?.HostWindowState is HostWindowState st)
            {
                st.Process(_dragHostWindow.Position, EventType.Moved);
            }
        };

        hostWindow.PositionChanged += _positionChangedHandler;

        hostWindow.BeginMoveDrag(_lastPointerPressedArgs);
        e.Handled = true;
    }

    internal static bool IsChildOfType<T>(Control owner, Control control) where T : Control
    {
        var parent = control;
        while (parent != null && parent != owner)
        {
            if (parent is T)
            {
                return true;
            }

            parent = parent.Parent as Control;
        }

        return false;
    }
}
