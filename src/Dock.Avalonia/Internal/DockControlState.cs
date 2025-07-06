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

internal class DockDragContext
{
    public Control? DragControl { get; set; }
    public Point DragStartPoint { get; set; }
    public bool PointerPressed { get; set; }
    public bool DoDragDrop { get; set; }
    public Point TargetPoint { get; set; }
    public Visual? TargetDockControl { get; set; }
    
    public PixelPoint DragOffset { get; set; }

    public void Start(Control dragControl, Point point)
    {
        DragControl = dragControl;
        DragStartPoint = point;
        PointerPressed = true;
        DoDragDrop = false;
        TargetPoint = default;
        TargetDockControl = null;
    }

    public void End()
    {
        DragControl = null;
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
internal class DockControlState : DockManagerState, IDockControlState
{
    private readonly DockDragContext _context = new();
    private readonly DragPreviewHelper _dragPreviewHelper = new();

    public IDragOffsetCalculator DragOffsetCalculator { get; set; }

    public DockControlState(IDockManager dockManager, IDragOffsetCalculator dragOffsetCalculator)
        : base(dockManager)
    {
        DragOffsetCalculator = dragOffsetCalculator;
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);

        AddAdorners(isValid);
    }

    private void Over(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            operation = dockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (globalOperation != DockOperation.None)
        {
            // TODO: Handle global dock target operation
        }
        else
        {
            Validate(point, operation, dragAction, relativeTo);
        }
    }

    private void Drop(Point point, DragAction dragAction, Visual relativeTo)
    {
        var operation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            operation = dockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            // TODO: Handle global dock target operation
            globalOperation = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, Validate);
        }

        RemoveAdorners();

        if (_context.DragControl is null || DropControl is null)
        {
            return;
        }

        if (globalOperation != DockOperation.None)
        {
            if (DropControl is not { } dropControl)
            {
                return;
            }
            
            var dockControl = dropControl.FindAncestorOfType<DockControl>();
            if (dockControl is null)
            {
                return;
            }

            if (_context.DragControl.DataContext is IDockable sourceDockable 
                && dockControl.Layout is { } dockControlLayout 
                && dockControlLayout.ActiveDockable is IDock dockControlActiveDock)
            {
                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, dockControlActiveDock);
            }
        }
        else
        {
            if (_context.DragControl.DataContext is IDockable sourceDockable &&
                DropControl.DataContext is IDockable targetDockable)
            {
                Execute(point, operation, dragAction, relativeTo, sourceDockable, targetDockable);
            }
        }
    }

    private void Leave()
    {
        RemoveAdorners();
    }

    private bool Validate(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_context.DragControl is null || DropControl is null)
        {
            return false;
        }

        if (_context.DragControl.DataContext is IDockable sourceDockable && DropControl.DataContext is IDockable targetDockable)
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

    private void Execute(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable, IDockable targetDockable)
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

                    _context.Start(dragControl, point);
                    DropControl = null;
                    activeDockControl.IsDraggingDock = true;
                }
                break;
            }
            case EventType.Released:
            {
                if (_context.DoDragDrop)
                {
                    var executed = false;

                    if (DropControl is { } dropControl && _context.TargetDockControl is { })
                    {
                        var isDropEnabled = dropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        if (isDropEnabled)
                        {
                            Drop(_context.TargetPoint, dragAction, _context.TargetDockControl);
                            executed = true;
                        }
                    }

                    if (!executed && _context.DragControl?.DataContext is IDockable dockable &&
                        inputActiveDockControl.Layout?.Factory is { } factory)
                    {
                        Float(point, inputActiveDockControl, dockable, factory);
                    }
                }

                _dragPreviewHelper.Hide();

                Leave();
                _context.End();
                DropControl = null;
                activeDockControl.IsDraggingDock = false;
                break;
            }
            case EventType.Moved:
            {
                if (_context.PointerPressed == false)
                {
                    break;
                }

                if (_context.DoDragDrop == false)
                {
                    Vector diff = _context.DragStartPoint - point;
                    var haveMinimumDragDistance = IsMinimumDragDistance(diff);
                    if (haveMinimumDragDistance)
                    {
                        if (_context.DragControl?.DataContext is IDockable targetDockable)
                        {
                            DockHelpers.ShowWindows(targetDockable);
                            var sp = inputActiveDockControl.PointToScreen(point);

                            _context.DragOffset = DragOffsetCalculator.CalculateOffset(
                                _context.DragControl, inputActiveDockControl, point);

                            _dragPreviewHelper.Show(targetDockable, sp, _context.DragOffset);
                        }
                        _context.DoDragDrop = true;
                    }
                }

                if (_context.DoDragDrop)
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
                            if (DropControl == dropControl)
                            {
                                _context.TargetPoint = targetPoint;
                                _context.TargetDockControl = targetDockControl;
                                Over(targetPoint, dragAction, targetDockControl);
                            }
                            else
                            {
                                if (DropControl is { })
                                {
                                    Leave();
                                    DropControl = null;
                                }

                                DropControl = dropControl;
                                _context.TargetPoint = targetPoint;
                                _context.TargetDockControl = targetDockControl;
                                Enter(targetPoint, dragAction, targetDockControl);
                            }

                            var globalOperation = GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget
                                ? globalDockTarget.GetDockOperation(targetPoint, targetDockControl, dragAction, Validate)
                                : DockOperation.None;
                            
                            var operation = LocalAdornerHelper.Adorner is DockTarget dockTarget
                                ? dockTarget.GetDockOperation(targetPoint, targetDockControl, dragAction, Validate)
                                : DockOperation.Fill;

                            // TODO: Handle global dock target operation
                            if (globalOperation != DockOperation.None)
                            {
                                preview = "Dock";
                            }
                            else
                            {
                                var valid = Validate(targetPoint, operation, dragAction, targetDockControl);
                                preview = valid
                                    ? operation == DockOperation.Window ? "Float" : "Dock"
                                    : "None";
                            }
                        }
                        else
                        {
                            if (DropControl is { })
                            {
                                Leave();
                                DropControl = null;
                                _context.TargetPoint = default;
                                _context.TargetDockControl = null;
                            }
                            preview = "Float";
                        }
                    }
                    else
                    {
                        Leave();
                        DropControl = null;
                        _context.TargetPoint = default;
                        _context.TargetDockControl = null;
                        preview = "Float";
                    }

                    _dragPreviewHelper.Move(screenPoint, _context.DragOffset, preview);
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
                _context.End();
                DropControl = null;
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
