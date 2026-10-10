using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A tabbed tool pane backed by a Dock tool dock.</summary>
public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutContentSelector
{
    /// <summary>Creates an empty anchorable pane.</summary>
    public LayoutAnchorablePane() : this(new ToolDock()) { }
    /// <summary>Creates a pane containing its first anchorable.</summary>
    public LayoutAnchorablePane(LayoutAnchorable anchorable) : this() => Children.Add(anchorable);
    internal LayoutAnchorablePane(ToolDock model, LayoutModelContext? context = null) : base(model, context) { }
    /// <summary>Gets or sets the pane's name.</summary>
    public string Name { get => DockModel.Name ?? string.Empty; set => DockModel.Name = value; }
    /// <summary>Gets whether every child can be hidden.</summary>
    public bool CanHide { get { foreach (LayoutAnchorable child in Children) if (!child.CanHide) return false; return Children.Count > 0; } }
    /// <summary>Gets whether every child can be closed.</summary>
    public bool CanClose { get { foreach (LayoutAnchorable child in Children) if (!child.CanClose) return false; return Children.Count > 0; } }
    /// <summary>Gets whether this pane is in a floating window.</summary>
    public bool IsHostedInFloatingWindow => (DockModel.DockingState & Dock.Model.Core.DockingWindowState.Floating) != 0;
    /// <summary>Gets whether this pane is the only pane in its floating window.</summary>
    public bool IsDirectlyHostedInFloatingWindow => IsHostedInFloatingWindow && Parent?.ChildrenCount == 1;
    /// <inheritdoc/>
    public int SelectedContentIndex
    {
        get => SelectedContent is LayoutAnchorable selected ? Children.IndexOf(selected) : -1;
        set => ((ToolDock)DockModel).ActiveDockable = value >= 0 && value < Children.Count ? Children[value].DockModel : null;
    }
    /// <inheritdoc/>
    public LayoutContent? SelectedContent => ((ToolDock)DockModel).ActiveDockable is { } model ? Context.Get(model) as LayoutContent : null;
    /// <inheritdoc/>
    public int IndexOf(LayoutContent content) => content is LayoutAnchorable anchorable ? Children.IndexOf(anchorable) : -1;
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockBase.ActiveDockableProperty)
        {
            RaisePropertyChanged(nameof(SelectedContent));
            RaisePropertyChanged(nameof(SelectedContentIndex));
        }
    }
}
