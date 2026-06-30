// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Dock.Controls.Flat;

/// <summary>
/// Splitter used by <see cref="FlatProportionalPanel"/> to resize flattened proportional dock regions.
/// </summary>
[PseudoClasses(":horizontal", ":vertical", ":preview")]
public class FlatProportionalSplitter : Thumb
{
    private Point _startPoint;
    private bool _isMoving;
    private FlatProportionalSplitterPreviewAdorner? _previewAdorner;
    private AdornerLayer? _adornerLayer;
    private double _startOffset;

    /// <summary>
    /// Defines the <see cref="Thickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<FlatProportionalSplitter, double>(nameof(Thickness), 4.0);

    /// <summary>
    /// Defines the <see cref="IsResizingEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizingEnabledProperty =
        AvaloniaProperty.Register<FlatProportionalSplitter, bool>(nameof(IsResizingEnabled), true);

    /// <summary>
    /// Defines the <see cref="PreviewResize"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> PreviewResizeProperty =
        AvaloniaProperty.Register<FlatProportionalSplitter, bool>(nameof(PreviewResize));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Avalonia.Layout.Orientation> OrientationProperty =
        AvaloniaProperty.Register<FlatProportionalSplitter, Avalonia.Layout.Orientation>(nameof(Orientation));

    /// <summary>
    /// Gets or sets the splitter thickness.
    /// </summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the splitter can resize neighboring dockables.
    /// </summary>
    public bool IsResizingEnabled
    {
        get => GetValue(IsResizingEnabledProperty);
        set => SetValue(IsResizingEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether resize changes are previewed until pointer release.
    /// </summary>
    public bool PreviewResize
    {
        get => GetValue(PreviewResizeProperty);
        set => SetValue(PreviewResizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the owning proportional dock.
    /// </summary>
    public Avalonia.Layout.Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets the model splitter represented by this control.
    /// </summary>
    public IFlatProportionalSplitter? Splitter { get; internal set; }

    /// <summary>
    /// Gets the proportional dock that owns <see cref="Splitter"/>.
    /// </summary>
    public IFlatProportionalDock? OwnerDock { get; internal set; }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty
            || change.Property == ThicknessProperty
            || change.Property == IsResizingEnabledProperty)
        {
            UpdateVisualState();
        }

        if (change.Property == PreviewResizeProperty)
        {
            UpdatePreviewPseudoClass();
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return Orientation == Avalonia.Layout.Orientation.Vertical
            ? new Size(0, Thickness)
            : new Size(Thickness, 0);
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        UpdateVisualState();
        UpdatePreviewPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!IsResizingEnabled || GetPanel() is not { } panel)
        {
            return;
        }

        _startPoint = e.GetPosition(panel);
        _isMoving = true;
        UpdatePreviewPseudoClass();

        if (!PreviewResize)
        {
            return;
        }

        var position = this.TranslatePoint(new Point(), panel);
        if (position is null)
        {
            return;
        }

        _adornerLayer = AdornerLayer.GetAdornerLayer(panel);
        if (_adornerLayer is null)
        {
            return;
        }

        _startOffset = Orientation == Avalonia.Layout.Orientation.Vertical ? position.Value.Y : position.Value.X;
        _previewAdorner = new FlatProportionalSplitterPreviewAdorner
        {
            Orientation = Orientation,
            Thickness = Thickness,
            Offset = _startOffset,
            [AdornerLayer.AdornedElementProperty] = panel
        };

        ((ISetLogicalParent)_previewAdorner).SetParent(panel);
        _adornerLayer.Children.Add(_previewAdorner);
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isMoving || !IsResizingEnabled || GetPanel() is not { } panel)
        {
            return;
        }

        var point = e.GetPosition(panel);
        var delta = point - _startPoint;
        var axisDelta = Orientation == Avalonia.Layout.Orientation.Vertical ? delta.Y : delta.X;

        if (PreviewResize)
        {
            if (_previewAdorner is not null)
            {
                _previewAdorner.Offset = _startOffset + axisDelta;
                _previewAdorner.InvalidateVisual();
            }
            return;
        }

        _startPoint = point;
        panel.ResizeSplitter(this, axisDelta);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isMoving && IsResizingEnabled && GetPanel() is { } panel && PreviewResize)
        {
            var point = e.GetPosition(panel);
            var delta = point - _startPoint;
            panel.ResizeSplitter(this, Orientation == Avalonia.Layout.Orientation.Vertical ? delta.Y : delta.X);
        }

        RemovePreviewAdorner();
        _isMoving = false;
        UpdatePreviewPseudoClass();
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);

        RemovePreviewAdorner();
        _isMoving = false;
        UpdatePreviewPseudoClass();
    }

    private FlatProportionalPanel? GetPanel()
    {
        return this.FindAncestorOfType<FlatProportionalPanel>();
    }

    private void UpdateVisualState()
    {
        if (Orientation == Avalonia.Layout.Orientation.Vertical)
        {
            Height = Thickness;
            Width = double.NaN;
            Cursor = IsResizingEnabled ? new Cursor(StandardCursorType.SizeNorthSouth) : new Cursor(StandardCursorType.Arrow);
            PseudoClasses.Set(":vertical", true);
            PseudoClasses.Set(":horizontal", false);
            return;
        }

        Width = Thickness;
        Height = double.NaN;
        Cursor = IsResizingEnabled ? new Cursor(StandardCursorType.SizeWestEast) : new Cursor(StandardCursorType.Arrow);
        PseudoClasses.Set(":horizontal", true);
        PseudoClasses.Set(":vertical", false);
    }

    private void UpdatePreviewPseudoClass()
    {
        PseudoClasses.Set(":preview", PreviewResize && _isMoving);
    }

    private void RemovePreviewAdorner()
    {
        if (_previewAdorner is null || _adornerLayer is null)
        {
            _previewAdorner = null;
            _adornerLayer = null;
            return;
        }

        _adornerLayer.Children.Remove(_previewAdorner);
        ((ISetLogicalParent)_previewAdorner).SetParent(null);
        _previewAdorner = null;
        _adornerLayer = null;
    }
}
