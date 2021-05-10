using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal
{
    /// <summary>
    /// Host window state.
    /// </summary>
    internal class HostWindowState : IHostWindowState
    {
        private readonly AdornerHelper _adornerHelper = new AdornerHelper();
        private readonly HostWindow _hostWindow;
        private Point _dragStartPoint;
        private bool _pointerPressed;
        private bool _doDragDrop;
        private DockControl? _targetDockControl;
        private Point _targetPoint;
        private IControl? _targetDropControl;
        private DragAction _dragAction;

        /// <inheritdoc/>
        public IDockManager DockManager { get; set; }

        public HostWindowState(IDockManager dockManager, HostWindow hostWindow)
        {
            DockManager = dockManager;
            _hostWindow = hostWindow;
        }

        private void Enter(Point point, DragAction dragAction, IVisual relativeTo)
        {
            var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

            if (isValid && _targetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
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

            if (_targetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.RemoveAdorner(control);
            }

            if (operation != DockOperation.Window)
            {
                Execute(point, operation, dragAction, relativeTo);
            }
        }

        private void Leave()
        {
            if (_targetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.RemoveAdorner(control);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_targetDropControl is null)
            {
                return false;
            }

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is { } sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                DockManager.Position = DockHelpers.ToDockPoint(point);

                if (relativeTo.VisualRoot is null)
                {
                    return false;
                }
                var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
                DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);
                
                return DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
            }

            return false;
        }

        private void Execute(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_targetDropControl is null)
            {
                return;
            }

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is { } sourceDockable && _targetDropControl.DataContext is IDockable targetDockable)
            {
                DockManager.Position = DockHelpers.ToDockPoint(point);

                if (relativeTo.VisualRoot is null)
                {
                    return;
                }
                var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
                DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

                DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
            }
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
                        var isDragEnabled = _hostWindow.GetValue(DockProperties.IsDragEnabledProperty);
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
                        if (_doDragDrop)
                        {
                            if (_targetDockControl is { } && _targetDropControl is { })
                            {
                                var isDropEnabled = true;

                                if (_targetDockControl is IControl targetControl)
                                {
                                    isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                                }

                                if (isDropEnabled)
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
                            var haveMinimumDragDistance = IsMinimumDragDistance(diff);
                            if (haveMinimumDragDistance)
                            {
                                _doDragDrop = true;
                            }
                        }

                        if (_doDragDrop && _hostWindow.Window?.Layout?.Factory is { } factory)
                        {
                            foreach (var visual in factory.DockControls)
                            {
                                if (visual is DockControl dockControl && dockControl.Layout != _hostWindow.Window?.Layout)
                                {
                                    var position = point + _dragStartPoint;
                                    var screenPoint = new PixelPoint((int)position.X, (int)position.Y);
                                    if (dockControl.GetVisualRoot() is null)
                                    {
                                        continue;
                                    }
                                    var dockControlPoint = dockControl.PointToClient(screenPoint);
                                    var dropControl = DockHelpers.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                                    if (dropControl is { })
                                    {
                                        var isDropEnabled = dockControl.GetValue(DockProperties.IsDropEnabledProperty);
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
                                                if (_targetDropControl is { })
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
