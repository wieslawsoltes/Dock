
namespace Dock.Model.Core;

/// <summary>
/// Defines the available dock operations.
/// </summary>
public enum DockOperation
{
    /// <summary>
    /// Fill dock.
    /// </summary>
    Fill,

    /// <summary>
    /// Dock to left.
    /// </summary>
    Left,

    /// <summary>
    /// Dock to bottom.
    /// </summary>
    Bottom,

    /// <summary>
    /// Dock to right.
    /// </summary>
    Right,

    /// <summary>
    /// Dock to top.
    /// </summary>
    Top,

    /// <summary>
    /// Dock to window.
    /// </summary>
    Window,

    /// <summary>
    /// Dock to top left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Dock to top right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Dock to bottom left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Dock to bottom right corner.
    /// </summary>
    BottomRight
}