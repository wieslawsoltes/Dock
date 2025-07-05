// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Contract;
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
    
    public PixelPoint DragOffset { get; set; }

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
    private readonly AdornerHelper<DockTarget> _localAdornerHelper = new();
    private readonly AdornerHelper<GlobalDockTarget> _globalAdornerHelper = new();
    private readonly DockDragState _state = new();
    private readonly DragPreviewHelper _dragPreviewHelper = new();

    public IDragOffsetCalculator DragOffsetCalculator { get; set; }

    /// <inheritdoc/>
    public IDockManager DockManager { get; set; }

    public DockControlState(IDockManager dockManager, IDragOffsetCalculator? dragOffsetCalculator = null)
    {
        DockManager = dockManager;
        DragOffsetCalculator = dragOffsetCalculator ?? new DefaultDragOffsetCalculator();
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

        // Global dock target
        if (_state.DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                _globalAdornerHelper.AddAdorner(dockControl);
            }
        }

        // Local dock target
        if (isValid && _state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _localAdornerHelper.AddAdorner(control);
        }
    }

    private void Over(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Fill;
        var isGlobal = false;

        if (_globalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            var op = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateGlobal);
            if (op != DockOperation.Window)
            {
                operation = op;
                isGlobal = true;
            }
        }

        if (!isGlobal && _localAdornerHelper.Adorner is DockTarget dockTarget)
        {
            operation = dockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (isGlobal)
        {
            ValidateGlobal(point, operation, dragAction, relativeTo);
        }
        else
        {
            Validate(point, operation, dragAction, relativeTo);
        }
    }

    private void Drop(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Fill;
        var isGlobal = false;

        if (_globalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            var op = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateGlobal);
            if (op != DockOperation.Window)
            {
                operation = op;
                isGlobal = true;
            }
        }

        if (!isGlobal && _localAdornerHelper.Adorner is DockTarget dockTarget)
        {
            operation = dockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (_state.DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                _globalAdornerHelper.RemoveAdorner(dockControl);
            }
        }

        if (_state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _localAdornerHelper.RemoveAdorner(control);
        }

        if (isGlobal)
        {
            ExecuteGlobal(point, operation, dragAction, relativeTo);
        }
        else
        {
            Execute(point, operation, dragAction, relativeTo);
        }
    }

    private void Leave()
    {
        // Global dock target
        if (_state.DropControl is { } dropControl)
        {
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is not null)
            {
                _globalAdornerHelper.RemoveAdorner(dockControl);
            }
        }

        // Local dock target
        if (_state.DropControl is { } control && control.GetValue(DockProperties.IsDockTargetProperty))
        {
            _localAdornerHelper.RemoveAdorner(control);
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
                if (dock.ActiveDockable == null)
                {
                    return;
                }

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

    private bool ValidateGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_state.DragControl is null || _state.DropControl is null)
        {
            return false;
        }

        if (_state.DragControl.DataContext is IDockable sourceDockable &&
            _state.DropControl.DataContext is IDockable targetDockable &&
            targetDockable.Owner is IDock dock && dock.Factory is { } factory &&
            factory.FindRoot(dock, _ => true) is { ActiveDockable: IDockable activeDockable })
        {
            DockManager.Position = DockHelpers.ToDockPoint(point);

            if (relativeTo.GetVisualRoot() is null)
            {
                return false;
            }

            var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
            DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

            return DockManager.ValidateDockable(sourceDockable, activeDockable, dragAction, operation, bExecute: false);
        }

        return false;
    }

    private void ExecuteGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_state.DragControl is null || _state.DropControl is null)
        {
            return;
        }

        if (_state.DragControl.DataContext is IDockable sourceDockable &&
            _state.DropControl.DataContext is IDockable targetDockable &&
            targetDockable.Owner is IDock dock && dock.Factory is { } factory &&
            factory.FindRoot(dock, _ => true) is { ActiveDockable: IDockable activeDockable })
        {
            DockManager.Position = DockHelpers.ToDockPoint(point);

            if (relativeTo.GetVisualRoot() is null)
            {
                return;
            }

            var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
            DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

            DockManager.ValidateDockable(sourceDockable, activeDockable, dragAction, operation, true);
        }
    }

    private static bool IsMinimumDragDistance(Vector diff)
    {
        return (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance
                || Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance);
    }

    private static void Float(Point point, DockControl inputActiveDockControl, IDockable dockable, IFactory factory)
    {
        var screen = inputActiveDockControl.PointToScreen(point);
        dockable.SetPointerScreenPosition(screen.X, screen.Y);
        factory.FloatDockable(dockable);
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
                    
                    if (dragControl.DataContext is IDockable { CanDrag: false })
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
                    var executed = false;

                    if (_state.DropControl is { } dropControl && _state.TargetDockControl is { })
                    {
                        var isDropEnabled = dropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        if (isDropEnabled)
                        {
                            Drop(_state.TargetPoint, dragAction, _state.TargetDockControl);
                            executed = true;
                        }
                    }

                    if (!executed && _state.DragControl?.DataContext is IDockable dockable &&
                        inputActiveDockControl.Layout?.Factory is { } factory)
                    {
                        Float(point, inputActiveDockControl, dockable, factory);
                    }
                }

                _dragPreviewHelper.Hide();

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
                            var sp = inputActiveDockControl.PointToScreen(point);

                            _state.DragOffset = DragOffsetCalculator.CalculateOffset(
                                _state.DragControl, inputActiveDockControl, point);

                            _dragPreviewHelper.Show(targetDockable, sp, _state.DragOffset);
                        }
                        _state.DoDragDrop = true;
                    }
                }

                if (_state.DoDragDrop)
                {
                    Point targetPoint = default;
                    Visual? targetDockControl = null;
                    Control? dropControl = null;

                    var screenPoint = inputActiveDockControl.PointToScreen(point);
                    var preview = "None";

                    foreach (var inputDockControl in dockControls.GetZOrderedDockControls())
                    {
                        if (inputActiveDockControl.GetVisualRoot() is null)
                        {
                            continue;
                        }

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
                        var isDropEnabled = dropControl.GetValue(DockProperties.IsDropEnabledProperty);
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

                            var globalOperation = _globalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget
                                ? globalDockTarget.GetDockOperation(targetPoint, targetDockControl, dragAction, ValidateGlobal)
                                : DockOperation.Window;

                            var operation = _localAdornerHelper.Adorner is DockTarget dockTarget
                                ? dockTarget.GetDockOperation(targetPoint, targetDockControl, dragAction, Validate)
                                : DockOperation.Fill;

                            var useGlobal = globalOperation != DockOperation.Window;
                            var valid = useGlobal
                                ? ValidateGlobal(targetPoint, globalOperation, dragAction, targetDockControl)
                                : Validate(targetPoint, operation, dragAction, targetDockControl);

                            var effectiveOperation = useGlobal ? globalOperation : operation;
                            preview = valid
                                ? effectiveOperation == DockOperation.Window ? "Float" : "Dock"
                                : "None";
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
                            preview = "Float";
                        }
                    }
                    else
                    {
                        Leave();
                        _state.DropControl = null;
                        _state.TargetPoint = default;
                        _state.TargetDockControl = null;
                        preview = "Float";
                    }

                    _dragPreviewHelper.Move(screenPoint, _state.DragOffset, preview);
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
                _dragPreviewHelper.Hide();
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
