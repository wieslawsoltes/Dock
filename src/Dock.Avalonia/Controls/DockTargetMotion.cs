// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace Dock.Avalonia.Controls;

/// <summary>
/// Provides compositor-backed motion helpers for dock target template parts.
/// </summary>
public sealed class DockTargetMotion : AvaloniaObject
{
    private static readonly CubicEaseOut s_easing = new();

    private static readonly AttachedProperty<MotionSubscription?> s_subscriptionProperty =
        AvaloniaProperty.RegisterAttached<DockTargetMotion, Control, MotionSubscription?>("Subscription");

    /// <summary>
    /// Defines the UseCompositorAnimations attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> UseCompositorAnimationsProperty =
        AvaloniaProperty.RegisterAttached<DockTargetMotion, Control, bool>("UseCompositorAnimations");

    /// <summary>
    /// Defines the ActiveScale attached property.
    /// </summary>
    public static readonly AttachedProperty<double> ActiveScaleProperty =
        AvaloniaProperty.RegisterAttached<DockTargetMotion, Control, double>("ActiveScale", 1.0);

    /// <summary>
    /// Defines the InactiveScale attached property.
    /// </summary>
    public static readonly AttachedProperty<double> InactiveScaleProperty =
        AvaloniaProperty.RegisterAttached<DockTargetMotion, Control, double>("InactiveScale", 0.985);

    static DockTargetMotion()
    {
        UseCompositorAnimationsProperty.Changed.AddClassHandler<Control>(OnUseCompositorAnimationsChanged);
        ActiveScaleProperty.Changed.AddClassHandler<Control>((control, _) => UpdateScale(control));
        InactiveScaleProperty.Changed.AddClassHandler<Control>((control, _) => UpdateScale(control));
    }

    private DockTargetMotion()
    {
    }

    /// <summary>
    /// Gets whether compositor animations are enabled for the control.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <returns>The current value.</returns>
    public static bool GetUseCompositorAnimations(Control control)
    {
        return control.GetValue(UseCompositorAnimationsProperty);
    }

    /// <summary>
    /// Sets whether compositor animations are enabled for the control.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <param name="value">The new value.</param>
    public static void SetUseCompositorAnimations(Control control, bool value)
    {
        control.SetValue(UseCompositorAnimationsProperty, value);
    }

    /// <summary>
    /// Gets the scale used when the control is active.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <returns>The active scale.</returns>
    public static double GetActiveScale(Control control)
    {
        return control.GetValue(ActiveScaleProperty);
    }

    /// <summary>
    /// Sets the scale used when the control is active.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <param name="value">The active scale.</param>
    public static void SetActiveScale(Control control, double value)
    {
        control.SetValue(ActiveScaleProperty, value);
    }

    /// <summary>
    /// Gets the scale used when the control is inactive.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <returns>The inactive scale.</returns>
    public static double GetInactiveScale(Control control)
    {
        return control.GetValue(InactiveScaleProperty);
    }

    /// <summary>
    /// Sets the scale used when the control is inactive.
    /// </summary>
    /// <param name="control">The target control.</param>
    /// <param name="value">The inactive scale.</param>
    public static void SetInactiveScale(Control control, double value)
    {
        control.SetValue(InactiveScaleProperty, value);
    }

    private static void OnUseCompositorAnimationsChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool enabled)
        {
            return;
        }

        DisposeSubscription(control);

        if (!enabled)
        {
            return;
        }

        var subscription = new MotionSubscription(control);
        control.SetValue(s_subscriptionProperty, subscription);

        if (control.IsAttachedToVisualTree())
        {
            Apply(control);
        }
    }

    private static void Apply(Control control)
    {
        if (!GetUseCompositorAnimations(control) || !control.IsAttachedToVisualTree())
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(control);
        if (visual is null)
        {
            return;
        }

        var compositor = visual.Compositor;
        if (compositor is null)
        {
            return;
        }

        var animations = compositor.CreateImplicitAnimationCollection();

        var opacity = compositor.CreateScalarKeyFrameAnimation();
        opacity.Target = nameof(CompositionVisual.Opacity);
        opacity.Duration = TimeSpan.FromMilliseconds(120);
        opacity.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_easing);
        opacity.InsertExpressionKeyFrame(1.0f, "this.FinalValue", s_easing);
        animations.Insert(nameof(CompositionVisual.Opacity), opacity);

        var scale = compositor.CreateVector3DKeyFrameAnimation();
        scale.Target = nameof(CompositionVisual.Scale);
        scale.Duration = TimeSpan.FromMilliseconds(140);
        scale.InsertExpressionKeyFrame(0.0f, "this.StartingValue", s_easing);
        scale.InsertExpressionKeyFrame(1.0f, "this.FinalValue", s_easing);
        animations.Insert(nameof(CompositionVisual.Scale), scale);

        visual.ImplicitAnimations = animations;
        UpdateCenterPoint(control);
        UpdateScale(control);
    }

    private static void UpdateCenterPoint(Control control)
    {
        if (!GetUseCompositorAnimations(control) || !control.IsAttachedToVisualTree())
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(control);
        if (visual is null)
        {
            return;
        }

        var bounds = control.Bounds;
        visual.CenterPoint = new Vector3D(bounds.Width / 2.0, bounds.Height / 2.0, 0.0);
    }

    private static void UpdateScale(Control control)
    {
        if (!GetUseCompositorAnimations(control) || !control.IsAttachedToVisualTree())
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(control);
        if (visual is null)
        {
            return;
        }

        var scale = control.Opacity > 0
            ? GetActiveScale(control)
            : GetInactiveScale(control);

        visual.Scale = new Vector3D(scale, scale, 1.0);
    }

    private static void DisposeSubscription(Control control)
    {
        var subscription = control.GetValue(s_subscriptionProperty);
        if (subscription is null)
        {
            return;
        }

        subscription.Dispose();
        control.SetValue(s_subscriptionProperty, null);
    }

    private sealed class MotionSubscription : IDisposable
    {
        private readonly Control _control;
        private readonly IDisposable _opacitySubscription;
        private readonly IDisposable _boundsSubscription;

        public MotionSubscription(Control control)
        {
            _control = control;
            _opacitySubscription = control.GetObservable(Visual.OpacityProperty)
                .Subscribe(new AnonymousObserver<double>(_ => UpdateScale(control)));
            _boundsSubscription = control.GetObservable(Visual.BoundsProperty)
                .Subscribe(new AnonymousObserver<Rect>(_ => UpdateCenterPoint(control)));
            control.AttachedToVisualTree += OnAttachedToVisualTree;
            control.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        public void Dispose()
        {
            _control.AttachedToVisualTree -= OnAttachedToVisualTree;
            _control.DetachedFromVisualTree -= OnDetachedFromVisualTree;
            _opacitySubscription.Dispose();
            _boundsSubscription.Dispose();
        }

        private static void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
            {
                Apply(control);
            }
        }

        private static void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is Control control)
            {
                var visual = ElementComposition.GetElementVisual(control);
                if (visual is not null)
                {
                    visual.ImplicitAnimations = null;
                }
            }
        }
    }
}
