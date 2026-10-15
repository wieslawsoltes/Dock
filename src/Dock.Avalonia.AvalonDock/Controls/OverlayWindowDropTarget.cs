using Avalonia;
using Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>Captures the identity and screen rectangle of a native Dock selector.</summary>
public class OverlayWindowDropTarget
{
    internal OverlayWindowDropTarget(OverlayWindowDropTargetType type, DropTargetType targetType, Control selector)
    {
        Type = type;
        TargetType = targetType;
        ScreenDetectionArea = LayoutOverlayGeometry.GetScreenBounds(selector);
    }

    /// <summary>Gets the selector kind, exposed for compiled Avalonia templates.</summary>
    public OverlayWindowDropTargetType Type { get; }
    /// <summary>Gets the corresponding AvalonDock docking destination.</summary>
    public DropTargetType TargetType { get; }
    /// <summary>Gets the captured selector rectangle in physical screen pixels.</summary>
    public Rect ScreenDetectionArea { get; }
}
