using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A split group containing anchorable panes.</summary>
public class LayoutAnchorablePaneGroup : LayoutPositionableGroup<ILayoutAnchorablePane>, ILayoutAnchorablePane, ILayoutOrientableGroup
{
    /// <summary>Creates an empty anchorable pane group.</summary>
    public LayoutAnchorablePaneGroup() { }
    /// <summary>Creates a group containing an anchorable pane.</summary>
    public LayoutAnchorablePaneGroup(LayoutAnchorablePane anchorablePane) => Children.Add(anchorablePane);
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
