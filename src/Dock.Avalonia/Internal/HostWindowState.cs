// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
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
            if (operation != DockOperation.Window)
            {
                Validate(point, operation, dragAction, relativeTo);
            } 
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
                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, dockControlActiveDock);
            }
        }
        else
        {
            if (layout?.ActiveDockable is { } sourceDockable
                && DropControl.DataContext is IDockable targetDockable)
            {
                if (operation != DockOperation.Window)
                {
                    Execute(point, operation, dragAction, relativeTo, sourceDockable, targetDockable);
                }
            }
        }
    }

    private void Leave()
    {
        RemoveAdorners();
    }

    private bool Validate(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (DropControl is null)
        {
            return false;
        }

        var layout = _hostWindow.Window?.Layout;

        if (layout?.FocusedDockable is { } sourceDockable && DropControl.DataContext is IDockable targetDockable)
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
        DockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return;
        }
        var screenPoint = relativeTo.PointToScreen(point).ToPoint(1.0);
        DockManager.ScreenPosition = DockHelpers.ToDockPoint(screenPoint);

        DockManager.ValidateDockable(sourceDockable, targetDockable, dragAction, operation, bExecute: true);
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
                    var haveMinimumDragDistance = IsMinimumDragDistance(diff);
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
