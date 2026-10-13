using System.Xml;
using System.Collections.Generic;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A tabbed document pane backed by a Dock document dock.</summary>
public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutContentSelector, ILayoutPaneSelection
{
    /// <summary>Restores this group's metadata before its inherited dimensions and children.</summary>
    public override void ReadXml(XmlReader reader)
    {
        LayoutGroupXml.ReadDocumentPane(this, reader);
        base.ReadXml(reader);
    }

    /// <summary>Writes this group's metadata before its inherited attributes and children.</summary>
    public override void WriteXml(XmlWriter writer)
    {
        writer = AvalonDockXml.PrepareWriter(this, writer);
        LayoutXmlAttributes.DocumentPane(this, writer);
        base.WriteXml(writer);
    }

    private bool _showHeader = true;
    private readonly LayoutPaneSelection<LayoutContent>? _selection;
    /// <summary>Creates an empty document pane.</summary>
    public LayoutDocumentPane() : this(new DocumentDock { IsCollapsable = false }) { }
    /// <summary>Creates a pane containing its first document.</summary>
    public LayoutDocumentPane(LayoutContent firstChild) : this() => Children.Add(firstChild);
    internal LayoutDocumentPane(DocumentDock model, LayoutModelContext? context = null) : base(model, context)
    {
        model.IsCollapsable = false;
        _selection = new(this, true, RaisePropertyChanging, RaisePropertyChanged);
        _selection.Initialize();
    }
    /// <summary>Gets or sets whether the pane shows its document tabs and document list menu. The selected content remains visible.</summary>
    public bool ShowHeader { get => _showHeader; set => SetFieldChanged(ref _showHeader, value); }
    internal void RestoreShowHeader(bool value) { VerifyAccess(); _showHeader = value; }
    /// <inheritdoc/>
    public int SelectedContentIndex
    {
        get => _selection?.Index ?? -1;
        set => _selection?.Select(value);
    }
    /// <inheritdoc/>
    public LayoutContent? SelectedContent => _selection?.Selected;
    void ILayoutPaneSelection.ReconcileSelection() => _selection?.ChildrenChanged();
    /// <summary>Gets the documents sorted by comparable application content, falling back to titles.</summary>
    public IEnumerable<LayoutContent> ChildrenSorted
    {
        get { List<LayoutContent> sorted = new(Children); sorted.Sort(); return sorted; }
    }
    /// <inheritdoc/>
    public int IndexOf(LayoutContent content) => Children.IndexOf(content);
    /// <summary>Keeps the main document surface visible, hiding empty panes inside document groups.</summary>
    protected override bool GetVisibility()
    {
        if (Parent is not LayoutDocumentPaneGroup) return true;
        foreach (LayoutContent content in Children)
            if (content is LayoutAnchorable || content is LayoutDocument { IsVisible: true }) return true;
        return false;
    }
    /// <summary>Notifies document list menus that the sorted contents changed.</summary>
    protected override void OnChildrenCollectionChanged()
    {
        _selection?.ChildrenChanged();
        base.OnChildrenCollectionChanged();
        RaisePropertyChanged(nameof(ChildrenSorted));
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
