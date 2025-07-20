using Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class DebugOverlayHelper
{
    private DebugOverlayAdorner? _adorner;
    private Control? _control;

    public void AddOverlay(Control control)
    {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is null)
        {
            return;
        }

        _control = control;
        _adorner = new DebugOverlayAdorner
        {
            [AdornerLayer.AdornedElementProperty] = control
        };

        ((ISetLogicalParent)_adorner).SetParent(control);
        layer.Children.Add(_adorner);

        control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble | RoutingStrategies.Direct);
        control.AddHandler(InputElement.PointerLeaveEvent, OnPointerLeave,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble | RoutingStrategies.Direct);
        control.LayoutUpdated += OnLayoutUpdated;
    }

    public void RemoveOverlay(Control control)
    {
        if (_adorner is null)
        {
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer is not null)
        {
            layer.Children.Remove(_adorner);
        }

        ((ISetLogicalParent)_adorner).SetParent(null);
        control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(InputElement.PointerLeaveEvent, OnPointerLeave);
        control.LayoutUpdated -= OnLayoutUpdated;
        _control = null;
        _adorner = null;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_control is null || _adorner is null)
        {
            return;
        }

        var pos = e.GetPosition(_control);
        var hit = _control.GetVisualsAt(pos)
            .OfType<Control>()
            .FirstOrDefault(IsDebugTarget);

        _adorner.SetPointerOver(hit);
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e)
    {
        _adorner?.SetPointerOver(null);
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        _adorner?.InvalidateVisual();
    }

    public void Invalidate()
    {
        _adorner?.InvalidateVisual();
    }

    private static bool IsDebugTarget(Control control)
    {
        return DockProperties.GetIsDockTarget(control)
            || DockProperties.GetIsDragArea(control)
            || DockProperties.GetIsDropArea(control);
    }
}

