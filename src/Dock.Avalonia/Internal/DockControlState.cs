// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Contract;
using Dock.Model.Controls;
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

    public void StartDrag(Control dragControl, Point startPoint, Point point, DockControl activeDockControl)
    {
        if (!dragControl.GetValue(DockProperties.IsDragEnabledProperty))
        {
            return;
        }

        if (dragControl.DataContext is IDockable { CanDrag: false })
        {
            return;
        }

        _context.Start(dragControl, startPoint);
        DropControl = null;
        activeDockControl.IsDraggingDock = true;

        if (dragControl.DataContext is IDockable targetDockable)
        {
            DockHelpers.ShowWindows(targetDockable);
            var sp = activeDockControl.PointToScreen(point);
            _context.DragOffset = DragOffsetCalculator.CalculateOffset(
                dragControl,
                activeDockControl,
                _context.DragStartPoint);
            _dragPreviewHelper.Show(targetDockable, sp, _context.DragOffset);
            _context.DoDragDrop = true;
        }
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
    
                // TODO: The validation fails in floating window as ActiveDockable is a tool dock.
                // if (!ValidateGlobalTarget(sourceDockable, targetDock))
                // {
                //     return;
                // }

                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, targetDock);

                if (sourceDockable.Owner != null) 
                    sourceDockable.Owner.Proportion = DockSettings.GlobalDockingProportion;
            }
        }
        else
        {
            if (_context.DragControl.DataContext is IDockable sourceDockable &&
                DropControl.DataContext is IDockable targetDockable)
            {
                if (!ValidateLocalTarget(sourceDockable, targetDockable))
                {
                    return;
                }

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

        // For local validation during Enter, we just check if source can do local docking
        // Detailed target validation happens later in ValidateDockable when DropControl is set
        if (DropControl?.DataContext is IDockable targetDockable)
        {
            return ValidateDockable(point, operation, dragAction, relativeTo, sourceDockable);
        }

        // If no specific target yet, allow local adorners if this control is marked as a dock target
        return DropControl?.GetValue(DockProperties.IsDockTargetProperty) == true;
    }

    private bool ValidateGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            return false;
        }

        if (DropControl is not { } dropCtrl)
        {
            return false;
        }

        var dockControl = dropCtrl.FindAncestorOfType<DockControl>();
        if (dockControl?.Layout is not { ActiveDockable: IDock activeDock })
        {
            return false;
        }

        // Use the same target dock as execution for consistency
        var targetDock = DockHelpers.FindProportionalDock(activeDock) ?? activeDock;

        // Check if the target dock (or any ancestor) has global docking enabled
        if (!DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock))
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

        // Check docking groups for global docking visual validation
        // Global adorners should only show when source dockable doesn't have a docking group
        // or when docking groups are compatible
        if (!DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock))
        {
            return false;
        }
        
        return DockManager.ValidateDockable(sourceDockable, targetDock, dragAction, operation, bExecute: false);
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
                        dockable.CanFloat &&
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
                        activeDockControl.IsDraggingDock = true;
                    }
                }

                if (_context.DoDragDrop)
                {
                    Point targetPoint = default;
                    Visual? targetDockControl = null;
                    Control? dropControl = null;

                    var screenPoint = inputActiveDockControl.PointToScreen(point);
                    var preview = "None";

                    foreach (var inputDockControl in DockHelpers.GetZOrderedDockControls(dockControls))
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

                            bool? debugIsValid = null;
                            string? debugReason = null;
                            var debugOperation = DockOperation.None;
                            bool? debugIsGlobal = null;

                            if (globalOperation != DockOperation.None)
                            {
                                var valid = ValidateGlobal(targetPoint, localOperation, dragAction, targetDockControl);
                                preview = valid ? "Dock" : "None";
                                debugIsValid = valid;
                                debugOperation = globalOperation;
                                debugIsGlobal = true;
                                if (!valid && _context.DragControl?.DataContext is IDockable s1 && dropControl.DataContext is IDockable t1)
                                {
                                    if (!DockGroupValidator.ValidateGlobalDocking(s1, t1 as IDock ?? s1.Owner as IDock ?? t1.Owner as IDock ?? s1.Owner as IDock))
                                    {
                                        debugReason = "Global docking not allowed for grouped source";
                                    }
                                }
                            }
                            else
                            {
                                var valid = ValidateLocal(targetPoint, localOperation, dragAction, targetDockControl);
                                preview = valid
                                    ? localOperation == DockOperation.Window ? "Float" : "Dock"
                                    : "None";
                                debugIsValid = valid;
                                debugOperation = localOperation;
                                debugIsGlobal = false;
                                if (!valid && _context.DragControl?.DataContext is IDockable s2 && dropControl.DataContext is IDockable t2)
                                {
                                    if (!DockGroupValidator.ValidateDockingGroups(s2, t2))
                                    {
                                        debugReason = "DockGroup mismatch";
                                    }
                                }
                            }

                            // Write debug state for overlay
                            DockProperties.SetDebugIsValidDrop(dropControl, debugIsValid);
                            DockProperties.SetDebugDropReason(dropControl, debugReason);
                            DockProperties.SetDebugDropOperation(dropControl, debugOperation);
                            DockProperties.SetDebugIsGlobalDrop(dropControl, debugIsGlobal);
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
                        }
                    }
                    else
                    {
                        Leave();
                        DropControl = null;
                        _context.TargetPoint = default;
                        _context.TargetDockControl = null;
                        var canFloat = _context.DragControl?.DataContext is IDockable sourceDockable && sourceDockable.CanFloat;
                        // For debug overlay, clear previous target debug state
                        if (DropControl is { })
                        {
                            DockProperties.SetDebugIsValidDrop(DropControl, null);
                            DockProperties.SetDebugDropReason(DropControl, null);
                            DockProperties.SetDebugDropOperation(DropControl, DockOperation.None);
                            DockProperties.SetDebugIsGlobalDrop(DropControl, null);
                        }
                        preview = canFloat ? "Float" : "None";
                    }

                    // Fallback: if no valid dock target, prefer Float preview only for non-grouped dockables
                    if (preview == "None")
                    {
                        if (_context.DragControl?.DataContext is IDockable sDockable)
                        {
                            var hasGroup = !string.IsNullOrEmpty(DockGroupValidator.GetEffectiveDockGroup(sDockable));
                            if (sDockable.CanFloat && !hasGroup)
                            {
                                preview = "Float";
                            }
                        }
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
