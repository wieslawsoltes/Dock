using System;
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
        private Point _dragStartPoint = default;
        private bool _pointerPressed = false;
        private bool _doDragDrop = false;
        private DockControl? _targetDockControl = null;
        private Point _targetPoint = default;
        private IControl? _targetDropControl = null;
        private DragAction _dragAction = default;

        public HostWindowState(HostWindow hostWindow)
        {
            _hostWindow = hostWindow;
        }

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (isValid == true && _targetDropControl is Panel)
            {
                Debug.WriteLine($"[Enter] {_targetDropControl}");
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

            if (_targetDropControl is Panel)
            {
                Debug.WriteLine($"[Drop] {_targetDropControl}");
                _adornerHelper.RemoveAdorner(_targetDropControl);
            }

            if (operation != DockOperation.Window)
            {
                Execute(point, operation, dragAction, relativeTo);
            }
        }

        private void Leave()
        {
            if (_targetDropControl is Panel)
            {
                Debug.WriteLine($"[Leave] {_targetDropControl}");
                _adornerHelper.RemoveAdorner(_targetDropControl);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_targetDropControl == null)
            {
                return false;
            }

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is IDockable sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                _dockManager.Position = DockHelpers.ToDockPoint(point);
                _dockManager.ScreenPosition = DockHelpers.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
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

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is IDockable sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                Debug.WriteLine($"Execute : {point} : {operation} : {dragAction} : {sourceDockable.Title} -> {targetDockable.Title}");
                _dockManager.Position = DockHelpers.ToDockPoint(point);
                _dockManager.ScreenPosition = DockHelpers.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
            }

            return false;
        }

        private bool IsMinimumDragDistance(Vector diff)
        {
            return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
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
                        bool isDragEnabled = _hostWindow.GetValue(DockProperties.IsDragEnabledProperty);
                        if (isDragEnabled != true)
                        {
                            break;
                        }
                        _dragStartPoint = point;
                        _pointerPressed = true;
                        _doDragDrop = false;
                        _targetDockControl = null;
                        _targetPoint = default;
                        _targetDropControl = null;
                        _dragAction = DragAction.Move;
                    }
                    break;
                case EventType.Released:
                    {
                        if (_doDragDrop == true)
                        {
                            if (_targetDockControl != null && _targetDropControl != null)
                            {
                                bool isDropEnabled = true;

                                if (_targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                if (isDropEnabled == true)
                                {
                                    Drop(_targetPoint, _dragAction, _targetDockControl);
                                }
                            } 
                        }
                        Leave();
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _doDragDrop = false;
                        _targetDockControl = null;
                        _targetPoint = default;
                        _targetDropControl = null;
                        _dragAction = DragAction.Move;
                    }
                    break;
                case EventType.Moved:
                    {
                        if (_pointerPressed == false)
                        {
                            break;
                        }

                        if (_doDragDrop == false)
                        {
                            Vector diff = _dragStartPoint - point;
                            bool haveMinimumDragDistance = IsMinimumDragDistance(diff);
                            if (haveMinimumDragDistance == true)
                            {
                                _doDragDrop = true;
                            }
                        }

                        if (_doDragDrop == true)
                        {
                            foreach (var visual in DockControl.s_dockControls)
                            {
                                if (visual is DockControl dockControl)
                                {
                                    if (dockControl.Layout != _hostWindow.Window?.Layout)
                                    {
                                        var position = point + _dragStartPoint;
                                        var screenPoint = new PixelPoint((int)position.X, (int)position.Y);
                                        var dockControlPoint = dockControl.PointToClient(screenPoint);
                                        if (dockControlPoint == null)
                                        {
                                            continue;
                                        }
                                        var dropControl = DockHelpers.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                                        if (dropControl != null)
                                        {
                                            bool isDropEnabled = dockControl.GetValue(DockProperties.IsDropEnabledProperty);

                                            Debug.WriteLine($"Drop : {dockControlPoint} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");

                                            if (isDropEnabled)
                                            {
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
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _dragStartPoint = default;
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
