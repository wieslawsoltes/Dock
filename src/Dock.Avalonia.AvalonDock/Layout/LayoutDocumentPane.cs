using System.Collections.Generic;
using Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>A tabbed document pane backed by a Dock document dock.</summary>
public class LayoutDocumentPane : LayoutPositionableGroup<LayoutContent>, ILayoutDocumentPane, ILayoutContentSelector
{
    private bool _showHeader = true;
    /// <summary>Creates an empty document pane.</summary>
    public LayoutDocumentPane() : this(new DocumentDock { IsCollapsable = false }) { }
    /// <summary>Creates a pane containing its first document.</summary>
    public LayoutDocumentPane(LayoutContent firstChild) : this() => Children.Add(firstChild);
    internal LayoutDocumentPane(DocumentDock model, LayoutModelContext? context = null) : base(model, context) => model.IsCollapsable = false;
    /// <summary>Gets or sets whether document headers are shown.</summary>
    public bool ShowHeader { get => _showHeader; set => SetField(ref _showHeader, value); }
    /// <inheritdoc/>
    public int SelectedContentIndex
    {
        get => SelectedContent is { } selected ? Children.IndexOf(selected) : -1;
        set => ((DocumentDock)DockModel).ActiveDockable = value >= 0 && value < Children.Count ? Children[value].DockModel : null;
    }
    /// <inheritdoc/>
    public LayoutContent? SelectedContent => ((DocumentDock)DockModel).ActiveDockable is { } model ? Context.Get(model) as LayoutContent : null;
    /// <summary>Gets the documents sorted by title.</summary>
    public IEnumerable<LayoutContent> ChildrenSorted
    {
        get { List<LayoutContent> sorted = new(Children); sorted.Sort(); return sorted; }
    }
    /// <inheritdoc/>
    public int IndexOf(LayoutContent content) => Children.IndexOf(content);
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
