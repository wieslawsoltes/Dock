using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A split panel containing document and anchorable panes.</summary>
public class LayoutPanel : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup
{
    /// <summary>Creates an empty panel.</summary>
    public LayoutPanel() : this(new ProportionalDock()) { }
    /// <summary>Creates a panel with its first child.</summary>
    public LayoutPanel(ILayoutPanelElement firstChild) : this() => Children.Add(firstChild);
    internal LayoutPanel(ProportionalDock model, LayoutModelContext? context = null) : base(model, context) { }
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
