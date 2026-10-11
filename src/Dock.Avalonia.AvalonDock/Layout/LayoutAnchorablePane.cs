using System;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A tabbed tool pane backed by a Dock tool dock.</summary>
public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutContentSelector, ILayoutPaneSelection
{
    private readonly LayoutPaneSelection<LayoutAnchorable>? _selection;
    /// <summary>Creates an empty anchorable pane.</summary>
    public LayoutAnchorablePane() : this(new ToolDock()) { }
    /// <summary>Creates a pane containing its first anchorable.</summary>
    public LayoutAnchorablePane(LayoutAnchorable anchorable) : this() => Children.Add(anchorable);
    // AvalonDock retains empty panes while floating/hidden contents refer to
    // them. Its layout collector decides when the native pane can be removed.
    internal LayoutAnchorablePane(ToolDock model, LayoutModelContext? context = null) : base(model, context)
    {
        model.IsCollapsable = false;
        _selection = new(this, false, RaisePropertyChanging, RaisePropertyChanged);
        _selection.Initialize();
    }
    /// <summary>Gets or sets the pane's name.</summary>
    public string Name { get => DockModel.Name ?? string.Empty; set => DockModel.Name = value; }
    /// <summary>Gets whether every child can be hidden.</summary>
    public bool CanHide { get { foreach (LayoutAnchorable child in Children) if (!child.CanHide) return false; return true; } }
    /// <summary>Gets whether every child can be closed.</summary>
    public bool CanClose { get { foreach (LayoutAnchorable child in Children) if (!child.CanClose) return false; return true; } }
    /// <summary>Gets whether this pane is in a floating window.</summary>
    public bool IsHostedInFloatingWindow => this.FindParent<LayoutFloatingWindow>() is not null;
    /// <summary>Gets whether this pane is the only pane in its floating window.</summary>
    public bool IsDirectlyHostedInFloatingWindow => this.FindParent<LayoutAnchorableFloatingWindow>()?.IsSinglePane == true;
    /// <inheritdoc/>
    public int SelectedContentIndex
    {
        get => _selection?.Index ?? -1;
        set => _selection?.Select(value);
    }
    /// <inheritdoc/>
    public LayoutContent? SelectedContent => _selection?.Selected;
    void ILayoutPaneSelection.ReconcileSelection() => _selection?.ChildrenChanged();
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

    /// <summary>Repairs pane selection and updates aggregate capabilities after a collection change.</summary>
    protected override void OnChildrenCollectionChanged()
    {
        _selection?.ChildrenChanged();
        RaisePropertyChanged(nameof(CanClose));
        RaisePropertyChanged(nameof(CanHide));
        RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
        base.OnChildrenCollectionChanged();
    }

    /// <summary>Refreshes the selected index after a child is reordered.</summary>
    protected override void ChildMoved(int oldIndex, int newIndex)
    {
        _selection?.Refresh();
        base.ChildMoved(oldIndex, newIndex);
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockBase.ActiveDockableProperty)
        {
            _selection?.Refresh();
        }
    }
}
