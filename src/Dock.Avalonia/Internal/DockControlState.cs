using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal
{
    /// <summary>
    /// Dock control state.
    /// </summary>
    internal class DockControlState : IDockControlState
    {
        private readonly AdornerHelper _adornerHelper = new();
        private IControl? _dragControl;
        private IControl? _dropControl;
        private Point _dragStartPoint;
        private bool _pointerPressed;
        private bool _doDragDrop;
        private Point _targetPoint;
        private IVisual? _targetDockControl;

        /// <inheritdoc/>
        public IDockManager DockManager { get; set; }

        public DockControlState(IDockManager dockManager)
        {
            DockManager = dockManager;
        }

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (isValid && _dropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Enter] {control}");
                _adornerHelper.AddAdorner(control);
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

            if (_dropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Drop] {control}");
                _adornerHelper.RemoveAdorner(control);
            }

            Execute(point, operation, dragAction, relativeTo);
        }

        private void Leave()
        {
            if (_dropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                Debug.WriteLine($"[Leave] {control}");
                _adornerHelper.RemoveAdorner(control);
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
                DockManager.Position = DockHelpers.ToDockPoint(point);
                DockManager.ScreenPosition = DockHelpers.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                return DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
            }

            return false;
        }

        private void Execute(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_dragControl == null || _dropControl == null)
            {
                return;
            }

            if (_dragControl.DataContext is IDockable sourceDockable && _dropControl.DataContext is IDockable targetDockable)
            {
                Debug.WriteLine($"Execute : {point} : {operation} : {dragAction} : {sourceDockable.Title} -> {targetDockable.Title}");
                DockManager.Position = DockHelpers.ToDockPoint(point);
                DockManager.ScreenPosition = DockHelpers.ToDockPoint(relativeTo.PointToScreen(point).ToPoint(1.0));
                DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, true);
            }
        }

        private static bool IsMinimumDragDistance(Vector diff)
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
        public void Process(Point point, Vector delta, EventType eventType, DragAction dragAction, IVisual activeDockControl, IList<IDockControl> dockControls)
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
                            var isDragEnabled = dragControl.GetValue(DockProperties.IsDragEnabledProperty);
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
                        }
                    }
                    break;
                case EventType.Released:
                    {
                        if (_doDragDrop)
                        {
                            if (_dropControl != null && _targetDockControl != null)
                            {
                                var isDropEnabled = true;

                                if (_targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                if (isDropEnabled)
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
                            var haveMinimumDragDistance = IsMinimumDragDistance(diff);
                            if (haveMinimumDragDistance)
                            {
                                if (_dragControl?.DataContext is IDockable targetDockable)
                                {
                                    DockHelpers.ShowWindows(targetDockable);
                                }
                                _doDragDrop = true;
                            }
                        }

                        if (_doDragDrop)
                        {
                            Point targetPoint = default;
                            IVisual? targetDockControl = null;
                            IControl? dropControl = null;

                            foreach (var dockControl in dockControls)
                            {
                                if (dockControl is IInputElement inputDockControl && inputDockControl != inputActiveDockControl)
                                {
                                    var screenPoint = inputActiveDockControl.PointToScreen(point);
                                    var dockControlPoint = inputDockControl.PointToClient(screenPoint);
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
                                var isDropEnabled = true;

                                if (targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                Debug.WriteLine($"Drop : {targetPoint} : {eventType} : {dropControl.Name} : {dropControl.GetType().Name} : {dropControl.DataContext?.GetType().Name}");

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
