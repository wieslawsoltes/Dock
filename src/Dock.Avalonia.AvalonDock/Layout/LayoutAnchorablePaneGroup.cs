using Avalonia.Controls;
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
    public LayoutAnchorablePaneGroup(LayoutAnchorablePane firstChild) => Children.Add(firstChild);
    internal LayoutAnchorablePaneGroup(ProportionalDock model, LayoutModelContext context) : base(model, context) { }
    /// <summary>Shows the split group while it contains a visible tool pane or group.</summary>
    protected override bool GetVisibility()
    {
        foreach (ILayoutAnchorablePane child in Children) if (child.IsVisible) return true;
        return false;
    }
    /// <summary>Propagates a changed pixel width to the group's only child.</summary>
    protected override void OnDockWidthChanged()
    {
        UpdateSingleChildLength(DockWidth, horizontal: true);
        base.OnDockWidthChanged();
    }
    /// <summary>Propagates a changed pixel height to the group's only child.</summary>
    protected override void OnDockHeightChanged()
    {
        UpdateSingleChildLength(DockHeight, horizontal: false);
        base.OnDockHeightChanged();
    }

    private void UpdateSingleChildLength(GridLength length, bool horizontal)
    {
        if (!length.IsAbsolute || Children.Count != 1 || Children[0] is not ILayoutPositionableGroup child) return;
        if (horizontal) child.DockWidth = length;
        else child.DockHeight = length;
    }
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
