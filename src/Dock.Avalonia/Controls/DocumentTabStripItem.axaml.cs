// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStripItem custom control.
/// </summary>
[PseudoClasses(":active")]
public class DocumentTabStripItem : TabStripItem
{
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

    private bool _pressed;
    private bool _detached;
    private Point _start;
        
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
                if (this.FindAncestorOfType<DocumentTabStrip>() is { } tabStrip)
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
