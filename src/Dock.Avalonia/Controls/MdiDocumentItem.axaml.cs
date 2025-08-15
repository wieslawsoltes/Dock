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
using Avalonia.Interactivity;
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

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<MdiDocumentItem, bool>(nameof(IsActive));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MdiDocumentItem, string?>(nameof(Title));

    public static readonly StyledProperty<bool> CanCloseProperty =
        AvaloniaProperty.Register<MdiDocumentItem, bool>(nameof(CanClose), true);

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

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool CanClose
    {
        get => GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    private Point _dragOffset;
    private bool _isDragging;
    private Thumb? _resizeThumb;
    private Border? _minimizedBorder;
    private Button? _restoreButton;
    private Button? _closeButton;
    private Rect? _restoreBounds;
    private IDisposable? _boundsSubscription;
    private Rect? _minimizedBounds;
    private const double SnapThreshold = 20.0;
    private const double MinimizedWidth = 120.0;
    private const double MinimizedHeight = 32.0;
    private const double IconMargin = 8.0;

    static MdiDocumentItem()
    {
        IsMaximizedProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => x.OnIsMaximizedChanged());
        IsMinimizedProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => 
        {
            Console.WriteLine($"IsMinimized changed: Old={e.OldValue}, New={e.NewValue}");
            // Save bounds synchronously when minimizing to prevent timing issues
            if ((bool)e.NewValue! && !(bool)e.OldValue!)
            {
                Console.WriteLine("Calling SaveMinimizedBoundsSync");
                x.SaveMinimizedBoundsSync();
            }
            x.OnIsMinimizedChanged();
        });
        WindowStateProperty.Changed.AddClassHandler<MdiDocumentItem>((x, e) => 
        {
            Console.WriteLine($"WindowState changed: Old={e.OldValue}, New={e.NewValue}");
            x.OnWindowStateChanged();
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
        AddHandler(PointerReleasedEvent, OnPointerReleased, handledEventsToo: true);
        AddHandler(PointerMovedEvent, OnPointerMoved, handledEventsToo: true);

        // Auto-increment Z-index when attached to visual tree
        if (GetValue(Visual.ZIndexProperty) == 0) // Only set if not already set
        {
            SetValue(Visual.ZIndexProperty, ++s_nextZIndex);
        }

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

        _minimizedBorder = e.NameScope.Find<Border>("PART_MinimizedBorder");
        if (_minimizedBorder is not null)
        {
            _minimizedBorder.PointerPressed += OnMinimizedBorderPressed;
        }

        _restoreButton = e.NameScope.Find<Button>("PART_RestoreButton");
        if (_restoreButton is not null)
        {
            _restoreButton.Click += OnRestoreClicked;
        }

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClicked;
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
        if (_minimizedBorder is not null)
        {
            _minimizedBorder.PointerPressed -= OnMinimizedBorderPressed;
            _minimizedBorder = null;
        }
        if (_restoreButton is not null)
        {
            _restoreButton.Click -= OnRestoreClicked;
            _restoreButton = null;
        }
        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClicked;
            _closeButton = null;
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

    private void OnMinimizedBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        // Don't handle if the click is on a button
        if (e.Source is Control sourceControl)
        {
            if (sourceControl is Button || sourceControl.FindAncestorOfType<Button>() is not null)
            {
                return;
            }
        }

        // When clicking on minimized border, restore the document
        if (IsMinimized)
        {
            Restore();
            e.Handled = true;
        }
    }

    private void OnRestoreClicked(object? sender, RoutedEventArgs e)
    {
        if (IsMinimized)
        {
            Restore();
        }
        e.Handled = true;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDockable dockable && dockable.Owner is IDock { Factory: { } factory })
        {
            factory.CloseDockable(dockable);
        }
        e.Handled = true;
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
            _restoreBounds = new Rect(left, top, Width, Height);

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
        else if (_restoreBounds is Rect restore)
        {
            // Stop tracking bounds when not maximized
            _boundsSubscription?.Dispose();
            _boundsSubscription = null;

            if (container is not null)
            {
                Canvas.SetLeft(container, restore.X);
                Canvas.SetTop(container, restore.Y);
            }
            else
            {
                Canvas.SetLeft(this, restore.X);
                Canvas.SetTop(this, restore.Y);
            }
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
        
        // Update IDockable visible bounds when Canvas position changes
        if (DataContext is IDockable dockable)
        {
            dockable.SetVisibleBounds(left, top, Width, Height);
        }
    }

    private void SaveBoundsToDockable()
    {
        // Don't save bounds when minimized to preserve original bounds for restoration
        if (IsMinimized) return;
        
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

    private void SaveMinimizedBoundsSync()
    {
        var container = this.Parent as Control;
        
        // Save current visual bounds for restore - prefer IDockable bounds as they are more reliable
        var left = 0.0;
        var top = 0.0;
        var width = Width;
        var height = Height;
        
        // Try to get bounds from IDockable first, but only if they are valid
        if (DataContext is IDockable dockable)
        {
            dockable.GetVisibleBounds(out var dockableLeft, out var dockableTop, out var dockableWidth, out var dockableHeight);
            
            // Use IDockable bounds only if they are valid (not NaN)
            if (!double.IsNaN(dockableLeft) && !double.IsNaN(dockableTop))
            {
                left = dockableLeft;
                top = dockableTop;
                if (!double.IsNaN(dockableWidth)) width = dockableWidth;
                if (!double.IsNaN(dockableHeight)) height = dockableHeight;
            }
            else
            {
                // Fallback to Canvas position if IDockable bounds are invalid
                var canvasLeft = container is null ? Canvas.GetLeft(this) : Canvas.GetLeft(container);
                var canvasTop = container is null ? Canvas.GetTop(this) : Canvas.GetTop(container);
                
                // If Canvas.GetLeft/GetTop returns NaN, try to use the Bounds position as fallback
                if (!double.IsNaN(canvasLeft)) 
                {
                    left = canvasLeft;
                }
                else if (container is null)
                {
                    // Canvas.GetLeft returned NaN, use Bounds.X as fallback
                    left = Bounds.X;
                }
                
                if (!double.IsNaN(canvasTop)) 
                {
                    top = canvasTop;
                }
                else if (container is null)
                {
                    // Canvas.GetTop returned NaN, use Bounds.Y as fallback
                    top = Bounds.Y;
                }
            }
        }
        else
        {
            // Fallback to Canvas position if no IDockable
            var canvasLeft = container is null ? Canvas.GetLeft(this) : Canvas.GetLeft(container);
            var canvasTop = container is null ? Canvas.GetTop(this) : Canvas.GetTop(container);
            
            // Handle Canvas position with proper fallbacks
            if (!double.IsNaN(canvasLeft)) 
            {
                left = canvasLeft;
            }
            else if (container is null)
            {
                // Canvas.GetLeft returned NaN, try alternative approaches
                // First try to get the actual rendered position
                if (Bounds.X != 0 || Bounds.Y != 0)
                {
                    left = Bounds.X;
                }
                else
                {
                    // Last resort: check if we have a valid transform or use 0
                    left = 0;
                }
            }
            
            if (!double.IsNaN(canvasTop)) 
            {
                top = canvasTop;
            }
            else if (container is null)
            {
                // Canvas.GetTop returned NaN, try alternative approaches
                if (Bounds.X != 0 || Bounds.Y != 0)
                {
                    top = Bounds.Y;
                }
                else
                {
                    // Last resort: use 0
                    top = 0;
                }
            }
        }
        
        _minimizedBounds = new Rect(left, top, width, height);
        Console.WriteLine($"SaveMinimizedBoundsSync: Saved bounds ({left}, {top}, {width}, {height})");
    }

    private async void OnIsMinimizedChanged()
    {
        var container = this.Parent as Control;
        var canvas = container?.Parent as Control;
        
        if (IsMinimized)
        {
            // Bounds are now saved synchronously in SaveMinimizedBoundsSync()
            
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
                            new Setter(Canvas.LeftProperty, _minimizedBounds?.Left ?? 0),
                            new Setter(Canvas.TopProperty, _minimizedBounds?.Top ?? 0),
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
                            new Setter(WidthProperty, MinimizedWidth),
                            new Setter(HeightProperty, MinimizedHeight),
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
            
            Width = MinimizedWidth;
            Height = MinimizedHeight;
            WindowState = WindowState.Minimized;
        }
        else if (_minimizedBounds is Rect restore)
        {
            if (container is not null)
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
            }
            else
            {
                // Always restore to saved position, ignoring any moves during minimized state
                Canvas.SetLeft(this, restore.X);
                Canvas.SetTop(this, restore.Y);
            }
            
            // Ensure the item position is always set correctly, regardless of container
            Canvas.SetLeft(this, restore.X);
            Canvas.SetTop(this, restore.Y);
            
            Width = restore.Width;
            Height = restore.Height;
            // Only set to Normal if we're not transitioning to another state
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }
        else
        {
            // Fallback: restore to default size if no minimized bounds were saved
            Width = 300;
            Height = 200;
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
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
        var bottomY = canvasBounds.Height - MinimizedHeight - IconMargin;
        
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
                    if (Math.Abs(siblingLeft - currentX) < MinimizedWidth + IconMargin)
                    {
                        currentX = siblingLeft + MinimizedWidth + IconMargin;
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


