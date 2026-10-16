namespace AvalonDock.Controls;

/// <summary>Identifies an AvalonDock docking destination.</summary>
public enum DropTargetType
{
    /// <summary>Dock along the outer left edge.</summary>
    DockingManagerDockLeft,
    /// <summary>Dock along the outer top edge.</summary>
    DockingManagerDockTop,
    /// <summary>Dock along the outer right edge.</summary>
    DockingManagerDockRight,
    /// <summary>Dock along the outer bottom edge.</summary>
    DockingManagerDockBottom,
    /// <summary>Split a document pane on the left.</summary>
    DocumentPaneDockLeft,
    /// <summary>Split a document pane above.</summary>
    DocumentPaneDockTop,
    /// <summary>Split a document pane on the right.</summary>
    DocumentPaneDockRight,
    /// <summary>Split a document pane below.</summary>
    DocumentPaneDockBottom,
    /// <summary>Join a document pane's tabs.</summary>
    DocumentPaneDockInside,
    /// <summary>Dock inside a document pane group.</summary>
    DocumentPaneGroupDockInside,
    /// <summary>Split a tool pane on the left.</summary>
    AnchorablePaneDockLeft,
    /// <summary>Split a tool pane above.</summary>
    AnchorablePaneDockTop,
    /// <summary>Split a tool pane on the right.</summary>
    AnchorablePaneDockRight,
    /// <summary>Split a tool pane below.</summary>
    AnchorablePaneDockBottom,
    /// <summary>Join a tool pane's tabs.</summary>
    AnchorablePaneDockInside,
    /// <summary>Dock a tool pane to the left of a document pane.</summary>
    DocumentPaneDockAsAnchorableLeft,
    /// <summary>Dock a tool pane above a document pane.</summary>
    DocumentPaneDockAsAnchorableTop,
    /// <summary>Dock a tool pane to the right of a document pane.</summary>
    DocumentPaneDockAsAnchorableRight,
    /// <summary>Dock a tool pane below a document pane.</summary>
    DocumentPaneDockAsAnchorableBottom
}
