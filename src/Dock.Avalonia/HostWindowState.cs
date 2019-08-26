// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Host window state.
    /// </summary>
    internal class HostWindowState
    {
        private readonly IDockManager _dockManager = new DockManager();
        private readonly AdornerHelper _adornerHelper = new AdornerHelper();
        private readonly HostWindow _hostWindow;
        private bool _pointerPressed = false;
        private Point _pointerPressedPoint = default;
        private DockControl _targetDockControl = null;
        private Point _targetPoint = default;
        private IControl _targetDropControl = null;
        private DragAction _dragAction = default;

        public HostWindowState(HostWindow hostWindow)
        {
            _hostWindow = hostWindow;
        }

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (isValid == true && _targetDropControl is DockPanel)
            {
                _adornerHelper.AddAdorner(_targetDropControl);
            }
        }

        private void Over(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Fill;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            if (operation != DockOperation.Window)
            {
                Validate(point, operation, dragAction, relativeTo);
            }
        }

        private void Drop(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Window;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            if (_targetDropControl is DockPanel)
            {
                _adornerHelper.RemoveAdorner(_targetDropControl);
            }

            if (operation != DockOperation.Window)
            {
                Execute(point, operation, dragAction, relativeTo);
            }
        }

        private void Leave()
        {
            if (_targetDropControl is DockPanel)
            {
                _adornerHelper.RemoveAdorner(_targetDropControl);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_targetDropControl == null)
            {
                return false;
            }

            var layout = _hostWindow.Window.Layout;

            if (layout?.ActiveDockable is IDockable sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                _dockManager.Position = DockControlState.ToDockPoint(point);
                _dockManager.ScreenPosition = DockControlState.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
            }

            return false;
        }

        private bool Execute(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_targetDropControl == null)
            {
                return false;
            }

            var layout = _hostWindow.Window.Layout;

            if (layout?.ActiveDockable is IDockable sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                Debug.WriteLine($"Execute : {point} : {operation} : {dragAction} : {sourceDockable?.Title} -> {targetDockable?.Title}");
                _dockManager.Position = DockControlState.ToDockPoint(point);
                _dockManager.ScreenPosition = DockControlState.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
            }

            return false;
        }

        /// <summary>
        /// Process pointer event.
        /// </summary>
        /// <param name="point">The pointer position.</param>
        /// <param name="eventType">The pointer event type.</param>
        public void Process(Point point, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Pressed:
                    {
                        _pointerPressed = true;
                        _pointerPressedPoint = point;
                        _targetDockControl = null;
                        _targetPoint = default;
                        _targetDropControl = null;
                        _dragAction = DragAction.Move;
                    }
                    break;
                case EventType.Released:
                    {
                        if (_targetDockControl != null)
                        {
                            if (_targetDropControl != null)
                            {
                                Drop(_targetPoint, _dragAction, _targetDockControl);
                            }
                            Leave();
                        }
                        _pointerPressed = false;
                        _pointerPressedPoint = default;
                        _targetDockControl = null;
                        _targetPoint = default;
                        _targetDropControl = null;
                        _dragAction = DragAction.Move;
                    }
                    break;
                case EventType.Moved:
                    {
                        if (_pointerPressed == true)
                        {
                            foreach (var visual in DockControl.s_dockControls)
                            {
                                if (visual is DockControl dockControl)
                                {
                                    if (dockControl.Layout != _hostWindow.Window.Layout)
                                    {
                                        var position = point + _pointerPressedPoint;
                                        var screenPoint = new PixelPoint((int)position.X, (int)position.Y);
                                        var dockControlPoint = dockControl.PointToClient(screenPoint);
                                        if (dockControlPoint == null)
                                        {
                                            continue;
                                        }
                                        var dropControl = DockControlState.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                                        if (dropControl != null)
                                        {
                                            Debug.WriteLine($"Drop : {dockControlPoint} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");
                                            if (_targetDropControl == dropControl)
                                            {
                                                _targetDockControl = dockControl;
                                                _targetPoint = dockControlPoint;
                                                _targetDropControl = dropControl;
                                                _dragAction = DragAction.Move;
                                                Over(_targetPoint, _dragAction, _targetDockControl);
                                                break;
                                            }
                                            else
                                            {
                                                if (_targetDropControl != null)
                                                {
                                                    Leave();
                                                    _targetDropControl = null;
                                                }
                                                _targetDockControl = dockControl;
                                                _targetPoint = dockControlPoint;
                                                _targetDropControl = dropControl;
                                                _dragAction = DragAction.Move;
                                                Enter(_targetPoint, _dragAction, _targetDockControl);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            Leave();
                                            _targetDockControl = null;
                                            _targetPoint = default;
                                            _targetDropControl = null;
                                            _dragAction = DragAction.Move;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case EventType.Enter:
                    {
                    }
                    break;
                case EventType.Leave:
                    {
                    }
                    break;
                case EventType.CaptureLost:
                    {
                        _pointerPressed = false;
                        _pointerPressedPoint = default;
                        _targetDockControl = null;
                        _targetPoint = default;
                        _targetDropControl = null;
                        _dragAction = default;
                    }
                    break;
                case EventType.WheelChanged:
                    {
                    }
                    break;
            }
        }
    }
}
