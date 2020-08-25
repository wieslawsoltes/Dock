using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model;

namespace Dock.Avalonia
{
    /// <summary>
    /// Dock control state.
    /// </summary>
    internal class DockControlState
    {
        private readonly IDockManager _dockManager = new DockManager();
        private readonly AdornerHelper _adornerHelper = new AdornerHelper();
        private IControl? _dragControl = null;
        private IControl? _dropControl = null;
        private Point _dragStartPoint = default;
        private bool _pointerPressed = false;
        private bool _doDragDrop = false;
        private Point _targetPoint = default;
        private IVisual? _targetDockControl = null;

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (isValid == true && _dropControl is Panel panel && panel.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Enter] {panel}");
                _adornerHelper.AddAdorner(panel);
            }
        }

        private void Over(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Fill;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            Validate(point, operation, dragAction, relativeTo);
        }

        private void Drop(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var operation = DockOperation.Window;

            if (_adornerHelper.Adorner is DockTarget target)
            {
                operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
            }

            if (_dropControl is Panel panel && panel.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Drop] {panel}");
                _adornerHelper.RemoveAdorner(panel);
            }

            Execute(point, operation, dragAction, relativeTo);
        }

        private void Leave()
        {
            if (_dropControl is Panel panel && panel.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Leave] {panel}");
                _adornerHelper.RemoveAdorner(panel);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_dragControl == null || _dropControl == null)
            {
                return false;
            }

            if (_dragControl.DataContext is IDockable sourceDockable && _dropControl.DataContext is IDockable targetDockable)
            {
                _dockManager.Position = DockHelpers.ToDockPoint(point);
                _dockManager.ScreenPosition = DockHelpers.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return _dockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
            }

            return false;
        }

        private bool Execute(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_dragControl == null || _dropControl == null)
            {
                return false;
            }

            if (_dragControl.DataContext is IDockable sourceDockable && _dropControl.DataContext is IDockable targetDockable)
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
        /// <param name="delta">The mouse wheel delta.</param>
        /// <param name="eventType">The pointer event type.</param>
        /// <param name="dragAction">The input drag action.</param>
        /// <param name="activeDockControl">The active dock control.</param>
        /// <param name="dockControls">The dock controls.</param>
        public void Process(Point point, Vector delta, EventType eventType, DragAction dragAction, IVisual activeDockControl, IList<IVisual> dockControls)
        {
            if (!(activeDockControl is IInputElement inputActiveDockControl))
            {
                return;
            }

            switch (eventType)
            {
                case EventType.Pressed:
                    {
                        var dragControl = DockHelpers.GetControl(inputActiveDockControl, point, DockProperties.IsDragAreaProperty);
                        if (dragControl != null)
                        {
                            bool isDragEnabled = dragControl.GetValue(DockProperties.IsDragEnabledProperty);
                            if (isDragEnabled != true)
                            {
                                break;
                            }
                            Debug.WriteLine($"Drag : {point} : {eventType} : {dragControl.Name} : {dragControl.GetType().Name} : {dragControl.DataContext?.GetType().Name}");
                            _dragControl = dragControl;
                            _dropControl = null;
                            _dragStartPoint = point;
                            _pointerPressed = true;
                            _doDragDrop = false;
                            _targetPoint = default;
                            _targetDockControl = null;
                            break;
                        }
                    }
                    break;
                case EventType.Released:
                    {
                        if (_doDragDrop == true)
                        {
                            if (_dropControl != null && _targetDockControl != null)
                            {
                                bool isDropEnabled = true;

                                if (_targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                if (isDropEnabled == true)
                                {
                                    Drop(_targetPoint, dragAction, _targetDockControl);
                                }
                            }
                            else
                            {
                                // TODO: Create window.
                            }
                        }
                        Leave();
                        _dragControl = null;
                        _dropControl = null;
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _doDragDrop = false;
                        _targetPoint = default;
                        _targetDockControl = null;
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
                                if (_dragControl?.DataContext is IDockable targetDockable)
                                {
                                    DockHelpers.ShowWindows(targetDockable);
                                }
                                _doDragDrop = true;
                            }
                        }

                        if (_doDragDrop == true)
                        {
                            Point targetPoint = default;
                            IVisual? targetDockControl = null;
                            IControl? dropControl = null;

                            foreach (var dockControl in dockControls)
                            {
                                if (dockControl is IInputElement inputDockControl && inputDockControl != inputActiveDockControl)
                                {
                                    var screenPoint = inputActiveDockControl.PointToScreen(point);
                                    var dockControlPoint = dockControl.PointToClient(screenPoint);
                                    if (dockControlPoint == null)
                                    {
                                        continue;
                                    }
                                    dropControl = DockHelpers.GetControl(inputDockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                                    if (dropControl != null)
                                    {
                                        targetPoint = dockControlPoint;
                                        targetDockControl = inputDockControl;
                                        break;
                                    }
                                }
                            }

                            if (dropControl == null)
                            {
                                dropControl = DockHelpers.GetControl(inputActiveDockControl, point, DockProperties.IsDropAreaProperty);
                                if (dropControl != null)
                                {
                                    targetPoint = point;
                                    targetDockControl = inputActiveDockControl;
                                }
                            }

                            if (dropControl != null && targetDockControl != null)
                            {
                                bool isDropEnabled = true;

                                if (targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                //Debug.WriteLine($"Drop : {targetPoint} : {eventType} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");
                                
                                if (isDropEnabled)
                                {
                                    if (_dropControl == dropControl)
                                    {
                                        _targetPoint = targetPoint;
                                        _targetDockControl = targetDockControl;
                                        Over(targetPoint, dragAction, targetDockControl);
                                    }
                                    else
                                    {
                                        if (_dropControl != null)
                                        {
                                            Leave();
                                            _dropControl = null;
                                        }

                                        _dropControl = dropControl;
                                        _targetPoint = targetPoint;
                                        _targetDockControl = targetDockControl;

                                        Enter(targetPoint, dragAction, targetDockControl);
                                    }
                                }
                                else
                                {
                                    if (_dropControl != null)
                                    {
                                        Leave();
                                        _dropControl = null;
                                        _targetPoint = default;
                                        _targetDockControl = null;
                                    }
                                }
                            }
                            else
                            {
                                Leave();
                                _dropControl = null;
                                _targetPoint = default;
                                _targetDockControl = null;
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
                        Leave();
                        _dragControl = null;
                        _dropControl = null;
                        _dragStartPoint = default;
                        _pointerPressed = false;
                        _doDragDrop = false;
                        _targetPoint = default;
                        _targetDockControl = null;
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
