using System.Xml;
using System;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A tabbed tool pane backed by a Dock tool dock.</summary>
public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutContentSelector, ILayoutPaneSelection
{
    /// <summary>Restores this group's metadata before its inherited dimensions and children.</summary>
    /// <remarks>Automatic selection waits for success; explicit selection stays immediate. A failed read leaves fallback disabled until a successful read.</remarks>
    public override void ReadXml(XmlReader reader)
    {
        LayoutGroupXml.ReadAnchorablePane(this, reader);
        base.ReadXml(reader);
        CompleteXmlRead();
    }

    /// <summary>Writes this group's metadata before its inherited attributes and children.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        LayoutXmlAttributes.AnchorablePane(this, writer);
        base.WriteXml(writer);
    }

    private readonly LayoutPaneSelection<LayoutAnchorable>? _selection;
    private string? _name;
    /// <summary>Creates an empty anchorable pane.</summary>
    public LayoutAnchorablePane() : this(new ToolDock()) { }
    /// <summary>Creates a pane containing its first anchorable.</summary>
    public LayoutAnchorablePane(LayoutAnchorable anchorable) : this() => Children.Add(anchorable);
    // AvalonDock retains empty panes while floating/hidden contents refer to
    // them. Its layout collector decides when the native pane can be removed.
    internal LayoutAnchorablePane(ToolDock model, LayoutModelContext? context = null) : base(model, context)
    {
        _name = model.Name;
        model.IsCollapsable = false;
        _selection = new(this, false, RaisePropertyChanging, RaisePropertyChanged);
        _selection.Initialize();
    }
    /// <summary>Gets or sets the pane's name; initially null.</summary>
    /// <remarks>This layout metadata remains editable after native styling and is separate from Avalonia's styling name.</remarks>
    public string Name { get => _name!; set => SetFieldChanged(ref _name, value); }
    internal void RestoreName(string value) { VerifyAccess(); _name = value; }
    internal void BeginXmlRead() => _selection?.BeginXmlRead();
    internal void CompleteXmlRead() => _selection?.CompleteXmlRead();
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
    bool ILayoutPaneSelection.IsRefreshingChildren => IsRefreshingNativeChildren;
    bool ILayoutPaneSelection.IsMovingChildren => IsMovingNativeChildren;
    /// <inheritdoc/>
    public int IndexOf(LayoutContent content) => content is LayoutAnchorable anchorable ? Children.IndexOf(anchorable) : -1;
    /// <summary>Shows the pane while it contains a visible tool.</summary>
    protected override bool GetVisibility()
    {
        foreach (LayoutAnchorable content in Children) if (content.IsVisible) return true;
        return false;
    }

    /// <summary>Observes the new parent group's membership when reporting floating pane state.</summary>
    /// <remarks>Notifies IsDirectlyHostedInFloatingWindow before the inherited hook;
    /// IsHostedInFloatingWindow is read from the current ancestry without an additional property event.</remarks>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        if (oldValue is ILayoutGroup previous) previous.ChildrenCollectionChanged -= ParentChildrenChanged;
        RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));
        if (newValue is ILayoutGroup next) next.ChildrenCollectionChanged += ParentChildrenChanged;
        base.OnParentChanged(oldValue, newValue);
    }

    private void ParentChildrenChanged(object? sender, EventArgs args) =>
        RaisePropertyChanged(nameof(IsDirectlyHostedInFloatingWindow));

    internal void NotifyDirectlyHostedInFloatingWindow() =>
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

    /// <summary>Moves the stored selected index when it equals the child's old index.</summary>
    /// <remarks>The collection notification may already have updated the index from child selection flags.</remarks>
    protected override void ChildMoved(int oldIndex, int newIndex)
    {
        _selection?.ChildMoved(oldIndex, newIndex);
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
