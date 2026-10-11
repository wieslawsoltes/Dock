using System;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A tabbed tool pane backed by a Dock tool dock.</summary>
public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutContentSelector
{
    /// <summary>Creates an empty anchorable pane.</summary>
    public LayoutAnchorablePane() : this(new ToolDock()) { }
    /// <summary>Creates a pane containing its first anchorable.</summary>
    public LayoutAnchorablePane(LayoutAnchorable anchorable) : this() => Children.Add(anchorable);
    // AvalonDock retains empty panes while floating/hidden contents refer to
    // them. Its layout collector decides when the native pane can be removed.
    internal LayoutAnchorablePane(ToolDock model, LayoutModelContext? context = null) : base(model, context) => model.IsCollapsable = false;
    /// <summary>Gets or sets the pane's name.</summary>
    public string Name { get => DockModel.Name ?? string.Empty; set => DockModel.Name = value; }
    /// <summary>Gets whether every child can be hidden.</summary>
    public bool CanHide { get { foreach (LayoutAnchorable child in Children) if (!child.CanHide) return false; return Children.Count > 0; } }
    /// <summary>Gets whether every child can be closed.</summary>
    public bool CanClose { get { foreach (LayoutAnchorable child in Children) if (!child.CanClose) return false; return Children.Count > 0; } }
    /// <summary>Gets whether this pane is in a floating window.</summary>
    public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() is not null;
    /// <summary>Gets whether this pane is the only pane in its floating window.</summary>
    public bool IsDirectlyHostedInFloatingWindow => this.FindParent<LayoutAnchorableFloatingWindow>()?.IsSinglePane == true;
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
    /// <summary>Shows the pane while it contains a visible tool.</summary>
    protected override bool GetVisibility()
    {
        foreach (LayoutAnchorable content in Children) if (content.IsVisible) return true;
        return false;
    }

    /// <summary>Observes the new parent group's membership when reporting floating pane state.</summary>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (oldValue is ILayoutGroup previous) previous.ChildrenCollectionChanged -= ParentChildrenChanged;
        RaisePropertyChanged(nameof(IsHostedInFloatingWindow));
        RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
        if (newValue is ILayoutGroup next) next.ChildrenCollectionChanged += ParentChildrenChanged;
        base.OnParentChanged(oldValue, newValue);
    }

    private void ParentChildrenChanged(object? sender, EventArgs args) =>
        RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockBase.ActiveDockableProperty)
        {
            LayoutSelection.SynchronizeWithoutFactory(this, ((ToolDock)DockModel).ActiveDockable);
            RaisePropertyChanged(nameof(SelectedContent));
            RaisePropertyChanged(nameof(SelectedContentIndex));
        }
    }
}
