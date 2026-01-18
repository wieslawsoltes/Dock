using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Wraps an overlay control with layer metadata.
/// </summary>
public sealed class OverlayLayer : AvaloniaObject, IOverlayLayer
{
    /// <summary>
    /// Defines the <see cref="ZIndex"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<int> ZIndexProperty =
        AvaloniaProperty.Register<OverlayLayer, int>(nameof(ZIndex));

    /// <summary>
    /// Defines the <see cref="IsVisible"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<OverlayLayer, bool>(nameof(IsVisible), true);

    /// <summary>
    /// Defines the <see cref="BlocksInput"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<bool> BlocksInputProperty =
        AvaloniaProperty.Register<OverlayLayer, bool>(nameof(BlocksInput), true);

    /// <summary>
    /// Defines the <see cref="StyleKey"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<object?> StyleKeyProperty =
        AvaloniaProperty.Register<OverlayLayer, object?>(nameof(StyleKey));

    /// <summary>
    /// Defines the <see cref="Overlay"/> Avalonia property.
    /// </summary>
    public static readonly StyledProperty<Control?> OverlayProperty =
        AvaloniaProperty.Register<OverlayLayer, Control?>(nameof(Overlay));

    /// <inheritdoc />
    public int ZIndex
    {
        get => GetValue(ZIndexProperty);
        set => SetValue(ZIndexProperty, value);
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <inheritdoc />
    public bool BlocksInput
    {
        get => GetValue(BlocksInputProperty);
        set => SetValue(BlocksInputProperty, value);
    }

    /// <inheritdoc />
    public object? StyleKey
    {
        get => GetValue(StyleKeyProperty);
        set => SetValue(StyleKeyProperty, value);
    }

    /// <inheritdoc />
    [Content]
    public Control? Overlay
    {
        get => GetValue(OverlayProperty);
        set => SetValue(OverlayProperty, value);
    }
}
