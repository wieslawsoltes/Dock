// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Avalonia.Internal;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Document TabStripItem custom control.
/// </summary>
[PseudoClasses(":active")]
public class DocumentTabStripItem : TabStripItem
{
    private bool _pointerCaptured;
    private bool _isDragging;
    private Point _dragStartPoint;
    private int _startIndex;
    private DocumentTabStrip? _tabStrip;
    private TranslateTransform? _dragTransform;
    private const double FloatThreshold = 40;
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
            return;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _tabStrip = this.FindAncestorOfType<DocumentTabStrip>();
            if (_tabStrip is { } ts && ts.ChromeTabDrag)
            {
                _pointerCaptured = e.Pointer.Capture(this);
                _dragStartPoint = e.GetPosition(ts);
                _startIndex = ts.IndexFromContainer(this);
                _dragTransform = new TranslateTransform();
                RenderTransform = _dragTransform;
                _isDragging = false;
                e.Handled = true;
            }
        }
    }

    private void MovedHandler(object? sender, PointerEventArgs e)
    {
        if (!_pointerCaptured || _tabStrip is null)
            return;

        var position = e.GetPosition(_tabStrip);
        var delta = position - _dragStartPoint;

        if (_dragTransform is { } transform)
        {
            if (_tabStrip.Orientation == Avalonia.Layout.Orientation.Horizontal)
                transform.X = delta.X;
            else
                transform.Y = delta.Y;
        }

        if (!_isDragging)
        {
            if (!DockSettings.IsMinimumDragDistance(delta))
                return;
            _isDragging = true;
        }

        if (_isDragging && Math.Abs(delta.Y) > FloatThreshold)
        {
            FloatDockable(e);
            return;
        }

        var newIndex = GetIndexForPosition(position);
        if (newIndex != _startIndex && newIndex >= 0)
        {
            MoveDockable(newIndex);
            _startIndex = newIndex;
            _dragStartPoint = position;
            if (_dragTransform is { } t)
            {
                t.X = 0;
                t.Y = 0;
            }
        }
    }

    private void ReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        if (_pointerCaptured)
        {
            _pointerCaptured = false;
            _isDragging = false;
            _tabStrip = null;
            if (_dragTransform is { })
            {
                _dragTransform.X = 0;
                _dragTransform.Y = 0;
                RenderTransform = null;
                _dragTransform = null;
            }
            e.Pointer.Capture(null);
        }
    }

    private void FloatDockable(PointerEventArgs e)
    {
        if (_tabStrip?.DataContext is IDocumentDock { Factory: { } factory } dock &&
            DataContext is IDockable dockable)
        {
            var screen = this.PointToScreen(e.GetPosition(this));
            dockable.SetPointerScreenPosition(screen.X, screen.Y);
            factory.FloatDockable(dockable);
        }
        _pointerCaptured = false;
        _isDragging = false;
        _tabStrip = null;
        if (_dragTransform is { })
        {
            _dragTransform.X = 0;
            _dragTransform.Y = 0;
            RenderTransform = null;
            _dragTransform = null;
        }
        e.Pointer.Capture(null);
    }

    private int GetIndexForPosition(Point pos)
    {
        if (_tabStrip is null)
            return -1;

        for (var i = 0; i < _tabStrip.ItemCount; i++)
        {
            if (_tabStrip.ContainerFromIndex(i) is Control item)
            {
                var bounds = item.Bounds;
                if (pos.X < bounds.X + bounds.Width / 2)
                    return i;
            }
        }

        return _tabStrip.ItemCount - 1;
    }

    private void MoveDockable(int index)
    {
        if (_tabStrip?.DataContext is IDocumentDock { VisibleDockables: { } list, Factory: { } factory } dock &&
            DataContext is IDockable dockable)
        {
            if (index >= list.Count)
                index = list.Count - 1;

            var target = list[index];
            if (!ReferenceEquals(target, dockable))
            {
                factory.MoveDockable(dock, dockable, target);
            }
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

