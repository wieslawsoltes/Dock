using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal
{
    internal class WindowDragState
    {
        public Point DragStartPoint { get; set; }
        public bool PointerPressed { get; set; }
        public bool DoDragDrop { get; set; }
        public DockControl? TargetDockControl { get; set; }
        public Point TargetPoint { get; set; }
        public IControl? TargetDropControl { get; set; }
        public DragAction DragAction { get; set; }

        public void Start(Point point)
        {
            DragStartPoint = point;
            PointerPressed = true;
            DoDragDrop = false;
            TargetDockControl = null;
            TargetPoint = default;
            TargetDropControl = null;
            DragAction = DragAction.Move;
        }

        public void End()
        {
            DragStartPoint = default;
            PointerPressed = false;
            DoDragDrop = false;
            TargetDockControl = null;
            TargetPoint = default;
            TargetDropControl = null;
            DragAction = DragAction.Move;
        }
    }
    
    /// <summary>
    /// Host window state.
    /// </summary>
    internal class HostWindowState : IHostWindowState
    {
        private readonly AdornerHelper _adornerHelper = new AdornerHelper();
        private readonly HostWindow _hostWindow;
        private readonly WindowDragState _state = new();

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

            if (isValid && _state.TargetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
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

            if (_state.TargetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
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
            if (_state.TargetDropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.RemoveAdorner(control);
            }
        }

        private bool Validate(Point point, DockOperation operation, DragAction dragAction, IVisual relativeTo)
        {
            if (_state.TargetDropControl is null)
            {
                return false;
            }

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is { } sourceDockable && _state.TargetDropControl.DataContext is IDockable targetDockable)
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
            if (_state.TargetDropControl is null)
            {
                return;
            }

            var layout = _hostWindow.Window?.Layout;

            if (layout?.ActiveDockable is { } sourceDockable && _state.TargetDropControl.DataContext is IDockable targetDockable)
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
                    _state.Start(point);
                    break;
                }
                case EventType.Released:
                {
                    if (_state.DoDragDrop)
                    {
                        if (_state.TargetDockControl is { } && _state.TargetDropControl is { })
                        {
                            var isDropEnabled = true;

                            if (_state.TargetDockControl is IControl targetControl)
                            {
                                isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                            }

                            if (isDropEnabled)
                            {
                                Drop(_state.TargetPoint, _state.DragAction, _state.TargetDockControl);
                            }
                        } 
                    }
                    Leave();
                    _state.End();
                    break;
                }
                case EventType.Moved:
                {
                    if (_state.PointerPressed == false)
                    {
                        break;
                    }

                    if (_state.DoDragDrop == false)
                    {
                        Vector diff = _state.DragStartPoint - point;
                        var haveMinimumDragDistance = IsMinimumDragDistance(diff);
                        if (haveMinimumDragDistance)
                        {
                            _state.DoDragDrop = true;
                        }
                    }

                    if (!_state.DoDragDrop || _hostWindow.Window?.Layout?.Factory is not { } factory)
                    {
                        break;
                    }

                    foreach (var visual in factory.DockControls)
                    {
                        if (visual is not DockControl dockControl || dockControl.Layout == _hostWindow.Window?.Layout)
                        {
                            continue;
                        }

                        var position = point + _state.DragStartPoint;
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
                            if (!isDropEnabled)
                            {
                                Leave();
                                _state.TargetDockControl = null;
                                _state.TargetPoint = default;
                                _state.TargetDropControl = null;
                            }
                            else
                            {
                                if (_state.TargetDropControl == dropControl)
                                {
                                    _state.TargetDockControl = dockControl;
                                    _state.TargetPoint = dockControlPoint;
                                    _state.TargetDropControl = dropControl;
                                    _state.DragAction = DragAction.Move;
                                    Over(_state.TargetPoint, _state.DragAction, _state.TargetDockControl);
                                    break;
                                }

                                if (_state.TargetDropControl is { })
                                {
                                    Leave();
                                    _state.TargetDropControl = null;
                                }

                                _state.TargetDockControl = dockControl;
                                _state.TargetPoint = dockControlPoint;
                                _state.TargetDropControl = dropControl;
                                _state.DragAction = DragAction.Move;
                                Enter(_state.TargetPoint, _state.DragAction, _state.TargetDockControl);
                                break;
                            }
                        }
                        else
                        {
                            Leave();
                            _state.TargetDockControl = null;
                            _state.TargetPoint = default;
                            _state.TargetDropControl = null;
                            _state.DragAction = DragAction.Move;
                            break;
                        }
                    }

                    break;
                }
                case EventType.Enter:
                {
                    break;
                }
                case EventType.Leave:
                {
                    break;
                }
                case EventType.CaptureLost:
                {
                    _state.End();
                    break;
                }
                case EventType.WheelChanged:
                {
                    break;
                }
            }
        }
    }
}
