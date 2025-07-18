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
using Dock.Model.Controls;
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
        var isLocalValid = ValidateLocal(point, DockOperation.Fill, dragAction, relativeTo);
        var isGlobalValid = ValidateGlobal(point, DockOperation.Fill, dragAction, relativeTo);

        AddAdorners(isLocalValid, isGlobalValid);
    }

    private void Over(Point point, DragAction dragAction, Control dropControl, Visual relativeTo)
    {
        var localOperation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        if (globalOperation != DockOperation.None)
        {
            ValidateGlobal(point, localOperation, dragAction, relativeTo);
        }
        else
        {
            ValidateLocal(point, localOperation, dragAction, relativeTo);
        }
    }

    private void Drop(Point point, DragAction dragAction, Control dropControl, Visual relativeTo)
    {
        var localOperation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        RemoveAdorners();

        if (_context.DragControl is null || DropControl is null)
        {
            return;
        }

        if (globalOperation != DockOperation.None)
        {
            if (DropControl is not { } dropCtrl)
            {
                return;
            }

            var dockControl = dropCtrl.FindAncestorOfType<DockControl>();
            if (dockControl is null)
            {
                return;
            }

            if (_context.DragControl.DataContext is IDockable sourceDockable
                && dockControl.Layout is { } dockControlLayout
                && dockControlLayout.ActiveDockable is IDock dockControlActiveDock)
            {
                var targetDock = DockHelpers.FindProportionalDock(dockControlActiveDock) ?? dockControlActiveDock;
                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, targetDock);
            }
        }
        else
        {
            if (_context.DragControl.DataContext is IDockable sourceDockable &&
                DropControl.DataContext is IDockable targetDockable)
            {
                Execute(point, localOperation, dragAction, relativeTo, sourceDockable, targetDockable);
            }
        }
    }

    private void Leave()
    {
        RemoveAdorners();
    }

    private bool ValidateLocal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            return false;
        }

        return ValidateDockable(point, operation, dragAction, relativeTo, sourceDockable);
    }

    private bool ValidateGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (!DockSettings.EnableGlobalDocking)
        {
            return false;
        }

        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            return false;
        }

        if (DropControl is not { } dropCtrl)
        {
            return false;
        }

        var dockControl = dropCtrl.FindAncestorOfType<DockControl>();
        if (dockControl?.Layout is not { ActiveDockable: IDock dock })
        {
            return false;
        }

        DockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return false;
        }

        var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
        DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        return DockManager.ValidateDockable(sourceDockable, dock, dragAction, operation, bExecute: false);
    }

    private bool IsDockTargetVisible(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            return true;
        }

        if (DropControl?.DataContext is not IDockable targetDockable)
        {
            return true;
        }

        return DockManager.IsDockTargetVisible(sourceDockable, targetDockable, operation);
    }

    protected override void Execute(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo, IDockable sourceDockable, IDockable targetDockable)
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

        base.Execute(point, operation, dragAction, relativeTo, sourceDockable, targetDockable);
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
                            Drop(_context.TargetPoint, dragAction, dropControl, _context.TargetDockControl);
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
                    var haveMinimumDragDistance = DockSettings.IsMinimumDragDistance(diff);
                    if (haveMinimumDragDistance)
                    {
                        if (_context.DragControl?.DataContext is IDockable targetDockable)
                        {
                            DockHelpers.ShowWindows(targetDockable);
                            var sp = inputActiveDockControl.PointToScreen(point);

                            _context.DragOffset = DragOffsetCalculator.CalculateOffset(
                                _context.DragControl, inputActiveDockControl, _context.DragStartPoint);

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
                                Over(targetPoint, dragAction, dropControl, targetDockControl);
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
                                ? globalDockTarget.GetDockOperation(targetPoint, dropControl, targetDockControl, dragAction, ValidateGlobal, IsDockTargetVisible)
                                : DockOperation.None;

                            var localOperation = LocalAdornerHelper.Adorner is DockTarget dockTarget
                                ? dockTarget.GetDockOperation(targetPoint, dropControl, targetDockControl, dragAction, ValidateLocal, IsDockTargetVisible)
                                : DockOperation.Fill;

                            if (globalOperation != DockOperation.None)
                            {
                                var valid = ValidateGlobal(targetPoint, localOperation, dragAction, targetDockControl);
                                preview = valid ? "Dock" : "None";
                            }
                            else
                            {
                                var valid = ValidateLocal(targetPoint, localOperation, dragAction, targetDockControl);
                                preview = valid
                                    ? localOperation == DockOperation.Window ? "Float" : "Dock"
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
