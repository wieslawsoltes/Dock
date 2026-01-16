// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model.Controls;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Overlay splitter control that resizes adjacent overlay panels.
/// </summary>
public class OverlaySplitterControl : ProportionalStackPanelSplitter
{
    private const double MinSize = 48.0;
    private IOverlaySplitter? _splitter;
    private INotifyPropertyChanged? _splitterNotifier;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(DragStartedEvent, OnDragStarted, RoutingStrategies.Bubble);
        AddHandler(DragDeltaEvent, OnDragDelta, RoutingStrategies.Bubble);
        AddHandler(DragCompletedEvent, OnDragCompleted, RoutingStrategies.Bubble);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        RemoveHandler(DragStartedEvent, OnDragStarted);
        RemoveHandler(DragDeltaEvent, OnDragDelta);
        RemoveHandler(DragCompletedEvent, OnDragCompleted);
        DetachSplitterHandlers();
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        DetachSplitterHandlers();

        _splitter = DataContext as IOverlaySplitter;
        if (_splitter is INotifyPropertyChanged notifier)
        {
            _splitterNotifier = notifier;
            _splitterNotifier.PropertyChanged += OnSplitterPropertyChanged;
        }

        UpdateSplitterAppearance();
    }

    private void DetachSplitterHandlers()
    {
        if (_splitterNotifier != null)
        {
            _splitterNotifier.PropertyChanged -= OnSplitterPropertyChanged;
            _splitterNotifier = null;
        }

        _splitter = null;
    }

    private void OnSplitterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IOverlaySplitter.Orientation)
            || e.PropertyName == nameof(IOverlaySplitter.Thickness))
        {
            UpdateSplitterAppearance();
        }
    }

    private void UpdateSplitterAppearance()
    {
        if (_splitter is null)
        {
            ClearValue(CursorProperty);
            ClearValue(WidthProperty);
            ClearValue(HeightProperty);
            ClearValue(ThicknessProperty);
            return;
        }

        IsResizingEnabled = false;
        Thickness = _splitter.Thickness;

        if (_splitter.Orientation == Dock.Model.Core.Orientation.Horizontal)
        {
            Cursor = new Cursor(StandardCursorType.SizeWestEast);
            Width = _splitter.Thickness;
            Height = double.NaN;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
            Width = double.NaN;
            Height = _splitter.Thickness;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }
    }

    private void OnDragStarted(object? sender, VectorEventArgs e)
    {
        if (DataContext is IOverlaySplitter splitter)
        {
            splitter.IsDragging = true;
        }
    }

    private void OnDragDelta(object? sender, VectorEventArgs e)
    {
        if (DataContext is not IOverlaySplitter splitter)
        {
            return;
        }

        if (!splitter.CanResize)
        {
            return;
        }

        var before = splitter.PanelBefore;
        var after = splitter.PanelAfter;

        if (before is null || after is null)
        {
            return;
        }

        switch (splitter.Orientation)
        {
            case Dock.Model.Core.Orientation.Horizontal:
                ResizeHorizontal(splitter, before, after, e.Vector.X);
                break;
            case Dock.Model.Core.Orientation.Vertical:
                ResizeVertical(splitter, before, after, e.Vector.Y);
                break;
        }
    }

    private static void ResizeHorizontal(IOverlaySplitter splitter, IOverlayPanel before, IOverlayPanel after, double deltaX)
    {
        var minBefore = Math.Max(splitter.MinSizeBefore, MinSize);
        var minAfter = Math.Max(splitter.MinSizeAfter, MinSize);

        var nextBefore = before.Width + deltaX;
        var nextAfter = after.Width - deltaX;

        if (nextBefore < minBefore || nextAfter < minAfter)
        {
            return;
        }

        before.Width = nextBefore;
        after.Width = nextAfter;
        after.X += deltaX;

        UpdateProportions(before, after, nextBefore, nextAfter);
    }

    private static void ResizeVertical(IOverlaySplitter splitter, IOverlayPanel before, IOverlayPanel after, double deltaY)
    {
        var minBefore = Math.Max(splitter.MinSizeBefore, MinSize);
        var minAfter = Math.Max(splitter.MinSizeAfter, MinSize);

        var nextBefore = before.Height + deltaY;
        var nextAfter = after.Height - deltaY;

        if (nextBefore < minBefore || nextAfter < minAfter)
        {
            return;
        }

        before.Height = nextBefore;
        after.Height = nextAfter;
        after.Y += deltaY;

        UpdateProportions(before, after, nextBefore, nextAfter);
    }

    private static void UpdateProportions(IOverlayPanel before, IOverlayPanel after, double sizeBefore, double sizeAfter)
    {
        var total = sizeBefore + sizeAfter;
        if (total <= 0)
        {
            return;
        }

        // If panels support Proportion (IDockable), keep them in sync with layout sizes.
        before.Proportion = sizeBefore / total;
        after.Proportion = sizeAfter / total;
    }

    private void OnDragCompleted(object? sender, VectorEventArgs e)
    {
        if (DataContext is IOverlaySplitter splitter)
        {
            splitter.IsDragging = false;
        }
    }
}
