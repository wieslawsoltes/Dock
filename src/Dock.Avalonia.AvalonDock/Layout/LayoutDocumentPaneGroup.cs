using System.Xml;
using Avalonia.Layout;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A split group containing document panes.</summary>
public class LayoutDocumentPaneGroup : LayoutPositionableGroup<ILayoutDocumentPane>, ILayoutDocumentPane, ILayoutOrientableGroup
{
    /// <summary>Writes this group's metadata before its inherited attributes and children.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        writer.WriteAttributeString(nameof(Orientation), Orientation.ToString());
        base.WriteXml(writer);
    }

    /// <summary>Creates an empty document pane group.</summary>
    public LayoutDocumentPaneGroup() { }
    /// <summary>Creates a group containing a document pane.</summary>
    public LayoutDocumentPaneGroup(LayoutDocumentPane documentPane) => Children.Add(documentPane);
    internal LayoutDocumentPaneGroup(ProportionalDock model, LayoutModelContext context) : base(model, context) { }
    /// <summary>Keeps the document region visible even when its individual panes are empty.</summary>
    protected override bool GetVisibility() => true;
    /// <inheritdoc/>
    public Orientation Orientation
    {
        get => (Orientation)((ProportionalDock)DockModel).Orientation;
        set => ((ProportionalDock)DockModel).Orientation = (Dock.Model.Core.Orientation)value;
    }
}
