using Dock.Model.Avalonia.Controls;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A group of tools in an auto-hide strip.</summary>
public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>
{
    /// <summary>Creates an empty auto-hide group.</summary>
    public LayoutAnchorGroup() : base(new ToolDock()) { }

    internal ILayoutContainer? PreviousContainer { get; set; }
    internal string? PreviousContainerId { get; set; }

    /// <inheritdoc/>
    protected override bool GetVisibility() => Children.Count > 0;
}
