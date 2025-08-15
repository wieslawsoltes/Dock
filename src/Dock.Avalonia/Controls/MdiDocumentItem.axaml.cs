// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Avalonia.Input;
using Dock.Model.Core;
using Avalonia.Reactive;
using Avalonia.Media;
using Avalonia.Styling;

namespace Dock.Avalonia.Controls;

/// <summary>
/// A simple window-like content presenter for MDI documents.
/// Supports dragging and resizing, and stores bounds through IDockable tracking.
/// </summary>
    public class MdiDocumentItem : ContentControl
{
        private static int s_nextZIndex;
    public static readonly StyledProperty<bool> IsMaximizedProperty =
        AvaloniaProperty.Register<MdiDocumentItem, bool>(nameof(IsMaximized));

    public static readonly StyledProperty<bool> IsMinimizedProperty =
        AvaloniaProperty.Register<MdiDocumentItem, bool>(nameof(IsMinimized));

    public static readonly StyledProperty<WindowState> WindowStateProperty =
        AvaloniaProperty.Register<MdiDocumentItem, WindowState>(nameof(WindowState), WindowState.Normal);

    public bool IsMaximized
    {
        get => GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    public bool IsMinimized
    {
        get => GetValue(IsMinimizedProperty);
        set => SetValue(IsMinimizedProperty, value);
    }

    public WindowState WindowState
    {
        get => GetValue(WindowStateProperty);
        set => SetValue(WindowStateProperty, value);
    }

    private Point _dragOffset;
    private bool _isDragging;
    private Thumb? _resizeThumb;
    private Rect? _restoreBounds;
    private IDisposable? _boundsSubscription;
    private Rect? _minimizedBounds;
    private const double SnapThreshold = 20.0;
    private const double IconSize = 64.0;
    private const double IconMargin = 8.0;

    static MdiDocumentItem()
    {
        IsMaximizedProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => x.OnIsMaximizedChanged());
        IsMinimizedProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => x.OnIsMinimizedChanged());
        WindowStateProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => x.OnWindowStateChanged());
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnPointerMoved, handledEventsToo: true);

        UpdateBoundsFromDockable();

        // Ensure maximized item tracks parent size changes when window resizes
        if (IsMaximized)
        {
            SubscribeToParentBounds();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resizeThumb = e.NameScope.Find<Thumb>("PART_ResizeThumb");
        if (_resizeThumb is not null)
        {
            _resizeThumb.DragDelta += OnResizeThumbDragDelta;
            _resizeThumb.DragCompleted += OnResizeThumbDragCompleted;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_resizeThumb is not null)
        {
            _resizeThumb.DragDelta -= OnResizeThumbDragDelta;
            _resizeThumb.DragCompleted -= OnResizeThumbDragCompleted;
            _resizeThumb = null;
        }
        _boundsSubscription?.Dispose();
        _boundsSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnResizeThumbDragDelta(object? sender, VectorEventArgs e)
    {
        if (IsMaximized)
            return;

        var newWidth = Math.Max(100, Width + e.Vector.X);
        var newHeight = Math.Max(80, Height + e.Vector.Y);
        Width = newWidth;
        Height = newHeight;
    }

    private void OnResizeThumbDragCompleted(object? sender, VectorEventArgs e)
    {
        SaveBoundsToDockable();
    }

    private void OnIsMaximizedChanged()
    {
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        if (IsMaximized)
        {
            // Save current bounds for restore
            var left = container is null ? Canvas.GetLeft(this) : Canvas.GetLeft(container);
            var top = container is null ? Canvas.GetTop(this) : Canvas.GetTop(container);
            _restoreBounds = new Rect(left, top, Bounds.Width, Bounds.Height);

            if (container is not null)
            {
                Canvas.SetLeft(container, 0);
                Canvas.SetTop(container, 0);
            }

            var size = canvas?.Bounds.Size ?? (container?.Bounds.Size ?? default);
            if (size != default)
            {
                Width = size.Width;
                Height = size.Height;
            }

            // Track parent bounds to keep maximized document resized with window
            SubscribeToParentBounds();
        }
        else if (_restoreBounds is Rect restore && container is not null)
        {
            // Stop tracking bounds when not maximized
            _boundsSubscription?.Dispose();
            _boundsSubscription = null;

            Canvas.SetLeft(container, restore.X);
            Canvas.SetTop(container, restore.Y);
            Width = restore.Width;
            Height = restore.Height;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateBoundsFromDockable();
    }

    private void UpdateBoundsFromDockable()
    {
        if (DataContext is IDockable dockable)
        {
            dockable.GetVisibleBounds(out var x, out var y, out var w, out var h);
            var container = this.Parent as Control;
            if (container is not null)
            {
                if (!double.IsNaN(x)) Canvas.SetLeft(container, x);
                if (!double.IsNaN(y)) Canvas.SetTop(container, y);
            }
            else
            {
                if (!double.IsNaN(x)) Canvas.SetLeft(this, x);
                if (!double.IsNaN(y)) Canvas.SetTop(this, y);
            }
            if (!double.IsNaN(w)) Width = w;
            if (!double.IsNaN(h)) Height = h;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsMaximized)
            return;

        // Do not start window drag when clicking caption buttons; allow drag from chrome background
        if (e.Source is Visual visual)
        {
            // Avoid dragging when clicking any button within the chrome or the resize thumb
            if (visual.FindAncestorOfType<Button>() is not null)
                return;
            if (visual.FindAncestorOfType<Thumb>() is not null)
                return;
        }

        var container = this.Parent as Control;
        if (container is null)
            return;

        // Bring this window to front and activate
        container.ZIndex = ++s_nextZIndex;
        if (DataContext is IDockable dockable && dockable.Owner is IDock owner && dockable.Factory is { } factory)
        {
            factory.SetActiveDockable(dockable);
            factory.SetFocusedDockable(owner, dockable);
        }

        _isDragging = true;
        // Use Canvas coordinates for stable dragging independent of moving container
        var canvas = container.Parent as Control; // ItemsPanel is Canvas
        var pressPos = e.GetPosition(canvas ?? container);
        var left = Canvas.GetLeft(container);
        var top = Canvas.GetTop(container);
        if (double.IsNaN(left)) left = 0;
        if (double.IsNaN(top)) top = 0;
        _dragOffset = new Point(pressPos.X - left, pressPos.Y - top);
        e.Pointer.Capture(this);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        e.Pointer.Capture(null);

        SaveBoundsToDockable();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;
        var container = this.Parent as Control;
        if (container is null) return;
        var canvas = container.Parent as Control;
        var p = e.GetPosition(canvas ?? container);
        var left = p.X - _dragOffset.X;
        var top = p.Y - _dragOffset.Y;
        
        // Apply window snapping
        if (canvas is not null)
        {
            var canvasBounds = canvas.Bounds;
            var windowWidth = Bounds.Width;
            var windowHeight = Bounds.Height;
            
            // Snap to left edge
            if (left <= SnapThreshold)
            {
                left = 0;
            }
            // Snap to right edge
            else if (left + windowWidth >= canvasBounds.Width - SnapThreshold)
            {
                left = canvasBounds.Width - windowWidth;
            }
            
            // Snap to top edge
            if (top <= SnapThreshold)
            {
                top = 0;
            }
            // Snap to bottom edge
            else if (top + windowHeight >= canvasBounds.Height - SnapThreshold)
            {
                top = canvasBounds.Height - windowHeight;
            }
        }
        
        Canvas.SetLeft(container, left);
        Canvas.SetTop(container, top);
    }

    private void SaveBoundsToDockable()
    {
        if (DataContext is IDockable dockable)
        {
            var container = this.Parent as Control;
            var left = container is null ? Canvas.GetLeft(this) : Canvas.GetLeft(container);
            var top = container is null ? Canvas.GetTop(this) : Canvas.GetTop(container);
            var w = Bounds.Width;
            var h = Bounds.Height;
            dockable.SetVisibleBounds(left, top, w, h);
        }
    }

    private void SubscribeToParentBounds()
    {
        _boundsSubscription?.Dispose();
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        var target = canvas ?? container;
        if (target is not null)
        {
            // Initialize to current size
            UpdateMaximizedSize();

            _boundsSubscription = target
                .GetObservable(BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ =>
                {
                    if (IsMaximized)
                    {
                        UpdateMaximizedSize();
                    }
                }));
        }
    }

    private void UpdateMaximizedSize()
    {
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        var size = canvas?.Bounds.Size ?? (container?.Bounds.Size ?? default);
        if (size != default)
        {
            Width = size.Width;
            Height = size.Height;
        }
    }

    private async void OnIsMinimizedChanged()
    {
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        
        if (IsMinimized)
        {
            // Save current bounds for restore
            var left = container is null ? Canvas.GetLeft(this) : Canvas.GetLeft(container);
            var top = container is null ? Canvas.GetTop(this) : Canvas.GetTop(container);
            _minimizedBounds = new Rect(left, top, Bounds.Width, Bounds.Height);
            
            // Position as icon at bottom of canvas
            var iconPosition = GetNextIconPosition();
            
            // Animate to minimized state
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Canvas.LeftProperty, left),
                            new Setter(Canvas.TopProperty, top),
                            new Setter(WidthProperty, Bounds.Width),
                            new Setter(HeightProperty, Bounds.Height),
                            new Setter(OpacityProperty, 1.0)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Canvas.LeftProperty, iconPosition.X),
                            new Setter(Canvas.TopProperty, iconPosition.Y),
                            new Setter(WidthProperty, IconSize),
                            new Setter(HeightProperty, IconSize),
                            new Setter(OpacityProperty, 0.8)
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
            
            if (container is not null)
            {
                await animation.RunAsync(container);
                Canvas.SetLeft(container, iconPosition.X);
                Canvas.SetTop(container, iconPosition.Y);
            }
            
            Width = IconSize;
            Height = IconSize;
            WindowState = WindowState.Minimized;
        }
        else if (_minimizedBounds is Rect restore && container is not null)
        {
            // Animate restore from minimized state
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Canvas.LeftProperty, Canvas.GetLeft(container)),
                            new Setter(Canvas.TopProperty, Canvas.GetTop(container)),
                            new Setter(WidthProperty, Width),
                            new Setter(HeightProperty, Height),
                            new Setter(OpacityProperty, 0.8)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(Canvas.LeftProperty, restore.X),
                            new Setter(Canvas.TopProperty, restore.Y),
                            new Setter(WidthProperty, restore.Width),
                            new Setter(HeightProperty, restore.Height),
                            new Setter(OpacityProperty, 1.0)
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
            
            await animation.RunAsync(container);
            Canvas.SetLeft(container, restore.X);
            Canvas.SetTop(container, restore.Y);
            Width = restore.Width;
            Height = restore.Height;
            WindowState = WindowState.Normal;
        }
    }

    private void OnWindowStateChanged()
    {
        switch (WindowState)
        {
            case WindowState.Normal:
                IsMaximized = false;
                IsMinimized = false;
                break;
            case WindowState.Minimized:
                IsMaximized = false;
                IsMinimized = true;
                break;
            case WindowState.Maximized:
                IsMaximized = true;
                IsMinimized = false;
                break;
        }
    }

    private Point GetNextIconPosition()
    {
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        if (canvas is null) return new Point(0, 0);
        
        var canvasBounds = canvas.Bounds;
        var bottomY = canvasBounds.Height - IconSize - IconMargin;
        
        // Find next available position from left to right
        var currentX = IconMargin;
        var siblings = canvas.GetVisualChildren().OfType<Control>();
        
        foreach (var sibling in siblings)
        {
            if (sibling != container && sibling.DataContext is IDockable siblingDockable)
            {
                var siblingItem = sibling.GetVisualChildren().OfType<MdiDocumentItem>().FirstOrDefault();
                if (siblingItem?.IsMinimized == true)
                {
                    var siblingLeft = Canvas.GetLeft(sibling);
                    if (Math.Abs(siblingLeft - currentX) < IconSize + IconMargin)
                    {
                        currentX = siblingLeft + IconSize + IconMargin;
                    }
                }
            }
        }
        
        return new Point(currentX, bottomY);
    }

    /// <summary>
    /// Minimizes the window to an icon.
    /// </summary>
    public void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Restores the window from minimized or maximized state.
    /// </summary>
    public void Restore()
    {
        WindowState = WindowState.Normal;
    }

    /// <summary>
    /// Maximizes the window to fill the canvas.
    /// </summary>
    public void Maximize()
    {
        WindowState = WindowState.Maximized;
    }
}


