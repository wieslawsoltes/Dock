// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

/// <summary>
/// Helper that enables starting window drag operations from custom controls.
/// </summary>
internal class WindowDragHelper
{
    private readonly Control _owner;
    private readonly Func<bool> _isEnabled;
    private readonly Func<Control?, bool> _canStartDrag;
    private readonly Func<Control?, WindowDragDockScope> _getDockScope;
    private readonly bool _handlePointerPressed;
    private bool _handledPointerPressed;
    private Point _dragStartPoint;
    private bool _pointerPressed;
    private bool _isDragging;
    private WindowDragDockScope _dockScope = WindowDragDockScope.FullWindow;
    private PointerPressedEventArgs? _lastPointerPressedArgs;
    private Window? _dragWindow;
    private EventHandler<PixelPointEventArgs>? _positionChangedHandler;
    private IDisposable[]? _disposables;
    private IDisposable? _releasedEventDisposable;

    public WindowDragHelper(
        Control owner,
        Func<bool> isEnabled,
        Func<Control?, bool> canStartDrag,
        bool handlePointerPressed = true,
        Func<Control?, WindowDragDockScope>? getDockScope = null)
    {
        _owner = owner;
        _isEnabled = isEnabled;
        _canStartDrag = canStartDrag;
        _handlePointerPressed = handlePointerPressed;
        _getDockScope = getDockScope ?? (_ => WindowDragDockScope.FullWindow);
    }

    public void Attach()
    {
        Detach();
        
        _disposables =
        [
            _owner.AddDisposableHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel),
            _owner.AddDisposableHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel)
        ];
    }

    public void Detach()
    {
        if (_disposables != null)
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables = null;
        }

        _releasedEventDisposable?.Dispose();
        _releasedEventDisposable = null;

        if (_dragWindow is not null && _positionChangedHandler is not null)
        {
            _dragWindow.PositionChanged -= _positionChangedHandler;
            _positionChangedHandler = null;
        }

        _dragWindow = null;
        _pointerPressed = false;
        _isDragging = false;
        _handledPointerPressed = false;
        _dockScope = WindowDragDockScope.FullWindow;
        _lastPointerPressedArgs = null;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_isEnabled())
        {
            return;
        }

        _lastPointerPressedArgs = e;
        _handledPointerPressed = false;

        if (!e.GetCurrentPoint(_owner).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Control;
        if (_canStartDrag(source))
        {
            _dragStartPoint = e.GetPosition(_owner);
            _pointerPressed = true;
            _dockScope = _getDockScope(source);
            if (_handlePointerPressed)
            {
                e.Handled = true;
                _handledPointerPressed = true;
            }
            else
            {
                _handledPointerPressed = false;
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _releasedEventDisposable?.Dispose();
        _releasedEventDisposable = null;
        
        if (!_pointerPressed && !_isDragging)
        {
            return;
        }

        var shouldHandleRelease = _isDragging || _handledPointerPressed;

        _pointerPressed = false;
        _isDragging = false;
        _handledPointerPressed = false;

        if (_dragWindow is not null)
        {
            if (_positionChangedHandler is not null)
            {
                _dragWindow.PositionChanged -= _positionChangedHandler;
                _positionChangedHandler = null;
            }

            if (_dragWindow is HostWindow hostWindow)
            {
                hostWindow.WindowDragDockScope = WindowDragDockScope.FullWindow;

                if (hostWindow.HostWindowState is HostWindowState state)
                {
                    var point = hostWindow.PointToScreen(e.GetPosition(hostWindow)) -
                                hostWindow.PointToScreen(new Point(0, 0));
                    state.Process(new PixelPoint(point.X, point.Y), EventType.Released);
                }

                hostWindow.Window?.Factory?.OnWindowMoveDragEnd(hostWindow.Window);
            }
        }

        _dragWindow = null;
        _dockScope = WindowDragDockScope.FullWindow;

        if (shouldHandleRelease)
        {
            e.Handled = true;
        }
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



        if (_lastPointerPressedArgs is null)
        {
            return;
        }

        var root = _owner.GetVisualRoot();

        if (root is not HostWindow hostWindow)
        {
            if (root is Window window)
            {
                if (DockSettings.BringWindowsToFrontOnDrag && _owner.DataContext is IDockWindow { Factory: { } dockWindowFactory })
                {
                    WindowActivationHelper.ActivateAllWindows(dockWindowFactory, _owner);
                }

                _releasedEventDisposable?.Dispose();
                _releasedEventDisposable = SubscribeToPointerReleased(window);
                window.BeginMoveDrag(_lastPointerPressedArgs);
                
                _pointerPressed = false;
                _isDragging = false;
                e.Handled = true;
            }
            return;
        }

        _isDragging = true;
        _pointerPressed = false;

        var dockWindow = hostWindow.Window;
        if (dockWindow?.Factory?.OnWindowMoveDragBegin(dockWindow) != true)
        {
            _isDragging = false;
            return;
        }

        if (DockSettings.BringWindowsToFrontOnDrag && dockWindow.Factory is { } factory)
        {
            WindowActivationHelper.ActivateAllWindows(factory, hostWindow);
        }

        if (hostWindow.HostWindowState is HostWindowState state)
        {
            var start = hostWindow.PointToScreen(_lastPointerPressedArgs.GetPosition(hostWindow)) - hostWindow.PointToScreen(new Point(0, 0));
            state.Process(new PixelPoint(start.X, start.Y), EventType.Pressed);
        }

        _dragWindow = hostWindow;
        hostWindow.WindowDragDockScope = _dockScope;

        _positionChangedHandler = (_, _) =>
        {
            if (hostWindow.Window is { } dw)
            {
                dw.Factory?.OnWindowMoveDrag(dw);
            }

            if (hostWindow.HostWindowState is HostWindowState st)
            {
                st.Process(_dragWindow.Position, EventType.Moved);
            }
        };

        hostWindow.PositionChanged += _positionChangedHandler;
        
        _releasedEventDisposable?.Dispose();
        _releasedEventDisposable = SubscribeToPointerReleased(hostWindow);
        hostWindow.BeginMoveDrag(_lastPointerPressedArgs);
        e.Handled = true;
    }

    private IDisposable SubscribeToPointerReleased(Window window)
    {
        return window.AddDisposableHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
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
