using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Renders a public group of tool panes through Dock's proportional layout.</summary>
public class LayoutAnchorablePaneGroupControl : LayoutGridControl<ILayoutAnchorablePane>, ILayoutControl
{
    private readonly LayoutAnchorablePaneGroup _model;
    internal LayoutAnchorablePaneGroupControl(LayoutAnchorablePaneGroup model) : base(model) => _model = model;

    /// <summary>Converts non-star child lengths to one star along the group's split axis.</summary>
    protected override void OnFixChildrenDockLengths() => LayoutGroupLengths.MakeStars(_model, Orientation);
}
