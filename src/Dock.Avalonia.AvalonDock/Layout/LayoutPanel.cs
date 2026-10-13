using System.Xml;
using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A split panel containing document and anchorable panes.</summary>
public class LayoutPanel : LayoutPositionableGroup<ILayoutPanelElement>, ILayoutOrientableGroup
{
    /// <summary>Restores this group's metadata before its inherited dimensions and children.</summary>
    public override void ReadXml(XmlReader reader)
    {
        LayoutGroupXml.ReadOrientable(this, this, reader);
        base.ReadXml(reader);
    }

    /// <summary>Writes this group's metadata before its inherited attributes and children.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        writer.WriteAttributeString(nameof(Orientation), Orientation.ToString());
        base.WriteXml(writer);
    }

    /// <summary>Creates an empty panel.</summary>
    public LayoutPanel() : this(new ProportionalDock()) { }
    /// <summary>Creates a panel with its first child.</summary>
    public LayoutPanel(ILayoutPanelElement firstChild) : this() => Children.Add(firstChild);
    internal LayoutPanel(ProportionalDock model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Shows the panel while at least one child is visible.</summary>
    protected override bool GetVisibility()
    {
        foreach (ILayoutPanelElement child in Children) if (child.IsVisible) return true;
        return false;
    }
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
