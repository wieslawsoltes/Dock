using System.Collections;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal class ItemDragBehavior
{
    private bool _enableDrag;
    private Point _start;
    private int _draggedIndex;
    private int _targetIndex;
    private ItemsControl? _itemsControl;
    private IControl? _draggedContainer;

    private IControl Control { get; }

    private Orientation Orientation { get; }

    public ItemDragBehavior(IControl associatedObject, Orientation orientation)
    {
        Control = associatedObject;
        Orientation = orientation;
    }

    public void Attach()
    {
        Control.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
        Control.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        Control.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        Control.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
    }

    public void Detach()
    {
        Control.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        Control.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        Control.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        Control.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (Control.Parent is not ItemsControl itemsControl)
        {
            return;
        }

        _enableDrag = true;
        _start = e.GetPosition(Control.Parent);
        _draggedIndex = -1;
        _targetIndex = -1;
        _itemsControl = itemsControl;
        _draggedContainer = Control;

        AddTransforms(_itemsControl);
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        Release();
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Release();
    }

    private void Release()
    {
        if (!_enableDrag)
        {
            return;
        }

        RemoveTransforms(_itemsControl);

        if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
        {
            MoveDockable(_itemsControl, _draggedIndex, _targetIndex);
        }

        _draggedIndex = -1;
        _targetIndex = -1;
        _enableDrag = false;
        _itemsControl = null;
        _draggedContainer = null;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_itemsControl?.Items is null || _draggedContainer?.RenderTransform is null || !_enableDrag)
        {
            return;
        }

        var orientation = Orientation;
        var isHorizontal = orientation == Orientation.Horizontal;
        var position = e.GetPosition(_itemsControl);
        var delta = isHorizontal ? position.X - _start.X : position.Y - _start.Y;
        var bounds = _draggedContainer.Bounds;

        var translatedOrigin = _draggedContainer.TranslatePoint(new Point(0.0, 0.0), _itemsControl);
        if (translatedOrigin is null)
        {
            return;
        }
            
        var translatedDelta = _draggedContainer.TranslatePoint(new Point(isHorizontal ? delta : 0.0, isHorizontal ? 0.0 : delta), _itemsControl);
        if (translatedDelta is null)
        {
            return;
        }

        Debug.WriteLine($"translatedOrigin={translatedOrigin.Value} translatedDelta={translatedOrigin.Value}");
            
        var tx = translatedDelta.Value.X;
        var ty = translatedDelta.Value.Y;
        if (isHorizontal)
        {
            if (tx < 0.0)
            {
                delta = 0.0;
            }

            //if (tx + bounds.Width > _itemsControl.Bounds.Width)
            //{
            //    delta = _itemsControl.Bounds.Width - bounds.Width;
            //}
        }
        else
        {
            if (ty < 0.0)
            {
                delta = 0.0;
            }

            //if (ty + bounds.Height > _itemsControl.Bounds.Height)
            //{
            //    delta = _itemsControl.Bounds.Height - bounds.Height;
            //}
        }
            
        MoveControl(_draggedContainer, orientation, delta, delta);
        _draggedIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
        _targetIndex = -1;

        var draggedStart = isHorizontal ? bounds.X : bounds.Y;
        var draggedStartDelta = isHorizontal ? bounds.X + delta : bounds.Y + delta;
        var draggedEndDelta = isHorizontal ? bounds.X + delta + bounds.Width : bounds.Y + delta + bounds.Height;

        var i = 0;
        foreach (var _ in _itemsControl.Items)
        {
            var targetContainer = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
            {
                i++;
                continue;
            }

            var target = targetContainer.Bounds;
            var targetStart = isHorizontal ? target.X : target.Y;
            var targetMid = isHorizontal ? target.X + target.Width / 2 : target.Y + target.Height / 2;

            var targetIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);
            if (targetStart > draggedStart && draggedEndDelta >= targetMid)
            {
                MoveControl(targetContainer, orientation, -bounds.Width, -bounds.Height);
                _targetIndex = _targetIndex == -1 ? targetIndex : targetIndex > _targetIndex ? targetIndex : _targetIndex;
            }
            else if (targetStart < draggedStart && draggedStartDelta <= targetMid)
            {
                MoveControl(targetContainer, orientation, bounds.Width, bounds.Height);
                _targetIndex = _targetIndex == -1 ? targetIndex : targetIndex < _targetIndex ? targetIndex : _targetIndex;
            }
            else
            {
                MoveControl(targetContainer, orientation, 0, 0);
            }

            i++;
        }
    }

    private void MoveControl(IControl? control, Orientation orientation, double x, double y)
    {
        if (control?.RenderTransform is not TranslateTransform translateTransform)
        {
            return;
        }

        if (orientation == Orientation.Horizontal)
        {
            translateTransform.X = x;
        }
        else
        {
            translateTransform.Y = y;
        }
    }

    private void MoveDockable(ItemsControl? itemsControl, int draggedIndex, int targetIndex)
    {
        if (itemsControl?.Items is not IList items)
        {
            return;
        }

        var draggedItem = items[draggedIndex];
        var targetItem = items[targetIndex];

        if (draggedItem is IDockable sourceDockable && targetItem is IDockable targetDockable)
        {
            if (sourceDockable.Owner is IDock sourceOwner && targetDockable.Owner is IDock targetOwner)
            {
                if (sourceOwner == targetOwner && sourceOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceOwner, sourceDockable, targetDockable);
                }
            }
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
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
            {
                container.RenderTransform = new TranslateTransform();
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
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
            {
                container.RenderTransform = null;
            }
  
            i++;
        }  
    }
}
