using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Base class for TabStripItems that support dragging, reordering and floating.
/// </summary>
/// <typeparam name="TTabStrip">Type of parent TabStrip.</typeparam>
public abstract class DraggableTabStripItem<TTabStrip> : TabStripItem
    where TTabStrip : TabStrip
{
    private bool _enableDrag;
    private bool _dragStarted;
    private bool _captured;
    private Point _start;
    private int _draggedIndex;
    private int _targetIndex;
    private TTabStrip? _tabStrip;
    private TabStripItem? _draggedContainer;

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, MovedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, ReleasedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerCaptureLostEvent, CaptureLostHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);
        RemoveHandler(PointerMovedEvent, MovedHandler);
        RemoveHandler(PointerReleasedEvent, ReleasedHandler);
        RemoveHandler(PointerCaptureLostEvent, CaptureLostHandler);
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsMiddleButtonPressed)
        {
            if (DataContext is IDockable { Owner: IDock { Factory: { } factory }, CanClose: true } dockable)
            {
                factory.CloseDockable(dockable);
            }
        }

        if (properties.IsLeftButtonPressed && this.FindAncestorOfType<TTabStrip>() is { } tabStrip)
        {
            _enableDrag = true;
            _dragStarted = false;
            _start = e.GetPosition(tabStrip);
            _draggedIndex = -1;
            _targetIndex = -1;
            _tabStrip = tabStrip;
            _draggedContainer = this;
            SetDraggingPseudoClasses(this, true);
            AddTransforms(tabStrip);
            _captured = true;
            e.Pointer.Capture(this);
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_captured && e.InitialPressMouseButton == MouseButton.Left)
        {
            Released();
        }
        _captured = false;
    }

    private void CaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
        _captured = false;
    }

    private void Released()
    {
        if (!_enableDrag)
        {
            return;
        }

        RemoveTransforms(_tabStrip);

        if (_tabStrip is not null)
        {
            foreach (var control in _tabStrip.GetRealizedContainers())
            {
                SetDraggingPseudoClasses(control, true);
            }
        }

        if (_dragStarted && _draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
        {
            MoveDockable(_draggedIndex, _targetIndex);
        }

        if (_tabStrip is not null)
        {
            foreach (var control in _tabStrip.GetRealizedContainers())
            {
                SetDraggingPseudoClasses(control, false);
            }
        }

        if (_draggedContainer is not null)
        {
            SetDraggingPseudoClasses(_draggedContainer, false);
        }

        _draggedIndex = -1;
        _targetIndex = -1;
        _enableDrag = false;
        _dragStarted = false;
        _tabStrip = null;
        _draggedContainer = null;
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        if (!_captured || !_enableDrag || _tabStrip?.Items is null || _draggedContainer?.RenderTransform is null)
        {
            return;
        }

        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed)
        {
            return;
        }

        var position = e.GetPosition(_tabStrip);
        var delta = position.X - _start.X;

        if (!_dragStarted)
        {
            var diff = _start - position;
            if (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance)
            {
                _dragStarted = true;
            }
            else
            {
                return;
            }
        }

        if (!_tabStrip.Bounds.Contains(position))
        {
            var bounds = _tabStrip.Bounds;
            var overshootX = position.X < 0 ? -position.X : position.X - bounds.Width;
            var overshootY = position.Y < 0 ? -position.Y : position.Y - bounds.Height;
            overshootX = Math.Max(overshootX, 0);
            overshootY = Math.Max(overshootY, 0);

            if (overshootX > DockSettings.FloatDragDistance || overshootY > DockSettings.FloatDragDistance)
            {
                Released();
                _captured = false;
                FloatDockable();
                return;
            }
        }

        SetTranslateTransform(_draggedContainer, delta, 0);

        _draggedIndex = _tabStrip.IndexFromContainer(_draggedContainer);
        _targetIndex = -1;

        var draggedBounds = _draggedContainer.Bounds;
        var draggedStart = draggedBounds.X;
        var draggedDeltaStart = draggedBounds.X + delta;
        var draggedDeltaEnd = draggedBounds.X + delta + draggedBounds.Width;

        var i = 0;
        foreach (var _ in _tabStrip.Items)
        {
            var targetContainer = _tabStrip.ItemContainerGenerator.ContainerFromIndex(i) as TabStripItem;
            if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
            {
                i++;
                continue;
            }

            var targetBounds = targetContainer.Bounds;
            var targetStart = targetBounds.X;
            var targetMid = targetBounds.X + targetBounds.Width / 2;
            var targetIndex = _tabStrip.IndexFromContainer(targetContainer);

            if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
            {
                SetTranslateTransform(targetContainer, -draggedBounds.Width, 0);
                _targetIndex = _targetIndex == -1 ? targetIndex : Math.Max(targetIndex, _targetIndex);
            }
            else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
            {
                SetTranslateTransform(targetContainer, draggedBounds.Width, 0);
                _targetIndex = _targetIndex == -1 ? targetIndex : Math.Min(targetIndex, _targetIndex);
            }
            else
            {
                SetTranslateTransform(targetContainer, 0, 0);
            }

            i++;
        }
    }

    private void FloatDockable()
    {
        if (DataContext is IDockable { Owner: IDock { Factory: { } factory } } dockable && dockable.CanFloat)
        {
            factory.FloatDockable(dockable);
        }
    }

    private void AddTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
        {
            return;
        }

        var i = 0;
        foreach (var _ in itemsControl.Items)
        {
            if (itemsControl.ContainerFromIndex(i) is Control container)
            {
                SetTranslateTransform(container, 0, 0);
            }
            i++;
        }
    }

    private void RemoveTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
        {
            return;
        }

        var i = 0;
        foreach (var _ in itemsControl.Items)
        {
            if (itemsControl.ContainerFromIndex(i) is Control container)
            {
                SetTranslateTransform(container, 0, 0);
            }
            i++;
        }
    }

    private void MoveDockable(int draggedIndex, int targetIndex)
    {
        if (_tabStrip is null)
        {
            return;
        }

        if (DataContext is not IDockable { Owner: IDock { Factory: { } factory } owner } dockable)
        {
            return;
        }

        if (owner.VisibleDockables is not { } list)
        {
            return;
        }

        if (targetIndex >= list.Count)
        {
            targetIndex = list.Count - 1;
        }

        if (targetIndex < 0 || targetIndex >= list.Count)
        {
            return;
        }

        var target = list[targetIndex];
        if (!ReferenceEquals(target, dockable))
        {
            factory.MoveDockable(owner, owner, dockable, target);
            if (_tabStrip is SelectingItemsControl selecting)
            {
                selecting.SelectedIndex = targetIndex;
            }
        }
    }

    private void SetDraggingPseudoClasses(Control control, bool isDragging)
    {
        if (isDragging)
        {
            ((IPseudoClasses)control.Classes).Add(":dragging");
        }
        else
        {
            ((IPseudoClasses)control.Classes).Remove(":dragging");
        }
    }

    private void SetTranslateTransform(Control control, double x, double y)
    {
        var builder = new TransformOperations.Builder(1);
        builder.AppendTranslate(x, y);
        control.RenderTransform = builder.Build();
    }
}
