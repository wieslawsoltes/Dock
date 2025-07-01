// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
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
    private readonly DragPreviewHelper _dragPreviewHelper = new();
    private readonly TabPreviewHelper _tabPreviewHelper = new();

    /// <inheritdoc/>
    public IDockManager DockManager { get; set; }

    public DockControlState(IDockManager dockManager)
    {
        DockManager = dockManager;
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isValid = Validate(point, DockOperation.Fill, dragAction, relativeTo);
        if (_state.DropControl is { } control)
        {
            if (control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.AddAdorner(control);
            }

            if (control is TabStrip tabStrip && isValid)
            {
                _tabPreviewHelper.AddAdorner(tabStrip);
            }
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

        if (_state.DropControl is { } control)
        {
            if (control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.RemoveAdorner(control);
            }

            if (control is TabStrip)
            {
                _tabPreviewHelper.RemoveAdorner();
            }
        }

        Execute(point, operation, dragAction, relativeTo);
    }

    private void Leave()
    {
        if (_state.DropControl is { } control)
        {
            if (control.GetValue(DockProperties.IsDockTargetProperty))
            {
                _adornerHelper.RemoveAdorner(control);
            }

            if (control is TabStrip)
            {
                _tabPreviewHelper.RemoveAdorner();
            }
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

    private static int GetInsertIndex(TabStrip tabStrip, Point point)
    {
        for (int i = 0; i < tabStrip.ItemCount; i++)
        {
            if (tabStrip.ItemContainerGenerator.ContainerFromIndex(i) is Control c)
            {
                var p = point - c.Bounds.Position;
                if (p.X <= c.Bounds.Width / 2)
                    return i;
                if (p.X <= c.Bounds.Width)
                    return i + 1;
            }
        }
        return tabStrip.ItemCount;
    }

    private static Rect GetPreviewRect(TabStrip tabStrip, int index)
    {
        if (tabStrip.ItemCount == 0)
            return new Rect(0, 0, 80, 24);

        if (index < 0)
            index = 0;
        else if (index > tabStrip.ItemCount)
            index = tabStrip.ItemCount;

        if (index < tabStrip.ItemCount)
        {
            if (tabStrip.ItemContainerGenerator.ContainerFromIndex(index) is Control c)
            {
                return new Rect(c.Bounds.Position, c.Bounds.Size);
            }
        }

        if (tabStrip.ItemContainerGenerator.ContainerFromIndex(tabStrip.ItemCount - 1) is Control last)
        {
            return new Rect(last.Bounds.Right, last.Bounds.Top, last.Bounds.Width, last.Bounds.Height);
        }

        return new Rect(0, 0, 80, 24);
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
                _tabPreviewHelper.RemoveAdorner();

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
                            _dragPreviewHelper.Show(targetDockable.Title ?? string.Empty, sp);
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
                    string preview = "None";

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

                            var operation = _adornerHelper.Adorner is DockTarget target
                                ? target.GetDockOperation(targetPoint, targetDockControl, dragAction, Validate)
                                : DockOperation.Fill;

                            var valid = Validate(targetPoint, operation, dragAction, targetDockControl);
                            preview = valid
                                ? operation == DockOperation.Window ? "Float" : "Dock"
                                : "None";

                            if (dropControl is TabStrip tabStrip && _state.DragControl?.DataContext is IDockable dragDockable)
                            {
                                var tp = tabStrip.PointToClient(screenPoint);
                                int index = GetInsertIndex(tabStrip, tp);
                                var rect = GetPreviewRect(tabStrip, index);
                                _tabPreviewHelper.AddAdorner(tabStrip);
                                _tabPreviewHelper.Move(rect, dragDockable.Title ?? string.Empty);
                            }
                            else
                            {
                                _tabPreviewHelper.RemoveAdorner();
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
                            preview = "Float";
                            _tabPreviewHelper.RemoveAdorner();
                        }
                    }
                    else
                    {
                        Leave();
                        _state.DropControl = null;
                        _state.TargetPoint = default;
                        _state.TargetDockControl = null;
                        preview = "Float";
                        _tabPreviewHelper.RemoveAdorner();
                    }

                    _dragPreviewHelper.Move(screenPoint, preview);
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
                _tabPreviewHelper.RemoveAdorner();
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
