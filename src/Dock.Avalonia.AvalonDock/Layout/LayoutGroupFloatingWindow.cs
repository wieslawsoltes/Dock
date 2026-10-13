using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using Avalonia;
using Avalonia.Metadata;
using AvalonDock.Infrastructure;
using Dock.Model.Avalonia.Core;

namespace AvalonDock.Layout;

/// <summary>Preserves a native Dock floating layout containing document groups, mixed panes, or generic panels.</summary>
/// <remarks>This Dock extension uses the LayoutGroupFloatingWindow XML element. Original AvalonDock
/// layouts continue to use their document and anchorable floating-window types.</remarks>
public class LayoutGroupFloatingWindow : LayoutFloatingWindow, ILayoutGroup, ILayoutElementWithVisibility
{
    private readonly LayoutChildren<ILayoutPanelElement> _panels;
    private readonly List<LayoutGroupBase> _observed = new();
    private bool _isVisible;

    /// <summary>Creates an empty floating group.</summary>
    public LayoutGroupFloatingWindow()
    {
        _panels = new(this, NativeRoot);
        _panels.CollectionChanged += (_, _) => PanelsChanged();
    }

    internal LayoutGroupFloatingWindow(DockWindow window, LayoutModelContext context) : base(window, context)
    {
        _panels = new(this, NativeRoot);
        _panels.CollectionChanged += (_, _) => PanelsChanged();
        PanelsChanged();
    }

    /// <summary>Gets the panes and panels hosted by the native floating root.</summary>
    [Content]
    public ObservableCollection<ILayoutPanelElement> RootPanels => _panels;
    /// <inheritdoc/>
    public override IEnumerable<ILayoutElement> Children => (IEnumerable<ILayoutElement>?)_panels ?? Array.Empty<ILayoutElement>();
    /// <inheritdoc/>
    public override int ChildrenCount => _panels?.Count ?? 0;
    /// <inheritdoc/>
    public override bool IsValid => ChildrenCount > 0;
    /// <summary>Gets whether any document or tool is currently hosted in this group.</summary>
    /// <remarks>Empty return panes can keep a valid floating model after its host is hidden.</remarks>
    public bool IsVisible => _isVisible;
    /// <inheritdoc/>
    public void ComputeVisibility()
    {
        bool visible = false;
        foreach (ILayoutElement child in this.Descendents())
            if (child is LayoutContent) { visible = true; break; }
        if (_isVisible == visible) return;
        RaisePropertyChanging(nameof(IsVisible));
        _isVisible = visible;
        RaisePropertyChanged(nameof(IsVisible));
    }
    /// <inheritdoc/>
    public event EventHandler? ChildrenCollectionChanged;
    /// <inheritdoc/>
    public int IndexOfChild(ILayoutElement element) => element is ILayoutPanelElement panel ? _panels.IndexOf(panel) : -1;
    /// <inheritdoc/>
    public void InsertChildAt(int index, ILayoutElement element) => _panels.Insert(index, (ILayoutPanelElement)element);
    /// <inheritdoc/>
    public void RemoveChildAt(int index) => _panels.RemoveAt(index);
    /// <inheritdoc/>
    public void ReplaceChildAt(int index, ILayoutElement element) => _panels[index] = (ILayoutPanelElement)element;
    /// <inheritdoc/>
    public override void RemoveChild(ILayoutElement element)
    {
        if (element is ILayoutPanelElement panel) _panels.Remove(panel);
    }
    /// <inheritdoc/>
    public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        int index = IndexOfChild(oldElement);
        if (index >= 0) ReplaceChildAt(index, newElement);
    }
    /// <inheritdoc/>
    public override void ReadXml(XmlReader reader) => LayoutXmlDeserializer.ReadContainer(this, reader);

    private void PanelsChanged()
    {
        foreach (LayoutGroupBase group in _observed) group.ChildrenTreeChanged -= TreeChanged;
        _observed.Clear();
        foreach (ILayoutPanelElement panel in _panels)
            if (panel is LayoutGroupBase group)
            {
                _observed.Add(group);
                group.ChildrenTreeChanged += TreeChanged;
            }
        RefreshBounds();
        ComputeVisibility();
        RaisePropertyChanged(nameof(ChildrenCount));
        RaisePropertyChanged(nameof(IsValid));
        ChildrenCollectionChanged?.Invoke(this, EventArgs.Empty);
        (Root as LayoutRoot)?.NotifyUpdated();
    }

    private void TreeChanged(object? sender, ChildrenTreeChangedEventArgs args)
    {
        RefreshBounds();
        ComputeVisibility();
        (Root as LayoutRoot)?.NotifyUpdated();
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        base.OnModelPropertyChanged(sender, args);
        if (args.Property == DockBase.VisibleDockablesProperty) _panels?.Reconnect();
    }
}
