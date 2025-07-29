// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using AvaloniaOrientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStripItem custom control.
/// </summary>
[PseudoClasses(":active")]
public class DocumentTabStripItem : TabStripItem
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
    /// <summary>
    /// Define the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<DocumentTabStripItem, bool>(nameof(IsActive));

    /// <summary>
    /// Gets or sets if this is the currently active dockable.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Define the <see cref="DocumentContextMenu"/> property.
    /// </summary>
    public static readonly StyledProperty<ContextMenu?> DocumentContextMenuProperty =
        AvaloniaProperty.Register<DocumentTabStripItem, ContextMenu?>(nameof(DocumentContextMenu));

    /// <summary>
    /// Gets or sets the document context menu.
    /// </summary>
    public ContextMenu? DocumentContextMenu
    {
        get => GetValue(DocumentContextMenuProperty);
        set => SetValue(DocumentContextMenuProperty, value);
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DocumentTabStripItem);

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentTabStripItem"/> class.
    /// </summary>
    public DocumentTabStripItem()
    {
        UpdatePseudoClasses(IsActive);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        AddHandler(PointerPressedEvent, PressedHandler, RoutingStrategies.Tunnel);
        AddHandler(DoubleTappedEvent, DoubleTappedHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        _dragHelper = new ItemDragHelper(
            this,
            () => Parent as ItemsControl,
            () => (Parent as DocumentTabStrip)?.Orientation ?? AvaloniaOrientation.Horizontal,
            dragOutside: StartDockDrag,
            getBoundsContainer: () => this.FindAncestorOfType<DocumentTabStrip>());
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

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<bool>());
        }
    }

    private void UpdatePseudoClasses(bool isActive)
    {
        PseudoClasses.Set(":active", isActive);
    }
}

