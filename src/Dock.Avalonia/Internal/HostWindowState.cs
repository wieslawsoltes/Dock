// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Model.Controls;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal class WindowDragContext
{
    public PixelPoint DragStartPoint { get; set; }
    public bool PointerPressed { get; set; }
    public bool DoDragDrop { get; set; }
    public DockControl? TargetDockControl { get; set; }
    public Point TargetPoint { get; set; }
    public DragAction DragAction { get; set; }

    public void Start(PixelPoint point)
    {
        DragStartPoint = point;
        PointerPressed = true;
        DoDragDrop = false;
        TargetDockControl = null;
        TargetPoint = default;
        DragAction = DragAction.Move;
    }

    public void End()
    {
        DragStartPoint = default;
        PointerPressed = false;
        DoDragDrop = false;
        TargetDockControl = null;
        TargetPoint = default;
        DragAction = DragAction.Move;
    }
}
    
/// <summary>
/// Host window state.
/// </summary>
internal class HostWindowState : DockManagerState, IHostWindowState
{
    private readonly HostWindow _hostWindow;
    private readonly WindowDragContext _context = new();

    public HostWindowState(IDockManager dockManager, HostWindow hostWindow) 
        : base(dockManager)
    {
        _hostWindow = hostWindow;
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isLocalValid = ValidateLocal(point, DockOperation.Fill, dragAction, relativeTo);
        var isGlobalValid = ValidateGlobal(point, DockOperation.Fill, dragAction, relativeTo);

        AddAdorners(isLocalValid, isGlobalValid);
    }

    private void Over(Point point, DragAction dragAction, Visual relativeTo)
    {
        var localOperation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        if (globalOperation != DockOperation.None)
        {
            ValidateGlobal(point, localOperation, dragAction, relativeTo);
        }
        else
        {
            if (localOperation != DockOperation.Window)
            {
                ValidateLocal(point, localOperation, dragAction, relativeTo);
            } 
        }
    }

    private void Drop(Point point, DragAction dragAction, Visual relativeTo)
    {
        var localOperation = DockOperation.Window;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        RemoveAdorners();

        if (DropControl is null)
        {
            return;
        }

        var layout = _hostWindow.Window?.Layout;

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

            if (layout?.ActiveDockable is { } sourceDockable
                && dockControl.Layout is { } dockControlLayout
                && dockControlLayout.ActiveDockable is IDock dockControlActiveDock)
            {
                var targetDock = DockHelpers.FindProportionalDock(dockControlActiveDock) ?? dockControlActiveDock;
                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, targetDock);
            }
        }
        else
        {
            if (layout?.ActiveDockable is { } sourceDockable
                && DropControl.DataContext is IDockable targetDockable)
            {
                if (localOperation != DockOperation.Window)
                {
                    Execute(point, localOperation, dragAction, relativeTo, sourceDockable, targetDockable);
                }
            }
        }
    }

    private void Leave()
    {
        RemoveAdorners();
    }

    private bool ValidateLocal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not { } sourceDockable)
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

        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not { } sourceDockable)
        {
            return false;
        }

        if (DropControl is not { } dropControl)
        {
            return false;
        }

        var dockControl = dropControl.FindAncestorOfType<DockControl>();
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
        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not IDockable sourceDockable)
        {
            return true;
        }

        if (DropControl?.DataContext is not IDockable targetDockable)
        {
            return true;
        }

        return DockManager.IsDockTargetVisible(sourceDockable, targetDockable, operation);
    }

    /// <summary>
    /// Process pointer event.
    /// </summary>
    /// <param name="point">The pointer position.</param>
    /// <param name="eventType">The pointer event type.</param>
    public void Process(PixelPoint point, EventType eventType)
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

                if (_hostWindow.DataContext is IDockable { CanDrag: false })
                {
                    break;
                }

                _context.Start(point);
                DropControl = null;
                break;
            }
            case EventType.Released:
            {
                if (_context.DoDragDrop)
                {
                    if (_context.TargetDockControl is { } && DropControl is { })
                    {
                        var isDropEnabled = true;

                        if (DropControl is { } targetDropControl)
                        {
                            isDropEnabled = targetDropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        }

                        if (isDropEnabled)
                        {
                            Drop(_context.TargetPoint, _context.DragAction, _context.TargetDockControl);
                        }
                    } 
                }

                Leave();
                _context.End();
                DropControl = null;
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
                    var diff = _context.DragStartPoint - point;
                    var haveMinimumDragDistance = DockSettings.IsMinimumDragDistance(diff);
                    if (haveMinimumDragDistance)
                    {
                       _context.DoDragDrop = true;
                    }
                }

                if (!_context.DoDragDrop || _hostWindow.Window?.Layout?.Factory is not { } factory)
                {
                    break;
                }

                foreach (var dockControl in factory.DockControls.GetZOrderedDockControls())
                {
                    if (dockControl.Layout == _hostWindow.Window?.Layout)
                    {
                        continue;
                    }

                    var position = point + _context.DragStartPoint;
                    var screenPoint = new PixelPoint(position.X, position.Y);
                    if (dockControl.GetVisualRoot() is null)
                    {
                        continue;
                    }

                    var dockControlPoint = dockControl.PointToClient(screenPoint);
                    var dropControl = DockHelpers.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                    if (dropControl is { })
                    {
                        var isDropEnabled = dropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        if (!isDropEnabled)
                        {
                            Leave();
                            _context.TargetDockControl = null;
                            _context.TargetPoint = default;
                            DropControl = null;
                        }
                        else
                        {
                            if (DropControl == dropControl)
                            {
                                _context.TargetDockControl = dockControl;
                                _context.TargetPoint = dockControlPoint;
                                DropControl = dropControl;
                                _context.DragAction = DragAction.Move;
                                Over(_context.TargetPoint, _context.DragAction, _context.TargetDockControl);
                                break;
                            }

                            if (DropControl is { })
                            {
                                Leave();
                                DropControl = null;
                            }

                            _context.TargetDockControl = dockControl;
                            _context.TargetPoint = dockControlPoint;
                            DropControl = dropControl;
                            _context.DragAction = DragAction.Move;
                            Enter(_context.TargetPoint, _context.DragAction, _context.TargetDockControl);
                            break;
                        }
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
                _context.End();
                DropControl = null;
                break;
            }
            case EventType.WheelChanged:
            {
                break;
            }
        }
    }
}
