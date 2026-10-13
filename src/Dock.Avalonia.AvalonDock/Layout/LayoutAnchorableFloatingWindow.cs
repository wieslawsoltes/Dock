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
    private bool _visible = true;

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
    /// <remarks>A removed or replaced group retains its public floating parent until another
    /// container adopts it. Its native Dock owner is detached immediately.</remarks>
    [Content]
    public LayoutAnchorablePaneGroup? RootPanel
    {
        get => _panel;
        set
        {
            if (ReferenceEquals(_panel, value)) return;
            LayoutAnchorablePaneGroup? previous = _panel;
            BeginPanelChange(value);
            _changing = true;
            try
            {
                if (previous is not null)
                {
                    previous.SetProjectedParent(this);
                    previous.DockModel.Owner = null;
                }
                NativeRoot.VisibleDockables!.Clear();
                if (value is not null)
                {
                    if (!ReferenceEquals(value.Parent, this)) LayoutParenting.RemoveFromParent(value);
                    Context.Adopt(value.Context);
                    NativeRoot.VisibleDockables.Add(value.DockModel);
                    if (DockModel.Factory is { } factory) factory.InitDockable(value.DockModel, NativeRoot);
                    else value.DockModel.Owner = NativeRoot;
                }
                NativeRoot.ActiveDockable = value?.DockModel;
                NativeRoot.DefaultDockable = value?.DockModel;
            }
            finally { _changing = false; }
            CompletePanelChange();
        }
    }
    /// <summary>Gets whether exactly one visible anchorable pane or group is a
    /// descendant of the root panel.</summary>
    public bool IsSinglePane => FindSingleVisiblePane() is not null;
    /// <summary>Gets the only visible pane, when there is one.</summary>
    /// <remarks>Reading a single pane refreshes its directly-hosted notification.</remarks>
    public ILayoutAnchorablePane? SinglePane
    {
        get
        {
            ILayoutAnchorablePane? single = FindSingleVisiblePane();
            if (single is null) return null;
            if (single is not LayoutAnchorablePane pane) throw new InvalidOperationException("The visible descendant is not an anchorable pane.");
            pane.NotifyDirectlyHostedInFloatingWindow();
            return pane;
        }
    }
    private ILayoutAnchorablePane? FindSingleVisiblePane()
    {
        ILayoutAnchorablePane? result = null;
        if (_panel is null) return null;
        foreach (ILayoutElement element in _panel.Descendents())
        {
            if (element is not ILayoutAnchorablePane { IsVisible: true } pane) continue;
            if (result is not null) return null;
            result = pane;
        }
        return result;
    }
    /// <summary>Gets whether the floating layout has visible content. An empty, newly
    /// constructed window remains visible until visibility is first computed.</summary>
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
    public override void RemoveChild(ILayoutElement element) => RootPanel = null;
    /// <inheritdoc/>
    public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        RootPanel = newElement as LayoutAnchorablePaneGroup;
    }
    void ILayoutElementWithVisibility.ComputeVisibility() => ComputeVisibility();

    /// <inheritdoc/>
    public override void ReadXml(XmlReader reader) => LayoutXmlDeserializer.ReadContainer(this, reader);

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
    private void TreeChanged(object? sender, ChildrenTreeChangedEventArgs args)
    {
        RefreshBounds();
        NotifyPaneState();
        (Root as LayoutRoot)?.NotifyStructureChanged();
    }
    private void SynchronizePanel()
    {
        LayoutAnchorablePaneGroup? next = null;
        if (NativeRoot.VisibleDockables is { Count: > 0 } children && children[0] is ProportionalDock model)
            next = Context.GetAnchorableGroup(model);
        if (ReferenceEquals(_panel, next)) return;
        BeginPanelChange(next);
        CompletePanelChange();
    }
    private void BeginPanelChange(LayoutAnchorablePaneGroup? next)
    {
        RaisePropertyChanging(nameof(RootPanel));
        if (_panel is not null) _panel.ChildrenTreeChanged -= TreeChanged;
        _panel = next;
    }
    private void CompletePanelChange()
    {
        if (_panel is { } next)
        {
            next.SetProjectedParent(this);
            next.ChildrenTreeChanged += TreeChanged;
        }
        RefreshBounds();
        RaisePropertyChanged(nameof(RootPanel));
        NotifyPaneState();
        RaisePropertyChanged(nameof(Children));
        RaisePropertyChanged(nameof(ChildrenCount));
        ComputeVisibility();
        (Root as LayoutRoot)?.NotifyStructureChanged();
    }
    private void NotifyPaneState()
    {
        RaisePropertyChanged(nameof(IsSinglePane));
        RaisePropertyChanged(nameof(SinglePane));
    }
    private void ComputeVisibility()
    {
        if (SetField(ref _visible, _panel?.IsVisible ?? false, nameof(IsVisible)))
        {
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
            // Existing compatible hosts follow PropertyChanged, like WPF's
            // visibility binding, including a subclass suppressing that event.
            if (DockWindow.Host is not Controls.LayoutFloatingWindowControl)
                (Root as LayoutRoot)?.Manager?.PresentFloatingWindow(this);
        }
    }
}
