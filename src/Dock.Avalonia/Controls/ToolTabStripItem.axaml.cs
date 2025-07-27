// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using AvaloniaOrientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Tool TabStripItem custom control.
/// </summary>
public class ToolTabStripItem : TabStripItem
{
    private ItemDragHelper? _dragHelper;

    private static DragAction ToDragAction(PointerEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return DragAction.Link;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return DragAction.Move;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return DragAction.Copy;
        }

        return DragAction.Move;
    }

    private void StartDockDrag(PointerEventArgs e)
    {
        var dockControl = this.FindAncestorOfType<DockControl>();
        if (dockControl?.Layout?.Factory?.DockControls is { } dockControls
            && dockControl.DockControlState is DockControlState state)
        {
            var position = e.GetPosition(dockControl);
            var action = ToDragAction(e);
            state.StartDrag(this, position, dockControl);
            state.Process(position, default, EventType.Moved, action, dockControl, dockControls);
        }
    }
    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(ToolTabStripItem);

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        AddHandler(DoubleTappedEvent, DoubleTappedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        _dragHelper = new ItemDragHelper(
            this,
            () => Parent as ItemsControl,
            () => AvaloniaOrientation.Horizontal,
            dragOutside: StartDockDrag,
            getBoundsContainer: () => this.FindAncestorOfType<ToolTabStrip>());
        _dragHelper.Attach();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        RemoveHandler(PointerPressedEvent, PressedHandler);
        RemoveHandler(DoubleTappedEvent, DoubleTappedHandler);

        _dragHelper?.Detach();
        _dragHelper = null;
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
    }

    private void DoubleTappedHandler(object? sender, TappedEventArgs e)
    {
        if (DataContext is IDockable { Owner: IDock { Factory: { } factory }, CanFloat: true } dockable)
        {
            factory.FloatDockable(dockable);
        }
    }
}

