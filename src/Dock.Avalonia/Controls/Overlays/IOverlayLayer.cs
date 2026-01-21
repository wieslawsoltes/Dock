using Avalonia.Controls;

namespace Dock.Avalonia.Controls.Overlays;

/// <summary>
/// Describes an overlay layer that can be composed by <see cref="OverlayHost"/>.
/// </summary>
public interface IOverlayLayer
{
    /// <summary>
    /// Gets the layer z-order.
    /// </summary>
    int ZIndex { get; }

    /// <summary>
    /// Gets a value indicating whether the layer is visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets a value indicating whether the layer blocks input.
    /// </summary>
    bool BlocksInput { get; }

    /// <summary>
    /// Gets the style key used to resolve a layer theme.
    /// </summary>
    object? StyleKey { get; }

    /// <summary>
    /// Gets the overlay control instance.
    /// </summary>
    Control? Overlay { get; }
}
