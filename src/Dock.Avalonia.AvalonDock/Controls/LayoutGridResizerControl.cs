using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using AvalonDock.Infrastructure;
using Dock.Controls.ProportionalStackPanel;

namespace AvalonDock.Controls;

/// <summary>Resizes layout panes and auto-hide previews through Dock's native layout controls.</summary>
/// <remarks>The default background is transparent, including when a replacement theme
/// supplies no background. Styles and local values can override it.</remarks>
public class LayoutGridResizerControl : ProportionalStackPanelSplitter
{
    static LayoutGridResizerControl()
    {
        BackgroundProperty.OverrideDefaultValue<LayoutGridResizerControl>(Brushes.Transparent);
    }

    /// <summary>Defines the brush used for the drag preview.</summary>
    public static readonly StyledProperty<IBrush?> BackgroundWhileDraggingProperty =
        AvaloniaProperty.Register<LayoutGridResizerControl, IBrush?>(name: nameof(BackgroundWhileDragging), metadata: AvalonDockProperty.StyledMetadata<IBrush?>(defaultValue: Brushes.Black));

    /// <summary>Defines the drag preview opacity.</summary>
    public static readonly StyledProperty<double> OpacityWhileDraggingProperty =
        AvaloniaProperty.Register<LayoutGridResizerControl, double>(name: nameof(OpacityWhileDragging), metadata: AvalonDockProperty.StyledMetadata<double>(defaultValue: 0.5));

    /// <summary>Creates a splitter with a black, half-opacity preview.</summary>
    public LayoutGridResizerControl() => SynchronizePreview();

    /// <summary>Gets or sets the drag preview brush. Defaults to black.</summary>
    public IBrush? BackgroundWhileDragging { get => GetValue(BackgroundWhileDraggingProperty); set => SetValue(BackgroundWhileDraggingProperty, value); }

    /// <summary>Gets or sets the drag preview opacity. Defaults to 0.5.</summary>
    public double OpacityWhileDragging { get => GetValue(OpacityWhileDraggingProperty); set => SetValue(OpacityWhileDraggingProperty, value); }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(LayoutGridResizerControl);

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (LayoutAutoHideResize.IsAutoHideResizer(this)
            && (!IsResizingEnabled || e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)) return;
        base.OnPointerPressed(e);
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        LayoutAutoHideResize.Cancel(this);
        base.OnPointerCaptureLost(e);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BackgroundWhileDraggingProperty || change.Property == OpacityWhileDraggingProperty)
            SynchronizePreview();
    }

    private void SynchronizePreview()
    {
        SetCurrentValue(PreviewBackgroundProperty, BackgroundWhileDragging);
        SetCurrentValue(PreviewOpacityProperty, OpacityWhileDragging);
    }
}
