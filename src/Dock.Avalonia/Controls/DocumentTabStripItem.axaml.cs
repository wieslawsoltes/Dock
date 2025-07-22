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
using Avalonia.Styling;
using Dock.Model.Core;
using Dock.Settings;
using System.Linq;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStripItem custom control.
/// </summary>
[PseudoClasses(":active")]
public class DocumentTabStripItem : TabStripItem
{
    private bool PointerPressed { get; set; }
    private bool _dragging;
    private Point _dragStartPoint;
    private DocumentTabStrip? _tabStrip;
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
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            if (DataContext is IDockable { Owner: IDock { Factory: { } factory }, CanClose: true } dockable)
            {
                factory.CloseDockable(dockable);
            }
        }
        else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _tabStrip = this.FindAncestorOfType<DocumentTabStrip>();
            if (_tabStrip is { })
            {
                _dragStartPoint = e.GetPosition(_tabStrip);
                PointerPressed = true;
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        if (!PointerPressed || _tabStrip is null)
        {
            return;
        }

        var point = e.GetPosition(_tabStrip);
        var delta = point - _dragStartPoint;

        if (!_dragging)
        {
            if (Math.Abs(delta.X) > Dock.Settings.DockSettings.MinimumHorizontalDragDistance ||
                Math.Abs(delta.Y) > Dock.Settings.DockSettings.MinimumVerticalDragDistance)
            {
                _tabStrip.StartItemDrag(this);
                _dragging = true;
            }
        }

        if (_dragging)
        {
            _tabStrip.UpdateItemDrag(this, delta, point);
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging && _tabStrip is { })
        {
            _tabStrip.EndItemDrag(this);
        }

        PointerPressed = false;
        _dragging = false;
        _tabStrip = null;
        e.Pointer.Capture(null);
    }

    private void CaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragging && _tabStrip is { })
        {
            _tabStrip.EndItemDrag(this);
        }

        PointerPressed = false;
        _dragging = false;
        _tabStrip = null;
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
