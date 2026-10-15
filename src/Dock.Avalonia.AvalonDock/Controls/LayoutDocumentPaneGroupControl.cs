using AvalonDock.Infrastructure;
using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Renders a public group of document panes through Dock's proportional layout.</summary>
public class LayoutDocumentPaneGroupControl : LayoutGridControl<ILayoutDocumentPane>, ILayoutControl
{
    private readonly LayoutDocumentPaneGroup _model;
    internal LayoutDocumentPaneGroupControl(LayoutDocumentPaneGroup model) : base(model) => _model = model;

    /// <summary>Converts non-star child lengths to one star along the group's split axis.</summary>
    protected override void OnFixChildrenDockLengths() => LayoutGroupLengths.MakeStars(_model, Orientation);
}
