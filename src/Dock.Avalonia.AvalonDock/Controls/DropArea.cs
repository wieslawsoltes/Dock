using Avalonia;
using Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>Identifies the layout surface underneath a docking overlay.</summary>
public enum DropAreaType
{
    /// <summary>The outer docking surface.</summary>
    DockingManager,
    /// <summary>A document pane.</summary>
    DocumentPane,
    /// <summary>A document pane group.</summary>
    DocumentPaneGroup,
    /// <summary>A tool pane.</summary>
    AnchorablePane
}

/// <summary>Describes a native docking surface in screen coordinates.</summary>
public interface IDropArea
{
    /// <summary>Gets the captured detection rectangle in physical screen pixels.</summary>
    Rect DetectionRect { get; }
    /// <summary>Gets the kind of docking surface.</summary>
    DropAreaType Type { get; }
}

/// <summary>Captures the screen rectangle and kind of an existing docking control.</summary>
/// <typeparam name="T">The type of the existing Avalonia control.</typeparam>
public class DropArea<T> : IDropArea where T : Control
{
    internal DropArea(T areaElement, DropAreaType type)
    {
        AreaElement = areaElement;
        Type = type;
        DetectionRect = LayoutOverlayGeometry.GetScreenBounds(areaElement);
    }

    /// <summary>Gets the screen rectangle captured when this area was created, in physical pixels.</summary>
    public Rect DetectionRect { get; }
    /// <summary>Gets the kind of docking surface.</summary>
    public DropAreaType Type { get; }
    /// <summary>Gets the existing control; no second docking layout is created.</summary>
    public T AreaElement { get; }
}
