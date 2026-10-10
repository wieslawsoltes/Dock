using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A floating group of tool panes backed by a native Dock window.</summary>
public class LayoutAnchorableFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
{
    private LayoutAnchorablePaneGroup? _panel;
    private INotifyCollectionChanged? _changes;
    private bool _changing;
    private bool _visible;

    /// <summary>Creates an empty tool floating window.</summary>
    public LayoutAnchorableFloatingWindow() => Reconnect();
    internal LayoutAnchorableFloatingWindow(DockWindow window, LayoutModelContext context) : base(window, context)
    {
        if (NativeRoot.VisibleDockables is { Count: 1 } children && children[0] is ToolDock toolPane)
        {
            ProportionalDock group = new() { VisibleDockables = new AvaloniaList<IDockable>(toolPane), Owner = NativeRoot };
            toolPane.Owner = group;
            children[0] = group;
            NativeRoot.ActiveDockable = group;
            NativeRoot.DefaultDockable = group;
        }
        Reconnect();
    }
    /// <summary>Gets or sets the root split group.</summary>
    [Content]
    public LayoutAnchorablePaneGroup? RootPanel
    {
        get => _panel;
        set
        {
            if (ReferenceEquals(_panel, value)) return;
            _changing = true;
            try
            {
                if (_panel is { } previous) previous.DockModel.Owner = null;
                NativeRoot.VisibleDockables!.Clear();
                if (value is not null)
                {
                    value.Parent?.RemoveChild(value);
                    Context.Adopt(value.Context);
                    NativeRoot.VisibleDockables.Add(value.DockModel);
                    if (DockModel.Factory is { } factory) factory.InitDockable(value.DockModel, NativeRoot);
                    else value.DockModel.Owner = NativeRoot;
                }
                NativeRoot.ActiveDockable = value?.DockModel;
                NativeRoot.DefaultDockable = value?.DockModel;
            }
            finally { _changing = false; }
            SynchronizePanel();
        }
    }
    /// <summary>Gets whether exactly one visible pane is hosted.</summary>
    public bool IsSinglePane => SinglePane is not null;
    /// <summary>Gets the only visible pane, when there is one.</summary>
    public ILayoutAnchorablePane? SinglePane
    {
        get
        {
            LayoutAnchorablePane? result = null;
            if (_panel is null) return null;
            foreach (ILayoutElement element in _panel.Descendents())
            {
                if (element is not LayoutAnchorablePane { IsVisible: true } pane) continue;
                if (result is not null) return null;
                result = pane;
            }
            return result;
        }
    }
    /// <summary>Gets whether the floating layout has visible content.</summary>
    public bool IsVisible => _visible;
    /// <summary>Occurs when the layout visibility changes.</summary>
    public event EventHandler? IsVisibleChanged;
    /// <inheritdoc/>
    public override IEnumerable<ILayoutElement> Children { get { if (_panel is { } panel) yield return panel; } }
    /// <inheritdoc/>
    public override int ChildrenCount => _panel is null ? 0 : 1;
    /// <inheritdoc/>
    public override bool IsValid => _panel is not null;
    /// <inheritdoc/>
    public override void RemoveChild(ILayoutElement element) { if (ReferenceEquals(element, _panel)) RootPanel = null; }
    /// <inheritdoc/>
    public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        if (ReferenceEquals(oldElement, _panel)) RootPanel = (LayoutAnchorablePaneGroup)newElement;
    }
    void ILayoutElementWithVisibility.ComputeVisibility() => NotifyPaneState();

    /// <inheritdoc/>
    public override void ReadXml(XmlReader reader)
    {
        LayoutAnchorableFloatingWindow restored = (LayoutAnchorableFloatingWindow)AvalonDockXml.ReadFloatingWindow(reader, nameof(LayoutAnchorableFloatingWindow));
        CopyBoundsFrom(restored);
        RootPanel = restored.RootPanel;
    }

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        base.OnModelPropertyChanged(sender, args);
        if (args.Property == DockBase.VisibleDockablesProperty) Reconnect();
    }
    private void Reconnect()
    {
        if (_changes is not null) _changes.CollectionChanged -= ChildrenChanged;
        _changes = NativeRoot.VisibleDockables as INotifyCollectionChanged;
        if (_changes is not null) _changes.CollectionChanged += ChildrenChanged;
        SynchronizePanel();
    }
    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs args) { if (!_changing) SynchronizePanel(); }
    private void TreeChanged(object? sender, ChildrenTreeChangedEventArgs args) => NotifyPaneState();
    private void SynchronizePanel()
    {
        LayoutAnchorablePaneGroup? next = null;
        if (NativeRoot.VisibleDockables is { Count: > 0 } children && children[0] is ProportionalDock model)
            next = Context.GetAnchorableGroup(model);
        if (ReferenceEquals(_panel, next)) return;
        if (_panel is not null) _panel.ChildrenTreeChanged -= TreeChanged;
        _panel = next;
        if (next is not null) next.ChildrenTreeChanged += TreeChanged;
        RaisePropertyChanged(nameof(RootPanel));
        RaisePropertyChanged(nameof(Children));
        RaisePropertyChanged(nameof(ChildrenCount));
        RaisePropertyChanged(nameof(IsValid));
        NotifyPaneState();
    }
    private void NotifyPaneState()
    {
        RefreshBounds();
        RaisePropertyChanged(nameof(IsSinglePane));
        RaisePropertyChanged(nameof(SinglePane));
        bool visible = SinglePane is not null;
        if (!visible && _panel is not null)
            foreach (ILayoutElement element in _panel.Descendents())
                if (element is LayoutAnchorablePane { IsVisible: true }) { visible = true; break; }
        if (SetField(ref _visible, visible, nameof(IsVisible)))
        {
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
            (Root as LayoutRoot)?.Manager?.PresentFloatingWindow(this);
        }
        (Root as LayoutRoot)?.NotifyUpdated();
    }
}
