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
using Dock.Model.Core;
using Avalonia.Controls.Primitives;

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
    private PixelPoint _dragStartScreenPoint;
    private bool _pointerPressed;
    private bool _isDragging;
    private PointerPressedEventArgs? _lastPointerPressedArgs;
    private IHostWindow? _dragHostWindow;
    private EventHandler<PixelPointEventArgs>? _positionChangedHandler;
    private PixelPoint _popupStartPosition;

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
            _dragStartScreenPoint = _owner.PointToScreen(_dragStartPoint);
            _pointerPressed = true;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_pointerPressed && _dragHostWindow is null)
        {
            return;
        }

        _pointerPressed = false;
        _isDragging = false;

        if (_dragHostWindow is not null)
        {
            if (_positionChangedHandler is not null)
            {
                if (_dragHostWindow is HostWindow hw)
                    hw.PositionChanged -= _positionChangedHandler;
                _positionChangedHandler = null;
            }

            if (_dragHostWindow is HostWindow hostWindow)
            {
                if (hostWindow.HostWindowState is HostWindowState state)
                {
                    var point = hostWindow.PointToScreen(e.GetPosition(hostWindow)) -
                                hostWindow.PointToScreen(new Point(0, 0));
                    state.Process(new PixelPoint(point.X, point.Y), EventType.Released);
                }

                hostWindow.Window?.Factory?.OnWindowMoveDragEnd(hostWindow.Window);
            }
            else if (_dragHostWindow is PopupHostWindow popup)
            {
                popup.Window?.Factory?.OnWindowMoveDragEnd(popup.Window);
            }
        }

        _dragHostWindow = null;

        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_pointerPressed && _dragHostWindow is null)
        {
            return;
        }

        if (_dragHostWindow is PopupHostWindow popupDragging)
        {
            var screenPoint = _owner.PointToScreen(e.GetPosition(_owner));
            var deltaScreen = screenPoint - _dragStartScreenPoint;
            popupDragging.SetPosition(_popupStartPosition.X + deltaScreen.X, _popupStartPosition.Y + deltaScreen.Y);
            popupDragging.Window?.Factory?.OnWindowMoveDrag(popupDragging.Window!);
            return;
        }

        var currentPoint = e.GetPosition(_owner);
        var delta = currentPoint - _dragStartPoint;

        if (!(Math.Abs(delta.X) > DockSettings.MinimumHorizontalDragDistance)
            && !(Math.Abs(delta.Y) > DockSettings.MinimumVerticalDragDistance))
        {
            return;
        }

  

        if (_lastPointerPressedArgs is null)
        {
            return;
        }

        var root = _owner.GetVisualRoot();

        switch (root)
        {
            case HostWindow hostWindow:
            {
                _isDragging = true;
                _pointerPressed = false;

                var dockWindow = hostWindow.Window;
                if (dockWindow?.Factory?.OnWindowMoveDragBegin(dockWindow) != true)
                {
                    _isDragging = false;
                    return;
                }

                if (hostWindow.HostWindowState is HostWindowState state)
                {
                    var start = hostWindow.PointToScreen(_lastPointerPressedArgs.GetPosition(hostWindow)) - hostWindow.PointToScreen(new Point(0, 0));
                    state.Process(new PixelPoint(start.X, start.Y), EventType.Pressed);
                }

                _dragHostWindow = hostWindow;

                _positionChangedHandler = (_, _) =>
                {
                    if (hostWindow.Window is { } dw)
                    {
                        dw.Factory?.OnWindowMoveDrag(dw);
                    }

                    if (hostWindow.HostWindowState is HostWindowState st)
                    {
                        st.Process(hostWindow.Position, EventType.Moved);
                    }
                };

                hostWindow.PositionChanged += _positionChangedHandler;
                hostWindow.BeginMoveDrag(_lastPointerPressedArgs);
                e.Handled = true;
                return;
            }
            case PopupRoot { Parent: PopupHostWindow popupHostWindow }:
            {
                _isDragging = true;

                var dockWindow = popupHostWindow.Window;
                if (dockWindow?.Factory?.OnWindowMoveDragBegin(dockWindow) != true)
                {
                    _isDragging = false;
                    return;
                }

                _dragHostWindow = popupHostWindow;
                _popupStartPosition = new PixelPoint((int)popupHostWindow.HorizontalOffset, (int)popupHostWindow.VerticalOffset);
                _dragStartScreenPoint = _owner.PointToScreen(e.GetPosition(_owner));

                return;
            }
            case Window window:
            {
                window.BeginMoveDrag(_lastPointerPressedArgs);
                return;
            }
            default:
                return;
        }
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
