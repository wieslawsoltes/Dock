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
using Avalonia.VisualTree;

namespace Dock.Controls.ProportionalStackPanel;

/// <summary>
/// Represents a control that lets the user change the size of elements in a <see cref="ProportionalStackPanel"/>.
/// </summary>
[PseudoClasses(":horizontal", ":vertical", ":preview")]
public class ProportionalStackPanelSplitter : Thumb
{
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

        if (change.Property == IsResizingEnabledProperty)
        {
            UpdateVisualState();
        }

        if (change.Property == PreviewResizeProperty)
        {
            UpdatePreviewPseudoClass();
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
            var point = e.GetPosition(panel);
            _startPoint = point;
            _isMoving = true;
            UpdatePreviewPseudoClass();

            if (PreviewResize)
            {
                var pos = this.TranslatePoint(new Point(), panel);
                if (pos is not null)
                {
                    _adornerLayer = AdornerLayer.GetAdornerLayer(panel);
                    if (_adornerLayer is not null)
                    {
                        _startOffset = panel.Orientation == Orientation.Vertical ? pos.Value.Y : pos.Value.X;
                        _previewAdorner = new SplitterPreviewAdorner
                        {
                            Orientation = panel.Orientation,
                            Thickness = Thickness,
                            Offset = _startOffset,
                            [AdornerLayer.AdornedElementProperty] = panel
                        };
                        ((ISetLogicalParent)_previewAdorner).SetParent(panel);
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
            var point = e.GetPosition(panel);
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
                var point = e.GetPosition(panel);
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

        UpdateVisualState();
        UpdatePreviewPseudoClass();
    }

    private Control? FindNextChild(ProportionalStackPanel panel)
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

        var resizer = new ProportionResizer(panel, target, neighbor, dragDelta, GetValue(MinimumProportionSizeProperty));
        resizer.ApplyResize();
    }

    private void UpdateVisualState()
    {
        if (GetPanel() is { } panel)
        {
            PanelNavigationHelper.UpdateControlVisualState(this, panel, Thickness, IsResizingEnabled);
            
            // Add pseudo classes for styling
            if (panel.Orientation == Orientation.Vertical)
            {
                PseudoClasses.Add(":vertical");
            }
            else
            {
                PseudoClasses.Add(":horizontal");
            }
        }
    }

    private ProportionalStackPanel? GetPanel()
    {
        return PanelNavigationHelper.GetParentPanel(this);
    }

    private Control? GetTargetElement(ProportionalStackPanel panel)
    {
        return PanelNavigationHelper.GetSiblingInDirection(this, panel, -1);
    }
}
