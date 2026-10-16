using AvalonDock.Layout;

namespace AvalonDock.Controls;

/// <summary>Renders a public layout panel through Dock's native proportional layout.</summary>
public class LayoutPanelControl : LayoutGridControl<ILayoutPanelElement>, ILayoutControl
{
    internal LayoutPanelControl(LayoutPanel model) : base(model) { }

    /// <summary>Preserves the panel children's explicit pixel, auto, and star lengths.</summary>
    protected override void OnFixChildrenDockLengths() { }
}
