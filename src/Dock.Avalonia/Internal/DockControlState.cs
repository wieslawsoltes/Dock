using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal class DockDragState
{
    public Control? DragControl { get; set; }
    public Control? DropControl { get; set; }
    public Point DragStartPoint { get; set; }
    public bool PointerPressed { get; set; }
    public bool DoDragDrop { get; set; }
    public Point TargetPoint { get; set; }
    public Visual? TargetDockControl { get; set; }

    public void Start(Control dragControl, Point point)
    {
        DragControl = dragControl;
        DropControl = null;
        DragStartPoint = point;
        PointerPressed = true;
        DoDragDrop = false;
        TargetPoint = default;
        TargetDockControl = null;
    }

    public void End()
    {
        DragControl = null;
        DropControl = null;
        DragStartPoint = default;
        PointerPressed = false;
        DoDragDrop = false;
        TargetPoint = default;
        TargetDockControl = null;
    }
}

/// <summary>
/// Dock control state.
/// </summary>
internal class DockControlState : IDockControlState
{
    private readonly AdornerHelper _adornerHelper = new();
    private readonly DockDragState _state = new();

    /// <inheritdoc/>
    public IDockManager DockManager { get; set; }

    public DockControlState(IDockManager dockManager)
    {
        DockManager = dockManager;
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);
        if (isValid && _state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _adornerHelper.AddAdorner(control);
        }
    }

    private void Over(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Fill;

        if (_adornerHelper.Adorner is DockTarget target)
        {
            operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        Validate(point, operation, dragAction, relativeTo);
    }

    private void Drop(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Window;

        if (_adornerHelper.Adorner is DockTarget target)
        {
            operation = target.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (_state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _adornerHelper.RemoveAdorner(control);
        }

        Execute(point, operation, dragAction, relativeTo);
    }

    private void Leave()
    {
        if (_state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _adornerHelper.RemoveAdorner(control);
        }
    }

    private bool Validate(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_state.DragControl is null || _state.DropControl is null)
        {
            return false;
        }

        if (_state.DragControl.DataContext is IDockable sourceDockable && _state.DropControl.DataContext is IDockable targetDockable)
        {
            DockManager.Position = DockHelpers.ToDockPoint(point);

            if (relativeTo.GetVisualRoot() is null)
            {
                return false;
            }

            var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
            DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

            return DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: false);
        }

        return false;
    }

    private void Execute(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_state.DragControl is null || _state.DropControl is null)
        {
            return;
        }

        if (_state.DragControl.DataContext is IDockable sourceDockable && _state.DropControl.DataContext is IDockable targetDockable)
        {
            if (sourceDockable is IDock dock)
            {
                sourceDockable = dock.ActiveDockable;
            }

            if (sourceDockable == null)
            {
                return;
            }

            DockManager.Position = DockHelpers.ToDockPoint(point);

            if (relativeTo.GetVisualRoot() is null)
            {
                return;
            }
            var relativePoint = relativeTo.PointToScreen(point).ToPoint(1.0);
            DockManager.ScreenPosition = DockHelpers.ToDockPoint(relativePoint);

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
    public void Process(Point point, Vector delta, EventType eventType, DragAction dragAction, DockControl activeDockControl, IList<IDockControl> dockControls)
    {
        if (activeDockControl is not { } inputActiveDockControl)
        {
            return;
        }

        switch (eventType)
        {
            case EventType.Pressed:
            {
                var dragControl = DockHelpers.GetControl(inputActiveDockControl, point, DockProperties.IsDragAreaProperty);
                if (dragControl is { })
                {
                    var isDragEnabled = dragControl.GetValue(DockProperties.IsDragEnabledProperty);
                    if (!isDragEnabled)
                    {
                        break;
                    }
                    _state.Start(dragControl, point);
                    activeDockControl.IsDraggingDock = true;
                }
                break;
            }
            case EventType.Released:
            {
                if (_state.DoDragDrop)
                {
                    if (_state.DropControl is { } && _state.TargetDockControl is { })
                    {
                        var isDropEnabled = true;

                        if (_state.TargetDockControl is Control targetControl)
                        {
                            isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                        }

                        if (isDropEnabled)
                        {
                            Drop(_state.TargetPoint, dragAction, _state.TargetDockControl);
                        }
                    }
                }
                Leave();
                _state.End();
                activeDockControl.IsDraggingDock = false;
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
                        if (_state.DragControl?.DataContext is IDockable targetDockable)
                        {
                            DockHelpers.ShowWindows(targetDockable);
                        }
                        _state.DoDragDrop = true;
                    }
                }

                if (_state.DoDragDrop)
                {
                    Point targetPoint = default;
                    Visual? targetDockControl = null;
                    Control? dropControl = null;

                    foreach (var inputDockControl in dockControls.GetZOrderedDockControls())
                    {
                        if (inputActiveDockControl.GetVisualRoot() is null)
                        {
                            continue;
                        }
                        var screenPoint = inputActiveDockControl.PointToScreen(point);

                        if (inputDockControl.GetVisualRoot() is null)
                        {
                            continue;
                        }
                        var dockControlPoint = inputDockControl.PointToClient(screenPoint);

                        dropControl = DockHelpers.GetControl(inputDockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                        if (dropControl is { })
                        {
                            targetPoint = dockControlPoint;
                            targetDockControl = inputDockControl;
                            break;
                        }
                    }

                    if (dropControl is null)
                    {
                        dropControl = DockHelpers.GetControl(inputActiveDockControl, point, DockProperties.IsDropAreaProperty);
                        if (dropControl is { })
                        {
                            targetPoint = point;
                            targetDockControl = inputActiveDockControl;
                        }
                    }

                    if (dropControl is { } && targetDockControl is { })
                    {
                        var isDropEnabled = true;

                        if (targetDockControl is Control targetControl)
                        {
                            isDropEnabled = targetControl.GetValue(DockProperties.IsDropEnabledProperty);
                        }

                        if (isDropEnabled)
                        {
                            if (_state.DropControl == dropControl)
                            {
                                _state.TargetPoint = targetPoint;
                                _state.TargetDockControl = targetDockControl;
                                Over(targetPoint, dragAction, targetDockControl);
                            }
                            else
                            {
                                if (_state.DropControl is { })
                                {
                                    Leave();
                                    _state.DropControl = null;
                                }

                                _state.DropControl = dropControl;
                                _state.TargetPoint = targetPoint;
                                _state.TargetDockControl = targetDockControl;
                                Enter(targetPoint, dragAction, targetDockControl);
                            }
                        }
                        else
                        {
                            if (_state.DropControl is { })
                            {
                                Leave();
                                _state.DropControl = null;
                                _state.TargetPoint = default;
                                _state.TargetDockControl = null;
                            }
                        }
                    }
                    else
                    {
                        Leave();
                        _state.DropControl = null;
                        _state.TargetPoint = default;
                        _state.TargetDockControl = null;
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
                Leave();
                _state.End();
                activeDockControl.IsDraggingDock = false;
                break;
            }
            case EventType.WheelChanged:
            {
                break;
            }
        }
    }
}
