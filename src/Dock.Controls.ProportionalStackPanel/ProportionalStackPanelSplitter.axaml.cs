// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
/// </summary>
[PseudoClasses(":horizontal", ":vertical", ":preview")]
public class ProportionalStackPanelSplitter : Thumb
{
    static ProportionalStackPanelSplitter() => AffectsMeasure<ProportionalStackPanelSplitter>(ThicknessProperty);

    /// <summary>
    /// Defines the <see cref="Thickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, double>(nameof(Thickness), 4.0);

    /// <summary>
    /// Defines the <see cref="IsResizingEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizingEnabledProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, bool>(nameof(IsResizingEnabled), true);

    /// <summary>
    /// Defines the <see cref="PreviewResize"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> PreviewResizeProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, bool>(nameof(PreviewResize));

    /// <summary>Defines the preview brush, or null to use the panel's accent resource.</summary>
    public static readonly StyledProperty<IBrush?> PreviewBackgroundProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, IBrush?>(nameof(PreviewBackground));

    /// <summary>Defines the opacity of the resize preview.</summary>
    public static readonly StyledProperty<double> PreviewOpacityProperty =
        AvaloniaProperty.Register<ProportionalStackPanelSplitter, double>(nameof(PreviewOpacity), 1);

    /// <summary>
    /// Defines the MinimumProportionSize attached property.
    /// </summary>
    public static readonly AttachedProperty<double> MinimumProportionSizeProperty =
        AvaloniaProperty.RegisterAttached<ProportionalStackPanelSplitter, Control, double>("MinimumProportionSize", 75, true);

    /// <summary>
    /// Gets the value of the MinimumProportion attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <returns>The minimum size a proportion can be resized to.</returns>
    public static double GetMinimumProportionSize(AvaloniaObject control)
    {
        return control.GetValue(MinimumProportionSizeProperty);
    }

    /// <summary>
    /// Sets the value of the MinimumProportionSize attached property on the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="value">The minimum size a proportion can be resized to.</param>
    public static void SetMinimumProportionSize(AvaloniaObject control, double value)
    {
        control.SetValue(MinimumProportionSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness (height or width, depending on orientation).
    /// </summary>
    /// <value>The thickness.</value>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether resizing is enabled.
    /// </summary>
    public bool IsResizingEnabled
    {
        get => GetValue(IsResizingEnabledProperty);
        set => SetValue(IsResizingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether resizing only previews changes until released.
    /// </summary>
    public bool PreviewResize
    {
        get => GetValue(PreviewResizeProperty);
        set => SetValue(PreviewResizeProperty, value);
    }

    /// <summary>Gets or sets the preview brush. Null retains the panel's accent resource.</summary>
    public IBrush? PreviewBackground { get => GetValue(PreviewBackgroundProperty); set => SetValue(PreviewBackgroundProperty, value); }

    /// <summary>Gets or sets the resize preview opacity. Defaults to one.</summary>
    public double PreviewOpacity { get => GetValue(PreviewOpacityProperty); set => SetValue(PreviewOpacityProperty, value); }

    private IProportionalSplitterHost? _subscribedHost;
    private Point _startPoint;
    private bool _isMoving;
    private SplitterPreviewAdorner? _previewAdorner;
    private AdornerLayer? _adornerLayer;
    private double _startOffset;

    private void UpdatePreviewPseudoClass()
    {
        PseudoClasses.Set(":preview", PreviewResize && _isMoving);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsResizingEnabledProperty || change.Property == ThicknessProperty)
        {
            UpdateVisualState();
        }

        if (change.Property == PreviewResizeProperty)
        {
            UpdatePreviewPseudoClass();
        }

        if (_previewAdorner is not null && (change.Property == PreviewBackgroundProperty || change.Property == PreviewOpacityProperty))
        {
            _previewAdorner.Background = PreviewBackground;
            _previewAdorner.Opacity = PreviewOpacity;
            _previewAdorner.InvalidateVisual();
        }
    }

    internal static bool IsSplitter(Control? control, out ProportionalStackPanelSplitter? proportionalStackPanelSplitter)
    {
        if (control is ContentPresenter contentPresenter)
        {
            if (contentPresenter.Child is null)
            {
                contentPresenter.UpdateChild();
            }

            if (contentPresenter.Child is ProportionalStackPanelSplitter childSplitter)
            {
                proportionalStackPanelSplitter = childSplitter;
                return true;
            }
        }

        if (control is ProportionalStackPanelSplitter splitter)
        {
            proportionalStackPanelSplitter = splitter;
            return true;
        }

        proportionalStackPanelSplitter = null;
        return false;
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (IsResizingEnabled && GetPanel() is { } panel)
        {
            var point = e.GetPosition(panel.Panel);
            _startPoint = point;
            _isMoving = true;
            UpdatePreviewPseudoClass();

            if (PreviewResize)
            {
                var pos = this.TranslatePoint(new Point(), panel.Panel);
                if (pos is not null)
                {
                    _adornerLayer = AdornerLayer.GetAdornerLayer(panel.Panel);
                    if (_adornerLayer is not null)
                    {
                        _startOffset = panel.Orientation == Orientation.Vertical ? pos.Value.Y : pos.Value.X;
                        _previewAdorner = new SplitterPreviewAdorner
                        {
                            Orientation = panel.Orientation,
                            Thickness = Thickness,
                            Offset = _startOffset,
                            Background = PreviewBackground,
                            Opacity = PreviewOpacity,
                            [AdornerLayer.AdornedElementProperty] = panel.Panel
                        };
                        ((ISetLogicalParent)_previewAdorner).SetParent(panel.Panel);
                        _adornerLayer.Children.Add(_previewAdorner);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isMoving && IsResizingEnabled && GetPanel() is { } panel)
        {
            var point = e.GetPosition(panel.Panel);
            if (PreviewResize)
            {
                var delta = point - _startPoint;
                SetTargetProportion(panel.Orientation == Orientation.Vertical ? delta.Y : delta.X);
                if (_previewAdorner is not null && _adornerLayer is not null)
                {
                    _adornerLayer.Children.Remove(_previewAdorner);
                    ((ISetLogicalParent)_previewAdorner).SetParent(null);
                    _previewAdorner = null;
                    _adornerLayer = null;
                }
            }
        }

        _isMoving = false;
        UpdatePreviewPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isMoving && IsResizingEnabled)
        {
            if (GetPanel() is { } panel)
            {
                var point = e.GetPosition(panel.Panel);
                var delta = point - _startPoint;

                if (PreviewResize)
                {
                    if (_previewAdorner is not null)
                    {
                        var offset = _startOffset + (panel.Orientation == Orientation.Vertical ? delta.Y : delta.X);
                        _previewAdorner.Offset = offset;
                        _previewAdorner.InvalidateVisual();
                    }
                }
                else
                {
                    _startPoint = point;
                    SetTargetProportion(panel.Orientation == Orientation.Vertical ? delta.Y : delta.X);
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        CancelResize();
    }

    private void CancelResize()
    {
        if (_previewAdorner is not null && _adornerLayer is not null)
        {
            _adornerLayer.Children.Remove(_previewAdorner);
            ((ISetLogicalParent)_previewAdorner).SetParent(null);
            _previewAdorner = null;
            _adornerLayer = null;
        }

        _isMoving = false;
        UpdatePreviewPseudoClass();
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (GetPanel() is { } panel)
        {
            if (panel.Orientation == Orientation.Vertical)
            {
                return new Size(0, Thickness);
            }
            else
            {
                return new Size(Thickness, 0);
            }
        }

        return new Size();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var panel = GetPanel();
        if (panel is null)
        {
            return;
        }

        _subscribedHost = panel;
        panel.OrientationChanged += OnHostOrientationChanged;
        UpdateVisualState();
        UpdatePreviewPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_subscribedHost is { } host) host.OrientationChanged -= OnHostOrientationChanged;
        _subscribedHost = null;
        CancelResize();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnHostOrientationChanged(object? sender, EventArgs e)
    {
        CancelResize();
        UpdateVisualState();
        InvalidateMeasure();
    }

    private Control? FindNextChild(IProportionalSplitterHost panel)
    {
        return PanelNavigationHelper.GetSiblingInDirection(this, panel, 1);
    }

    private void SetTargetProportion(double dragDelta)
    {
        var panel = GetPanel();
        if (panel == null) return;

        var target = GetTargetElement(panel);
        if (target is null) return;

        var neighbor = FindNextChild(panel);
        if (neighbor is null) return;

        panel.Resize(target, neighbor, dragDelta, GetValue(MinimumProportionSizeProperty));
    }

    internal void UpdateVisualState()
    {
        if (GetPanel() is { } panel)
        {
            PanelNavigationHelper.UpdateControlVisualState(this, panel, Thickness, IsResizingEnabled);
            
            PseudoClasses.Set(":vertical", panel.Orientation == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", panel.Orientation == Orientation.Horizontal);
        }
    }

    private IProportionalSplitterHost? GetPanel()
    {
        return PanelNavigationHelper.GetParentPanel(this);
    }

    private Control? GetTargetElement(IProportionalSplitterHost panel)
    {
        return PanelNavigationHelper.GetSiblingInDirection(this, panel, -1);
    }
}
