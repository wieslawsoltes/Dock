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
/// Interaction logic for <see cref="ToolPinItemControl"/> xaml.
/// </summary>
public class ToolPinItemControl : TemplatedControl
{
    private ItemDragHelper? _dragHelper;

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<AvaloniaOrientation> OrientationProperty =
        AvaloniaProperty.Register<ToolPinItemControl, AvaloniaOrientation>(nameof(Orientation), AvaloniaOrientation.Vertical);

    /// <summary>
    /// Gets or sets the orientation in which control will be layed out.
    /// </summary>
    public AvaloniaOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Define the <see cref="PinContextMenu"/> property.
    /// </summary>
    public static readonly StyledProperty<ContextMenu?> PinContextMenuProperty =
        AvaloniaProperty.Register<ToolPinItemControl, ContextMenu?>(nameof(PinContextMenu));

    /// <summary>
    /// Gets or sets the pin context menu.
    /// </summary>
    public ContextMenu? PinContextMenu
    {
        get => GetValue(PinContextMenuProperty);
        set => SetValue(PinContextMenuProperty, value);
    }


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

    private void StartDockDrag(PointerEventArgs startArgs, PointerEventArgs e)
    {
        var dockControl = this.FindAncestorOfType<DockControl>();
        if (dockControl?.Layout?.Factory?.DockControls is { } dockControls
            && dockControl.DockControlState is DockControlState state)
        {
            var startPosition = startArgs.GetPosition(dockControl);
            var position = e.GetPosition(dockControl);
            var action = ToDragAction(e);
            state.StartDrag(this, startPosition, position, dockControl);
            state.Process(position, default, EventType.Moved, action, dockControl, dockControls);
        }
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _dragHelper = new ItemDragHelper(
            this,
            () => Parent as ItemsControl,
            () => Orientation,
            dragOutside: StartDockDrag,
            getBoundsContainer: () => this.FindAncestorOfType<ToolPinnedControl>());
        _dragHelper.Attach();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _dragHelper?.Detach();
        _dragHelper = null;
    }
}
