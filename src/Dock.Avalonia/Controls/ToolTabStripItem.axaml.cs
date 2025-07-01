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
using System.Linq;

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

                    if (tabStrip.Bounds.Contains(pt))
                    {
                        ReorderDockable(tabStrip, pt);
                    }
                    else if (Math.Abs(diff.X) > DockSettings.FloatDragDistance ||
                             Math.Abs(diff.Y) > DockSettings.FloatDragDistance)
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

    private void ReorderDockable(TabStrip tabStrip, Point pointer)
    {
        if (DataContext is not IDockable { Owner: IDock { Factory: { } factory } owner } dockable)
        {
            return;
        }

        if (owner.VisibleDockables is not { } list)
        {
            return;
        }

        var from = list.IndexOf(dockable);
        for (var i = 0; i < list.Count; i++)
        {
            if (tabStrip.ItemContainerGenerator.ContainerFromIndex(i) is not TabStripItem container)
            {
                continue;
            }

            var bounds = container.Bounds;
            var mid = bounds.X + bounds.Width / 2;
            if (pointer.X < mid)
            {
                if (i != from && list[i] is IDockable target)
                {
                    factory.MoveDockable(owner, owner, dockable, target);
                }
                return;
            }
        }

        if (list.Count > 0 && from != list.Count - 1)
        {
            var last = list[list.Count - 1] as IDockable;
            if (last is not null)
            {
                factory.MoveDockable(owner, owner, dockable, last);
            }
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        _pressed = false;
        _detached = false;
    }
}
