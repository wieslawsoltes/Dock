// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Contract;
using Dock.Model;
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

    public DockControlState(
        IDockManager dockManager,
        IDragOffsetCalculator dragOffsetCalculator,
        IGlobalDockingService? globalDockingService = null)
        : base(dockManager, globalDockingService)
    {
        DragOffsetCalculator = dragOffsetCalculator;
    }

    private static bool CanDragDockable(IDockable dockable)
    {
        return DockCapabilityResolver.IsEnabled(
            dockable,
            DockCapability.Drag,
            DockCapabilityResolver.ResolveOperationDock(dockable));
    }

    private static bool CanFloatDockable(IDockable dockable)
    {
        return DockCapabilityResolver.IsEnabled(
            dockable,
            DockCapability.Float,
            DockCapabilityResolver.ResolveOperationDock(dockable));
    }

    public void StartDrag(Control dragControl, Point startPoint, Point point, DockControl activeDockControl)
    {
        if (!dragControl.GetValue(DockProperties.IsDragEnabledProperty))
        {
            return;
        }

        if (dragControl.DataContext is IDockable dragDockable && !CanDragDockable(dragDockable))
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
            _dragPreviewHelper.Show(targetDockable, sp, _context.DragOffset, activeDockControl);
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
        var hasLocalAdorner = LocalAdornerHelper.Adorner is DockTarget;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        var useGlobalOperation = GlobalDocking.ShouldUseGlobalOperation(
            hasLocalAdorner,
            localOperation,
            globalOperation);
        if (useGlobalOperation)
        {
            ValidateGlobal(point, globalOperation, dragAction, relativeTo);
        }
        else
        {
            ValidateLocal(point, localOperation, dragAction, relativeTo);
        }

        LocalAdornerHelper.SetGlobalDockActive(useGlobalOperation);
    }

    private void Drop(Point point, DragAction dragAction, Control dropControl, Visual relativeTo)
    {
        var localOperation = DockOperation.Fill;
        var globalOperation = DockOperation.None;
        var hasLocalAdorner = LocalAdornerHelper.Adorner is DockTarget;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        var useGlobalOperation = GlobalDocking.ShouldUseGlobalOperation(
            hasLocalAdorner,
            localOperation,
            globalOperation);

        RemoveAdorners();

        if (_context.DragControl is null || DropControl is null)
        {
            return;
        }

        if (useGlobalOperation)
        {
            if (DropControl is not { } dropCtrl)
            {
                return;
            }

            if (_context.DragControl.DataContext is IDockable sourceDockable
                && ResolveGlobalTargetDock(dropCtrl) is { } targetDock)
            {
                var sourceRoot = sourceDockable.Factory?.FindRoot(sourceDockable, _ => true);
                var targetRoot = targetDock.Factory?.FindRoot(targetDock, _ => true);

                // Validate before executing global docking; if validation fails, fall back to floating when possible.
                if (!ValidateGlobal(point, globalOperation, dragAction, relativeTo))
                {
                    if (CanFloatDockable(sourceDockable))
                    {
                        var activeDockControl = _context.DragControl.FindAncestorOfType<DockControl>();
                        var factory = activeDockControl?.Layout?.Factory ?? targetDock.Factory;
                        if (activeDockControl is { } active && factory is { })
                        {
                            var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
                            var screenPixel = new PixelPoint((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y));
                            var activePoint = active.PointToClient(screenPixel);
                            Float(activePoint, active, sourceDockable, factory, _context.DragOffset);
                        }
                     }
                     return;
                 }

                 // TODO: The validation fails in floating window as ActiveDockable is a tool dock.
                 // if (!ValidateGlobalTarget(sourceDockable, targetDock))
                 // {
                 //     return;
                 // }

                 Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, targetDock);

                 GlobalDocking.TryApplyGlobalDockingProportion(
                     sourceDockable,
                     sourceRoot,
                     targetRoot,
                     DockSettings.GlobalDockingProportion);
             }
         }
         else
         {
             if (_context.DragControl.DataContext is IDockable sourceDockable)
             {
                 var target = DropControl.DataContext as IDockable;
                 if (target is null)
                 {
                     return;
                 }

                if (!ValidateLocalTarget(sourceDockable, target))
                {
                    // If local docking target is invalid, fallback to floating if allowed
                    if (CanFloatDockable(sourceDockable))
                    {
                        var activeDockControl = _context.DragControl.FindAncestorOfType<DockControl>();
                        var factory = activeDockControl?.Layout?.Factory ?? DropControl.FindAncestorOfType<DockControl>()?.Layout?.Factory;
                         if (activeDockControl is { } active && factory is { })
                        {
                            var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
                            var screenPixel = new PixelPoint((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y));
                            var activePoint = active.PointToClient(screenPixel);
                            Float(activePoint, active, sourceDockable, factory, _context.DragOffset);
                        }
                     }
                     return;
                 }

                 Execute(point, localOperation, dragAction, relativeTo, sourceDockable, target);
              }
          }
      }

     private void Leave()
     {
         RemoveAdorners();
     }

    private bool ValidateLocal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (!DockManager.IsDockingEnabled)
        {
            LogDropRejection(nameof(ValidateLocal), "Docking is disabled.");
            return false;
        }

        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            LogDropRejection(nameof(ValidateLocal), "DragControl DataContext is not an IDockable.");
            return false;
        }

        // For local validation during Enter, we just check if source can do local docking
        // Detailed target validation happens later in ValidateDockable when DropControl is set
        if (DropControl?.DataContext is IDockable)
        {
            return ValidateDockable(point, operation, dragAction, relativeTo, sourceDockable);
        }

        // If no specific target yet, allow local adorners if this control is marked as a dock target
        var isDockTarget = DropControl?.GetValue(DockProperties.IsDockTargetProperty) == true;
        if (!isDockTarget)
        {
            var controlName = DropControl is null ? "null" : DropControl.GetType().Name;
            LogDropRejection(nameof(ValidateLocal), $"Control '{controlName}' is not marked as a dock target.");
        }

        return isDockTarget;
    }

    private bool ValidateGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (!DockManager.IsDockingEnabled)
        {
            LogDropRejection(nameof(ValidateGlobal), "Docking is disabled.");
            return false;
        }

        if (_context.DragControl?.DataContext is not IDockable sourceDockable)
        {
            LogDropRejection(nameof(ValidateGlobal), "DragControl DataContext is not an IDockable.");
            return false;
        }

        if (DropControl is not { } dropCtrl)
        {
            LogDropRejection(nameof(ValidateGlobal), "No DropControl available for global validation.");
            return false;
        }

        var targetDock = ResolveGlobalTargetDock(dropCtrl);
        if (targetDock is null)
        {
            LogDropRejection(nameof(ValidateGlobal), "Unable to resolve global docking target for DropControl.");
            return false;
        }

        // Check if the target dock (or any ancestor) has global docking enabled
        if (!DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock))
        {
            LogDropRejection(nameof(ValidateGlobal), $"Global docking is disabled for dock '{targetDock.Title}'.");
            return false;
        }

        DockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            LogDropRejection(nameof(ValidateGlobal), "Relative visual is not attached to a visual root.");
            return false;
        }

        var screenPoint = DockHelpers.GetScreenPoint(relativeTo, point);
        DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        // Check docking groups for global docking visual validation
        // Global adorners should only show when source dockable doesn't have a docking group
        // or when docking groups are compatible
        if (!DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock))
        {
            LogDropRejection(
                nameof(ValidateGlobal),
                $"Global docking group validation failed for '{sourceDockable.Title}' -> '{targetDock.Title}'.");
            return false;
        }

        var isValid = DockManager.ValidateDockable(sourceDockable, targetDock, dragAction, operation, bExecute: false);
        if (!isValid)
        {
            LogDropRejection(
                nameof(ValidateGlobal),
                WithCapabilityDiagnostics($"DockManager rejected global operation {operation} for '{sourceDockable.Title}' -> '{targetDock.Title}'."));
        }

        return isValid;
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
            LogDragState("Process aborted: activeDockControl is null.");
            return;
        }

        if (!DockManager.IsDockingEnabled)
        {
            if (_context.PointerPressed || _context.DoDragDrop)
            {
                _dragPreviewHelper.Hide();
                Leave();
                _context.End();
                DropControl = null;
                activeDockControl.IsDraggingDock = false;
            }

            LogDragState("Process skipped: docking disabled.");
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
                        LogDragState($"Pressed ignored: drag disabled on control '{dragControl.GetType().Name}'.");
                        break;
                    }
                    
                    if (dragControl.DataContext is IDockable dragDockable && !CanDragDockable(dragDockable))
                    {
                        LogDragState("Pressed ignored: dockable cannot be dragged (effective CanDrag=false).");
                        break;
                    }

                    _context.Start(dragControl, point);
                    DropControl = null;
                    LogDragState($"Drag started from control '{dragControl.GetType().Name}' at {point}.");
                }
                else
                {
                    LogDragState("Pressed ignored: no drag area under pointer.");
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
                            LogDragState($"Drop executed on '{dropControl.GetType().Name}' with action '{dragAction}'.");
                        }
                        else
                        {
                            LogDragState($"Drop skipped: drop disabled on '{dropControl.GetType().Name}'.");
                        }
                    }

                    if (!executed && _context.DragControl?.DataContext is IDockable dockable &&
                        CanFloatDockable(dockable) &&
                        inputActiveDockControl.Layout?.Factory is { } factory)
                    {
                        Float(point, inputActiveDockControl, dockable, factory, _context.DragOffset);
                        LogDragState($"Drop fallback: floating dockable '{dockable.Title}'.");
                    }
                }
                else
                {
                    LogDragState("Release ignored: drag operation was never activated.");
                }

                _dragPreviewHelper.Hide();

                Leave();
                _context.End();
                DropControl = null;
                activeDockControl.IsDraggingDock = false;
                LogDragState("Drag context reset after release.");
                break;
            }
            case EventType.Moved:
            {
                if (_context.PointerPressed == false)
                {
                    LogDragState("Move ignored: pointer not pressed.");
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

                            _dragPreviewHelper.Show(targetDockable, sp, _context.DragOffset, inputActiveDockControl);
                            LogDragState($"Drag threshold reached for dockable '{targetDockable.Title}'. Showing preview.");
                        }
                        _context.DoDragDrop = true;
                        activeDockControl.IsDraggingDock = true;
                        LogDragState("Drag operation activated.");
                    }
                    else
                    {
                        LogDragState($"Move below drag threshold (diff={diff}).");
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
                            LogDragState("Skipping dock control search: active dock control not attached to visual tree.");
                            continue;
                        }

                        if (inputDockControl.GetVisualRoot() is null)
                        {
                            LogDragState($"Skipping dock control '{inputDockControl.GetType().Name}': not attached to visual tree.");
                            continue;
                        }
                        var dockControlPoint = inputDockControl.PointToClient(screenPoint);
                        LogDragState($"Testing dock control '{inputDockControl.GetType().Name}' (Bounds={inputDockControl.Bounds}) at client point {dockControlPoint} (screen {screenPoint}).");

                        dropControl = DockHelpers.GetControl(inputDockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                        if (dropControl is { })
                        {
                            targetPoint = dockControlPoint;
                            targetDockControl = inputDockControl;
                            LogDragState($"Found drop control '{dropControl.GetType().Name}' in dock '{inputDockControl.GetType().Name}'.");
                            break;
                        }
                        else
                        {
                            DockHelpers.LogDropAreas(inputDockControl, $"Inspect {inputDockControl.GetType().Name} at {dockControlPoint}");
                        }
                    }

                    if (dropControl is null)
                    {
                        dropControl = DockHelpers.GetControl(inputActiveDockControl, point, DockProperties.IsDropAreaProperty);
                        if (dropControl is { })
                        {
                            targetPoint = point;
                            targetDockControl = inputActiveDockControl;
                            LogDragState($"Using active dock control '{inputActiveDockControl.GetType().Name}' as drop target.");
                        }
                        else
                        {
                            DockHelpers.LogDropAreas(inputActiveDockControl, $"ActiveDockControl fallback at point {point}");
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
                                LogDragState($"Dragging over '{dropControl.GetType().Name}' at {targetPoint}.");
                            }
                            else
                            {
                                if (DropControl is { })
                                {
                                    Leave();
                                    DropControl = null;
                                    LogDragState("Cleared previous drop control before assigning new target.");
                                }

                                DropControl = dropControl;
                                _context.TargetPoint = targetPoint;
                                _context.TargetDockControl = targetDockControl;
                                Enter(targetPoint, dragAction, targetDockControl);
                                LogDragState($"New drop control '{dropControl.GetType().Name}' at {targetPoint}.");
                            }

                            var globalOperation = GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget
                                ? globalDockTarget.GetDockOperation(targetPoint, dropControl, targetDockControl, dragAction, ValidateGlobal, IsDockTargetVisible)
                                : DockOperation.None;

                            var hasLocalAdorner = false;
                            DockOperation localOperation;
                            if (LocalAdornerHelper.Adorner is DockTarget localDockTarget)
                            {
                                hasLocalAdorner = true;
                                localOperation = localDockTarget.GetDockOperation(
                                    targetPoint, dropControl, targetDockControl, dragAction, ValidateLocal, IsDockTargetVisible);
                            }
                            else
                            {
                                localOperation = DockOperation.Fill;
                            }
                            var useGlobalOperation = GlobalDocking.ShouldUseGlobalOperation(
                                hasLocalAdorner,
                                localOperation,
                                globalOperation);

                            LogDragState($"Operations resolved: global={globalOperation}, local={localOperation}.");

                            if (useGlobalOperation)
                            {
                                var valid = ValidateGlobal(targetPoint, globalOperation, dragAction, targetDockControl);
                                preview = valid ? "Dock" : "None";
                                LogDragState($"Global validation {(valid ? "succeeded" : "failed")} for operation {globalOperation}.");
                            }
                            else
                            {
                                var valid = ValidateLocal(targetPoint, localOperation, dragAction, targetDockControl);
                                preview = valid
                                    ? localOperation == DockOperation.Window ? "Float" : "Dock"
                                    : "None";
                                LogDragState($"Local validation {(valid ? "succeeded" : "failed")} for operation {localOperation}.");
                            }
                        }
                        else
                        {
                            LogDragState($"Drop area '{dropControl.GetType().Name}' is disabled.");
                            if (DropControl is { })
                            {
                                Leave();
                                DropControl = null;
                                _context.TargetPoint = default;
                                _context.TargetDockControl = null;
                                LogDragState("Cleared drop control due to disabled target.");
                            }
                        }
                    }
                    else
                    {
                        Leave();
                        DropControl = null;
                        _context.TargetPoint = default;
                        _context.TargetDockControl = null;
                        LogDragState($"No valid drop target at current position (local={point}, screen={screenPoint}); cleared drop context.");
                        var canFloat = _context.DragControl?.DataContext is IDockable sourceDockable && CanFloatDockable(sourceDockable);
                        preview = canFloat ? "Float" : "None";
                    }

                    // If validation produced "None" but the dragged source supports floating, show Float preview.
                    if (preview == "None" && _context.DragControl?.DataContext is IDockable src && CanFloatDockable(src))
                    {
                        preview = "Float";
                        LogDragState("Preview overridden to Float because source can float.");
                    }

                    LogDragState($"Preview state set to '{preview}'.");
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
