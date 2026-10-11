using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A split group containing anchorable panes.</summary>
public class LayoutAnchorablePaneGroup : LayoutPositionableGroup<ILayoutAnchorablePane>, ILayoutAnchorablePane, ILayoutOrientableGroup
{
    /// <summary>Creates an empty anchorable pane group.</summary>
    public LayoutAnchorablePaneGroup() { }
    /// <summary>Creates a group containing an anchorable pane.</summary>
    public LayoutAnchorablePaneGroup(LayoutAnchorablePane anchorablePane) => Children.Add(anchorablePane);
    internal LayoutAnchorablePaneGroup(ProportionalDock model, LayoutModelContext context) : base(model, context) { }
    /// <summary>Shows the split group while it contains a visible tool pane or group.</summary>
    protected override bool GetVisibility()
    {
        foreach (ILayoutAnchorablePane child in Children) if (child.IsVisible) return true;
        return false;
    }
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
