using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;

namespace AvalonDock.Layout;

/// <summary>A split group containing document panes.</summary>
public class LayoutDocumentPaneGroup : LayoutPositionableGroup<ILayoutDocumentPane>, ILayoutDocumentPane, ILayoutOrientableGroup
{
    /// <summary>Creates an empty document pane group.</summary>
    public LayoutDocumentPaneGroup() { }
    /// <summary>Creates a group containing a document pane.</summary>
    public LayoutDocumentPaneGroup(LayoutDocumentPane documentPane) => Children.Add(documentPane);
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
