// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Settings;
using Orientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.Internal;

internal class ItemDragHelper
{
    private readonly Control _owner;
    private readonly Func<ItemsControl?> _getItemsControl;
    private readonly Func<Control?> _getBoundsContainer;
    private readonly Func<Orientation> _getOrientation;
    private readonly Func<double> _getHorizontalDragThreshold;
    private readonly Func<double> _getVerticalDragThreshold;
    private readonly Action<PointerEventArgs>? _dragOutside;

    private bool _enableDrag;
    private bool _dragStarted;
    private Point _start;
    private int _draggedIndex;
    private int _targetIndex;
    private ItemsControl? _itemsControl;
    private Control? _draggedContainer;
    private bool _captured;
    private DispatcherTimer? _autoScrollTimer;
    private ScrollViewer? _currentScrollViewer;
    private Vector _autoScrollVelocity;
    private const double AutoScrollEdgeSize = 20.0;
    private const double AutoScrollSpeed = 200.0; // pixels per second
    private const double AutoScrollTimerInterval = 16.0; // ~60fps

    public ItemDragHelper(
        Control owner,
        Func<ItemsControl?> getItemsControl,
        Func<Orientation> getOrientation,
        Func<double>? getHorizontalDragThreshold = null,
        Func<double>? getVerticalDragThreshold = null,
        Action<PointerEventArgs>? dragOutside = null,
        Func<Control?>? getBoundsContainer = null)
    {
        _owner = owner;
        _getItemsControl = getItemsControl;
        _getBoundsContainer = getBoundsContainer ?? (() => null);
        _getOrientation = getOrientation;
        _getHorizontalDragThreshold = getHorizontalDragThreshold ?? (() => DockSettings.MinimumHorizontalDragDistance);
        _getVerticalDragThreshold = getVerticalDragThreshold ?? (() => DockSettings.MinimumVerticalDragDistance);
        _dragOutside = dragOutside;
    }

    public void Attach()
    {
        _owner.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, RoutingStrategies.Tunnel);
        _owner.AddHandler(InputElement.PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
        _owner.AddHandler(InputElement.PointerMovedEvent, PointerMoved, RoutingStrategies.Tunnel);
        _owner.AddHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost, RoutingStrategies.Tunnel);
    }

    public void Detach()
    {
        _owner.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
        _owner.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
        _owner.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
        _owner.RemoveHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost);
        
        StopAutoScroll();
    }

    private void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(_owner).Properties;
        if (properties.IsLeftButtonPressed && _getItemsControl() is { } itemsControl)
        {
            _enableDrag = true;
            _dragStarted = false;
            _start = e.GetPosition(itemsControl);
            _draggedIndex = -1;
            _targetIndex = -1;
            _itemsControl = itemsControl;
            _draggedContainer = _owner;

            if (_draggedContainer is not null)
            {
                SetDraggingPseudoClasses(_draggedContainer, true);
                _draggedContainer.SetCurrentValue(Visual.ZIndexProperty, 1);
            }

            AddTransforms(_itemsControl);

            _captured = true;
        }
    }

    private void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_captured)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Released();
            }

            _captured = false;
        }
    }

    private void PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
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

        StopAutoScroll();

        _draggedContainer?.ClearValue(Visual.ZIndexProperty);
        
        RemoveTransforms(_itemsControl);

        if (_itemsControl is not null)
        {
            foreach (var control in _itemsControl.GetRealizedContainers())
            {
                SetDraggingPseudoClasses(control, true);
            }
        }

        if (_dragStarted)
        {
            if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
            {
                MoveDraggedItem(_itemsControl, _draggedIndex, _targetIndex);
            }
        }

        if (_itemsControl is not null)
        {
            foreach (var control in _itemsControl.GetRealizedContainers())
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
        _itemsControl = null;

        _draggedContainer = null;
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
            var container = itemsControl.ContainerFromIndex(i);
            if (container is not null)
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
            var container = itemsControl.ContainerFromIndex(i);
            if (container is not null)
            {
                SetTranslateTransform(container, 0, 0);
            }

            i++;
        }
    }

    private void MoveDraggedItem(ItemsControl? itemsControl, int draggedIndex, int targetIndex)
    {
        if (itemsControl is null || draggedIndex == targetIndex)
        {
            return;
        }

        if (itemsControl.ContainerFromIndex(draggedIndex) is { } dragged &&
            itemsControl.ContainerFromIndex(targetIndex) is { } target &&
            dragged.DataContext is IDockable draggedDockable &&
            target.DataContext is IDockable targetDockable &&
            draggedDockable.Owner is IDock { Factory: { } factory } dock)
        {
            // Check if we're dealing with pinned dockables
            if (factory.IsDockablePinned(draggedDockable) && factory.IsDockablePinned(targetDockable))
            {
                factory.MovePinnedDockable(draggedDockable, targetDockable);
            }
            else
            {
                factory.MoveDockable(dock, draggedDockable, targetDockable);
            }

            if (itemsControl is SelectingItemsControl selectingItemsControl)
            {
                selectingItemsControl.SelectedIndex = targetIndex;
            }

            return;
        }

        if (itemsControl.ItemsSource is IList itemsSource)
        {
            var draggedItem = itemsSource[draggedIndex];
            itemsSource.RemoveAt(draggedIndex);
            itemsSource.Insert(targetIndex, draggedItem);

            if (itemsControl is SelectingItemsControl selectingItemsControl)
            {
                selectingItemsControl.SelectedIndex = targetIndex;
            }
        }
        else if (itemsControl.Items is { IsReadOnly: false } itemCollection)
        {
            var draggedItem = itemCollection[draggedIndex];
            itemCollection.RemoveAt(draggedIndex);
            itemCollection.Insert(targetIndex, draggedItem);

            if (itemsControl is SelectingItemsControl selectingItemsControl)
            {
                selectingItemsControl.SelectedIndex = targetIndex;
            }
        }
    }

    private void StartAutoScroll(ScrollViewer scrollViewer, Vector velocity)
    {
        if (_autoScrollTimer?.IsEnabled == true && 
            _currentScrollViewer == scrollViewer && 
            _autoScrollVelocity.Equals(velocity))
        {
            return; // Already scrolling in the same direction
        }

        StopAutoScroll();

        _currentScrollViewer = scrollViewer;
        _autoScrollVelocity = velocity;

        _autoScrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(AutoScrollTimerInterval)
        };

        _autoScrollTimer.Tick += (_, _) =>
        {
            if (_currentScrollViewer == null)
            {
                StopAutoScroll();
                return;
            }

            var deltaTime = AutoScrollTimerInterval / 1000.0; // Convert to seconds
            var deltaX = _autoScrollVelocity.X * deltaTime;
            var deltaY = _autoScrollVelocity.Y * deltaTime;

            var newOffsetX = Math.Max(0, Math.Min(_currentScrollViewer.Extent.Width - _currentScrollViewer.Viewport.Width, 
                _currentScrollViewer.Offset.X + deltaX));
            var newOffsetY = Math.Max(0, Math.Min(_currentScrollViewer.Extent.Height - _currentScrollViewer.Viewport.Height, 
                _currentScrollViewer.Offset.Y + deltaY));

            _currentScrollViewer.Offset = new Vector(newOffsetX, newOffsetY);
        };

        _autoScrollTimer.Start();
    }

    private void StopAutoScroll()
    {
        if (_autoScrollTimer != null)
        {
            _autoScrollTimer.Stop();
            _autoScrollTimer = null;
        }
        _currentScrollViewer = null;
        _autoScrollVelocity = default;
    }

    private Vector CalculateAutoScrollVelocity(Point position, ScrollViewer scrollViewer)
    {
        var viewport = new Rect(scrollViewer.Viewport);
        var velocityX = 0.0;
        var velocityY = 0.0;

        // Calculate horizontal auto-scroll
        if (position.X < AutoScrollEdgeSize)
        {
            // Scroll left
            var intensity = (AutoScrollEdgeSize - position.X) / AutoScrollEdgeSize;
            velocityX = -AutoScrollSpeed * intensity;
        }
        else if (position.X > viewport.Width - AutoScrollEdgeSize)
        {
            // Scroll right  
            var intensity = (position.X - (viewport.Width - AutoScrollEdgeSize)) / AutoScrollEdgeSize;
            velocityX = AutoScrollSpeed * intensity;
        }

        // Calculate vertical auto-scroll
        if (position.Y < AutoScrollEdgeSize)
        {
            // Scroll up
            var intensity = (AutoScrollEdgeSize - position.Y) / AutoScrollEdgeSize;
            velocityY = -AutoScrollSpeed * intensity;
        }
        else if (position.Y > viewport.Height - AutoScrollEdgeSize)
        {
            // Scroll down
            var intensity = (position.Y - (viewport.Height - AutoScrollEdgeSize)) / AutoScrollEdgeSize;
            velocityY = AutoScrollSpeed * intensity;
        }

        return new Vector(velocityX, velocityY);
    }

    private bool IsPositionWithinDragBounds(Point position, ItemsControl itemsControl)
    {
        // Use the bounds container (TabStrip) if provided, otherwise use ItemsControl
        var boundsContainer = _getBoundsContainer() ?? itemsControl;
        
        // Get position relative to the bounds container
        var containerPosition = position;
        if (!ReferenceEquals(boundsContainer, itemsControl))
        {
            // Transform position from ItemsControl to bounds container coordinate space
            var transform = itemsControl.TransformToVisual(boundsContainer);
            if (transform != null)
            {
                containerPosition = transform.Value.Transform(position);
            }
        }
        
        var boundsRect = new Rect(boundsContainer.Bounds.Size);
        
        // Check if we have a ScrollViewer - look for it as a descendant of the TabStrip
        // since the ScrollViewer is inside the TabStrip template, not above it
        var scrollViewer = itemsControl.FindDescendantOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            // For scrollable areas, we need to consider the full scrollable extent
            var scrollViewerPosition = position;
            if (!ReferenceEquals(scrollViewer, itemsControl))
            {
                var scrollTransform = itemsControl.TransformToVisual(scrollViewer);
                if (scrollTransform != null)
                {
                    scrollViewerPosition = scrollTransform.Value.Transform(position);
                }
            }
            
            // Calculate auto-scroll velocity and start/stop auto-scrolling
            var autoScrollVelocity = CalculateAutoScrollVelocity(scrollViewerPosition, scrollViewer);
            if (autoScrollVelocity.Length > 0)
            {
                StartAutoScroll(scrollViewer, autoScrollVelocity);
            }
            else
            {
                StopAutoScroll();
            }
            
            // Create bounds that include the full scrollable area
            var scrollableBounds = new Rect(
                -scrollViewer.Offset.X,
                -scrollViewer.Offset.Y,
                Math.Max(scrollViewer.Viewport.Width, scrollViewer.Extent.Width),
                Math.Max(scrollViewer.Viewport.Height, scrollViewer.Extent.Height)
            );
            
            // Check if position is within scrollable bounds
            if (!scrollableBounds.Contains(scrollViewerPosition))
            {
                return false;
            }
            
            // Also check if position is within the container bounds (TabStrip visual area)
            return boundsRect.Contains(containerPosition);
        }
        else
        {
            // No scrolling, stop any auto-scroll that might be running
            StopAutoScroll();
        }
        
        // No scrolling, just check container bounds
        return boundsRect.Contains(containerPosition);
    }

    private void PointerMoved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(_owner).Properties;
        if (_captured && properties.IsLeftButtonPressed)
        {
            if (_itemsControl?.Items is null || _draggedContainer?.RenderTransform is null || !_enableDrag)
            {
                return;
            }

            var position = e.GetPosition(_itemsControl);
            if (!IsPositionWithinDragBounds(position, _itemsControl))
            {
                _dragStarted = false;
                Released();
                _captured = false;
                _dragOutside?.Invoke(e);
                return;
            }

            var orientation = _getOrientation();
            var delta = orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

            if (!_dragStarted)
            {
                var diff = _start - position;
                var horizontalDragThreshold = _getHorizontalDragThreshold();
                var verticalDragThreshold = _getVerticalDragThreshold();

                if (orientation == Orientation.Horizontal)
                {
                    if (Math.Abs(diff.X) > horizontalDragThreshold)
                    {
                        _dragStarted = true;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (Math.Abs(diff.Y) > verticalDragThreshold)
                    {
                        _dragStarted = true;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (orientation == Orientation.Horizontal)
            {
                SetTranslateTransform(_draggedContainer, delta, 0);
            }
            else
            {
                SetTranslateTransform(_draggedContainer, 0, delta);
            }

            _draggedIndex = _itemsControl.IndexFromContainer(_draggedContainer);
            _targetIndex = -1;

            var draggedBounds = _draggedContainer.Bounds;
            var draggedStart = orientation == Orientation.Horizontal ? draggedBounds.X : draggedBounds.Y;
            var draggedDeltaStart = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta
                : draggedBounds.Y + delta;
            var draggedDeltaEnd = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta + draggedBounds.Width
                : draggedBounds.Y + delta + draggedBounds.Height;

            var i = 0;
            foreach (var _ in _itemsControl.Items)
            {
                var targetContainer = _itemsControl.ContainerFromIndex(i);
                if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
                {
                    i++;
                    continue;
                }

                var targetBounds = targetContainer.Bounds;
                var targetStart = orientation == Orientation.Horizontal ? targetBounds.X : targetBounds.Y;
                var targetMid = orientation == Orientation.Horizontal
                    ? targetBounds.X + targetBounds.Width / 2
                    : targetBounds.Y + targetBounds.Height / 2;
                var targetIndex = _itemsControl.IndexFromContainer(targetContainer);

                if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        SetTranslateTransform(targetContainer, -draggedBounds.Width, 0);
                    }
                    else
                    {
                        SetTranslateTransform(targetContainer, 0, -draggedBounds.Height);
                    }

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex > _targetIndex ? targetIndex : _targetIndex;
                }
                else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        SetTranslateTransform(targetContainer, draggedBounds.Width, 0);
                    }
                    else
                    {
                        SetTranslateTransform(targetContainer, 0, draggedBounds.Height);
                    }

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex < _targetIndex ? targetIndex : _targetIndex;
                }
                else
                {
                    SetTranslateTransform(targetContainer, 0, 0);
                }

                i++;
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
        var transformBuilder = new TransformOperations.Builder(1);
        transformBuilder.AppendTranslate(x, y);
        control.RenderTransform = transformBuilder.Build();
    }
}

