// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStripItem custom control.
/// </summary>
public class ToolTabStripItem : TabStripItem
{
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStripItem);

    private bool _pressed;
    private bool _detached;
    private Point _start;
        
    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, MovedHandler, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, ReleasedHandler, RoutingStrategies.Tunnel);
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);
        RemoveHandler(PointerMovedEvent, MovedHandler);
        RemoveHandler(PointerReleasedEvent, ReleasedHandler);
    }

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            if (DataContext is IDockable { Owner: IDock { Factory: { } factory }, CanClose: true } dockable)
            {
                factory.CloseDockable(dockable);
            }
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _pressed = true;
            _detached = false;
            _start = e.GetPosition(this);
        }
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        if (_pressed && !_detached)
        {
            var position = e.GetPosition(this);
            var diff = position - _start;
            if (Math.Abs(diff.X) > DockSettings.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > DockSettings.MinimumVerticalDragDistance)
            {
                if (this.FindAncestorOfType<ToolTabStrip>() is { } tabStrip)
                {
                    var pt = e.GetPosition(tabStrip);
                    if (!tabStrip.Bounds.Contains(pt))
                    {
                        if (DataContext is IDockable { Owner: IDock { Factory: { } factory } } dockable)
                        {
                            factory.FloatDockable(dockable);
                            _detached = true;
                        }
                    }
                }
            }
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        _pressed = false;
        _detached = false;
    }
}
