using System.Xml;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A group of tools in an auto-hide strip.</summary>
public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>
{
    /// <summary>Writes this group's metadata before its inherited attributes and children.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        LayoutXmlAttributes.AnchorGroup(this, writer);
        base.WriteXml(writer);
    }

    private ILayoutContainer? _previousContainer;

    /// <summary>Creates an empty auto-hide group.</summary>
    public LayoutAnchorGroup() : base(new ToolDock()) { }

    internal ILayoutContainer? PreviousContainer
    {
        get => _previousContainer;
        set
        {
            VerifyAccess();
            if (ReferenceEquals(_previousContainer, value)) return;
            _previousContainer = value;
            RaisePropertyChanged();
            LayoutContainerIdentity.EnsureReferencedId(_previousContainer);
        }
    }
    internal string? PreviousContainerId { get; set; }

    /// <inheritdoc/>
    protected override bool GetVisibility() => Children.Count > 0;
}
