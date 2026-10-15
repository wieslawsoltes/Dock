using Avalonia;
using Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Controls;

/// <summary>Describes the screen rectangle occupied by an existing overlay area.</summary>
public abstract class OverlayArea
{
    internal OverlayArea(Control control) => SetScreenDetectionArea(LayoutOverlayGeometry.GetScreenBounds(control));

    /// <summary>Gets the captured detection rectangle in physical screen pixels.</summary>
    /// <remarks>This Avalonia projection is public so compiled templates can inspect the native overlay area.</remarks>
    public Rect ScreenDetectionArea { get; private set; }

    /// <summary>Replaces the screen rectangle used by the overlay projection.</summary>
    protected void SetScreenDetectionArea(Rect rect) => ScreenDetectionArea = rect;
}

/// <summary>Describes the outer native docking surface.</summary>
public class DockingManagerOverlayArea : OverlayArea
{
    internal DockingManagerOverlayArea(Control control) : base(control) { }
}

/// <summary>Describes a document pane's native docking surface.</summary>
public class DocumentPaneControlOverlayArea : OverlayArea
{
    internal DocumentPaneControlOverlayArea(Control control) : base(control) { }
}

/// <summary>Describes a tool pane's native docking surface.</summary>
public class AnchorablePaneControlOverlayArea : OverlayArea
{
    internal AnchorablePaneControlOverlayArea(Control control) : base(control) { }
}
