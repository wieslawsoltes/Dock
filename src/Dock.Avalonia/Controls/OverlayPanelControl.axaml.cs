// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Dock.Settings;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Overlay panel control that supports move/resize gestures over a canvas host.
/// </summary>
[TemplatePart("PART_MoveThumb", typeof(Thumb))]
[TemplatePart("PART_ResizeThumb", typeof(Thumb))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_Header", typeof(Control))]
[TemplatePart("PART_DockDragHandle", typeof(Control))]
public class OverlayPanelControl : TemplatedControl
{
    private const double MinSize = 48.0;
    private const double HiddenScale = 0.9;

    private Thumb? _moveThumb;
    private Thumb? _resizeThumb;
    private Control? _header;
    private Control? _dockDragHandle;
    private IOverlayPanel? _panel;
    private INotifyPropertyChanged? _panelNotifier;
    private CancellationTokenSource? _visibilityCts;
    private readonly TranslateTransform _translateTransform = new();
    private readonly ScaleTransform _scaleTransform = new(1, 1);
    private readonly TransformGroup _transformGroup = new();

    /// <summary>
    /// Defines the <see cref="VisibilityOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> VisibilityOpacityProperty =
        AvaloniaProperty.Register<OverlayPanelControl, double>(nameof(VisibilityOpacity), 1.0);

    /// <summary>
    /// Gets or sets the animation opacity applied to the panel chrome.
    /// </summary>
    public double VisibilityOpacity
    {
        get => GetValue(VisibilityOpacityProperty);
        set => SetValue(VisibilityOpacityProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();
        EnsureRenderTransforms();

        _moveThumb = e.NameScope.Find<Thumb>("PART_MoveThumb");
        _resizeThumb = e.NameScope.Find<Thumb>("PART_ResizeThumb");
        _header = e.NameScope.Find<Control>("PART_Header");
        _dockDragHandle = e.NameScope.Find<Control>("PART_DockDragHandle");

        if (_moveThumb != null)
        {
            _moveThumb.AddHandler(Thumb.DragDeltaEvent, MoveThumbOnDragDelta, RoutingStrategies.Bubble);
            _moveThumb.AddHandler(Thumb.DragCompletedEvent, MoveThumbOnDragCompleted, RoutingStrategies.Bubble);
            _moveThumb.DoubleTapped += MoveThumbOnDoubleTapped;
        }

        if (_resizeThumb != null)
        {
            _resizeThumb.AddHandler(Thumb.DragDeltaEvent, ResizeThumbOnDragDelta, RoutingStrategies.Bubble);
        }

        if (_dockDragHandle != null)
        {
            Dock.Settings.DockProperties.SetIsDragArea(_dockDragHandle, true);
        }

        UpdateVisibility(allowAnimation: false);
    }

    /// <inheritdoc />
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        DetachPanelHandlers();

        _panel = DataContext as IOverlayPanel;
        _panelNotifier = _panel as INotifyPropertyChanged;
        if (_panelNotifier != null)
        {
            _panelNotifier.PropertyChanged += OnPanelPropertyChanged;
        }

        UpdateVisibility(allowAnimation: false);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachPanelHandlers();
        DetachHandlers();
    }

    private void DetachHandlers()
    {
        if (_moveThumb != null)
        {
            _moveThumb.RemoveHandler(Thumb.DragDeltaEvent, MoveThumbOnDragDelta);
            _moveThumb.RemoveHandler(Thumb.DragCompletedEvent, MoveThumbOnDragCompleted);
            _moveThumb.DoubleTapped -= MoveThumbOnDoubleTapped;
            _moveThumb = null;
        }

        if (_resizeThumb != null)
        {
            _resizeThumb.RemoveHandler(Thumb.DragDeltaEvent, ResizeThumbOnDragDelta);
            _resizeThumb = null;
        }

        _header = null;
        _dockDragHandle = null;
    }

    private void DetachPanelHandlers()
    {
        if (_panelNotifier != null)
        {
            _panelNotifier.PropertyChanged -= OnPanelPropertyChanged;
            _panelNotifier = null;
        }

        _panel = null;
        _visibilityCts?.Cancel();
        _visibilityCts = null;
    }

    private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IOverlayPanel.Visibility)
            || e.PropertyName == nameof(IOverlayPanel.AnimateVisibility)
            || e.PropertyName == nameof(IOverlayPanel.VisibilityAnimation)
            || e.PropertyName == nameof(IOverlayPanel.VisibilityAnimationDuration))
        {
            UpdateVisibility(allowAnimation: true);
        }
    }

    private void EnsureRenderTransforms()
    {
        if (_transformGroup.Children.Count == 0)
        {
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
        }

        RenderTransform = _transformGroup;
        RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
    }

    private void UpdateVisibility(bool allowAnimation)
    {
        if (_panel == null)
        {
            return;
        }

        var show = _panel.Visibility == OverlayVisibility.Visible;
        var animation = _panel.VisibilityAnimation;
        var duration = _panel.VisibilityAnimationDuration;

        var animate = allowAnimation
            && _panel.AnimateVisibility
            && animation != OverlayVisibilityAnimation.None
            && duration > 0;

        ApplyVisibility(show, animation, animate, duration);
    }

    private async void ApplyVisibility(bool show, OverlayVisibilityAnimation animation, bool animate, double durationMs)
    {
        _visibilityCts?.Cancel();
        _visibilityCts = new CancellationTokenSource();
        var token = _visibilityCts.Token;

        if (!animate)
        {
            DisableTransitions();
            SetVisibleState(show);
            IsVisible = show;
            return;
        }

        var duration = TimeSpan.FromMilliseconds(Math.Max(durationMs, 0));
        if (duration <= TimeSpan.Zero)
        {
            DisableTransitions();
            SetVisibleState(show);
            IsVisible = show;
            return;
        }

        var size = Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
        {
            size = new Size(
                Math.Max(size.Width, 0),
                Math.Max(size.Height, 0));
        }

        if (show)
        {
            DisableTransitions();
            SetHiddenState(animation, size);
            IsVisible = true;
            ConfigureTransitions(duration);
            SetVisibleState(true);
            return;
        }

        ConfigureTransitions(duration);
        SetHiddenState(animation, size);

        try
        {
            await Task.Delay(duration, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (!token.IsCancellationRequested)
        {
            IsVisible = false;
        }
    }

    private void ConfigureTransitions(TimeSpan duration)
    {
        Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = VisibilityOpacityProperty,
                Duration = duration
            }
        };

        _translateTransform.Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = TranslateTransform.XProperty,
                Duration = duration
            },
            new DoubleTransition
            {
                Property = TranslateTransform.YProperty,
                Duration = duration
            }
        };

        _scaleTransform.Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = ScaleTransform.ScaleXProperty,
                Duration = duration
            },
            new DoubleTransition
            {
                Property = ScaleTransform.ScaleYProperty,
                Duration = duration
            }
        };
    }

    private void DisableTransitions()
    {
        Transitions = null;
        _translateTransform.Transitions = null;
        _scaleTransform.Transitions = null;
    }

    private void SetVisibleState(bool show)
    {
        if (show)
        {
            VisibilityOpacity = 1;
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
            _scaleTransform.ScaleX = 1;
            _scaleTransform.ScaleY = 1;
        }
        else
        {
            VisibilityOpacity = 0;
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
            _scaleTransform.ScaleX = 1;
            _scaleTransform.ScaleY = 1;
        }
    }

    private void SetHiddenState(OverlayVisibilityAnimation animation, Size size)
    {
        var fade = animation == OverlayVisibilityAnimation.Fade
            || animation == OverlayVisibilityAnimation.FadeSlide;

        var slideOffset = GetSlideOffset(animation, size);

        VisibilityOpacity = fade ? 0 : 1;
        _translateTransform.X = slideOffset.X;
        _translateTransform.Y = slideOffset.Y;

        if (animation == OverlayVisibilityAnimation.Scale)
        {
            _scaleTransform.ScaleX = HiddenScale;
            _scaleTransform.ScaleY = HiddenScale;
        }
        else
        {
            _scaleTransform.ScaleX = 1;
            _scaleTransform.ScaleY = 1;
        }
    }

    private static Vector GetSlideOffset(OverlayVisibilityAnimation animation, Size size)
    {
        return animation switch
        {
            OverlayVisibilityAnimation.SlideLeft => new Vector(-size.Width, 0),
            OverlayVisibilityAnimation.SlideRight => new Vector(size.Width, 0),
            OverlayVisibilityAnimation.SlideTop => new Vector(0, -size.Height),
            OverlayVisibilityAnimation.SlideBottom => new Vector(0, size.Height),
            OverlayVisibilityAnimation.FadeSlide => new Vector(0, size.Height),
            _ => default
        };
    }

    private void MoveThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        if (DataContext is not IOverlayPanel panel)
        {
            return;
        }

        if (panel.IsPositionLocked)
        {
            return;
        }

        if (panel.Owner is not IOverlayDock overlayDock || !overlayDock.AllowPanelDragging)
        {
            return;
        }

        panel.X += e.Vector.X;
        panel.Y += e.Vector.Y;
        panel.IsDragging = true;
        BringPanelToFront(overlayDock, panel);
        ApplySnap(overlayDock, panel);
    }

    private void MoveThumbOnDragCompleted(object? sender, VectorEventArgs e)
    {
        if (DataContext is not IOverlayPanel panel)
        {
            return;
        }

        panel.IsDragging = false;
    }

    private void MoveThumbOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not IOverlayPanel panel)
        {
            return;
        }

        if (!panel.FloatOnDoubleClick || !panel.CanFloat)
        {
            return;
        }

        if (panel.Owner?.Factory is { } factory)
        {
            factory.FloatDockable(panel);
        }
    }

    private void ResizeThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        if (DataContext is not IOverlayPanel panel)
        {
            return;
        }

        if (panel.IsSizeLocked)
        {
            return;
        }

        if (panel.Owner is not IOverlayDock overlayDock || !overlayDock.AllowPanelResizing)
        {
            return;
        }

        var nextWidth = Math.Max(panel.Width + e.Vector.X, MinSize);
        var nextHeight = Math.Max(panel.Height + e.Vector.Y, MinSize);

        panel.Width = nextWidth;
        panel.Height = nextHeight;
        ApplySnap(overlayDock, panel);
    }

    private static void BringPanelToFront(IOverlayDock overlayDock, IOverlayPanel panel)
    {
        if (overlayDock.OverlayPanels is null)
        {
            return;
        }

        var ordered = overlayDock.OverlayPanels.Where(p => p != null).OrderBy(p => p!.ZIndex).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i]!.ZIndex = i;
        }

        var maxZ = ordered.Count == 0 ? 0 : ordered.Max(p => p!.ZIndex);
        panel.ZIndex = maxZ + 1;
    }

    private void ApplySnap(IOverlayDock overlayDock, IOverlayPanel panel)
    {
        if (!overlayDock.EnableSnapToEdge && !overlayDock.EnableSnapToPanel)
        {
            return;
        }

        var canvas = this.FindAncestorOfType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        var threshold = overlayDock.SnapThreshold;

        var canvasWidth = canvas.Bounds.Width;
        var canvasHeight = canvas.Bounds.Height;

        if (overlayDock.EnableSnapToEdge)
        {
            if (Math.Abs(panel.X) <= threshold)
            {
                panel.X = 0;
            }

            if (Math.Abs((panel.X + panel.Width) - canvasWidth) <= threshold)
            {
                panel.X = canvasWidth - panel.Width;
            }

            if (Math.Abs(panel.Y) <= threshold)
            {
                panel.Y = 0;
            }

            if (Math.Abs((panel.Y + panel.Height) - canvasHeight) <= threshold)
            {
                panel.Y = canvasHeight - panel.Height;
            }
        }

        if (!overlayDock.EnableSnapToPanel || overlayDock.OverlayPanels is null)
        {
            return;
        }

        foreach (var other in overlayDock.OverlayPanels)
        {
            if (other == null || ReferenceEquals(other, panel))
            {
                continue;
            }

            var dxLeft = Math.Abs(panel.X - (other.X + other.Width));
            if (dxLeft <= threshold && Math.Abs(panel.Y - other.Y) <= threshold)
            {
                panel.X = other.X + other.Width;
            }

            var dxRight = Math.Abs((panel.X + panel.Width) - other.X);
            if (dxRight <= threshold && Math.Abs(panel.Y - other.Y) <= threshold)
            {
                panel.X = other.X - panel.Width;
            }

            var dyTop = Math.Abs(panel.Y - (other.Y + other.Height));
            if (dyTop <= threshold && Math.Abs(panel.X - other.X) <= threshold)
            {
                panel.Y = other.Y + other.Height;
            }

            var dyBottom = Math.Abs((panel.Y + panel.Height) - other.Y);
            if (dyBottom <= threshold && Math.Abs(panel.X - other.X) <= threshold)
            {
                panel.Y = other.Y - panel.Height;
            }
        }
    }
}
