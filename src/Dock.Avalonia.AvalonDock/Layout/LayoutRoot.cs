using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xceed.Wpf.AvalonDock.Infrastructure;

namespace Xceed.Wpf.AvalonDock.Layout;

/// <summary>The root contract of an AvalonDock layout.</summary>
public interface ILayoutRoot
{
    /// <summary>Gets the manager displaying the layout.</summary>
    DockingManager? Manager { get; }
    /// <summary>Gets the root split panel.</summary>
    LayoutPanel RootPanel { get; }
    /// <summary>Gets the top auto-hide strip.</summary>
    LayoutAnchorSide TopSide { get; }
    /// <summary>Gets the left auto-hide strip.</summary>
    LayoutAnchorSide LeftSide { get; }
    /// <summary>Gets the right auto-hide strip.</summary>
    LayoutAnchorSide RightSide { get; }
    /// <summary>Gets the bottom auto-hide strip.</summary>
    LayoutAnchorSide BottomSide { get; }
    /// <summary>Gets or sets the active content.</summary>
    LayoutContent? ActiveContent { get; set; }
    /// <summary>Gets the hidden tools.</summary>
    ObservableCollection<LayoutAnchorable> Hidden { get; }
    /// <summary>Gets the floating windows.</summary>
    ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }
    /// <summary>Removes empty collapsible groups.</summary>
    void CollectGarbage();
}

/// <summary>A layout root backed by a Dock Avalonia root and factory.</summary>
public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot
{
    private LayoutContent? _lastFocusedDocument;
    private readonly LayoutChildren<LayoutAnchorable> _hidden;
    private readonly LayoutFloatingWindows _floatingWindows;
    private LayoutAnchorSide _leftSide = null!;
    private LayoutAnchorSide _topSide = null!;
    private LayoutAnchorSide _rightSide = null!;
    private LayoutAnchorSide _bottomSide = null!;
    /// <summary>Creates a layout with an empty document pane.</summary>
    public LayoutRoot() : this(new RootDock { IsCollapsable = false })
    {
        RootPanel = new LayoutPanel(new LayoutDocumentPane());
        DockFactory.InitLayout(DockModel);
    }
    internal LayoutRoot(RootDock model, LayoutModelContext? context = null) : base(model, context)
    {
        _hidden = new LayoutChildren<LayoutAnchorable>(this, model, () => model.HiddenDockables, value => model.HiddenDockables = value);
        if (model.Factory is null)
        {
            Factory factory = new() { HideToolsOnClose = false, HideDocumentsOnClose = false };
            model.Factory = factory;
            factory.DockableClosing += (_, args) =>
            {
                if (args.Dockable is null || Context.Get(args.Dockable) is not LayoutContent content) return;
                CancelEventArgs forwarded = new(args.Cancel);
                content.NotifyClosing(forwarded);
                if (content is LayoutDocument document) Manager?.NotifyDocumentClosing(document, forwarded);
                args.Cancel |= forwarded.Cancel;
            };
            factory.DockableClosed += (_, args) =>
            {
                if (args.Dockable is null || Context.Get(args.Dockable) is not LayoutContent content) return;
                content.NotifyClosed();
                if (content is LayoutDocument document) Manager?.NotifyDocumentClosed(document);
                content.DockModel.Owner = null;
                CollectGarbage();
            };
            factory.DockableDocked += (_, args) =>
            {
                if (args.Dockable is Document or Tool && Context.Get(args.Dockable) is LayoutContent content)
                {
                    content.DockingHistory.CompleteMove();
                    CollectGarbage();
                }
            };
            factory.DockableHidden += (_, args) =>
            {
                if (args.Dockable is Tool tool && tool.Owner is RootDock floatingRoot &&
                    !ReferenceEquals(floatingRoot, model) && floatingRoot.HiddenDockables?.Remove(tool) == true)
                    Hidden.Add((LayoutAnchorable)Context.Get(tool));
            };
        }
        LeftSide = new LayoutAnchorSide();
        TopSide = new LayoutAnchorSide();
        RightSide = new LayoutAnchorSide();
        BottomSide = new LayoutAnchorSide();
        _floatingWindows = new LayoutFloatingWindows(this);
    }
    /// <summary>Gets the Dock factory which operates this layout.</summary>
    public IFactory DockFactory => DockModel.Factory!;
    /// <summary>Gets or sets the root split panel.</summary>
    [Content]
    public LayoutPanel RootPanel
    {
        get
        {
            if (((RootDock)DockModel).VisibleDockables is { Count: > 0 } children)
                return (LayoutPanel)Context.Get(children[0]);
            throw new InvalidOperationException("The layout does not have a root panel.");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            RootDock root = (RootDock)DockModel;
            if (root.VisibleDockables is { Count: > 0 } old && ReferenceEquals(old[0], value.DockModel)) return;
            if (root.VisibleDockables is { Count: > 0 } previous) previous[0].Owner = null;
            value.Parent?.RemoveChild(value);
            Context.Adopt(value.Context);
            root.VisibleDockables!.Clear();
            root.VisibleDockables.Add(value.DockModel);
            value.DockModel.IsCollapsable = false;
            root.DefaultDockable = value.DockModel;
            DockFactory.InitDockable(value.DockModel, root);
            root.ActiveDockable = value.DockModel;
            RaisePropertyChanged();
            NotifyUpdated();
        }
    }
    /// <inheritdoc/>
    public DockingManager? Manager { get; internal set; }
    /// <summary>Gets or sets the left auto-hide strip.</summary>
    public LayoutAnchorSide LeftSide { get => _leftSide; set => SetSide(ref _leftSide, value, AnchorSide.Left, nameof(LeftSide)); }
    /// <summary>Gets or sets the top auto-hide strip.</summary>
    public LayoutAnchorSide TopSide { get => _topSide; set => SetSide(ref _topSide, value, AnchorSide.Top, nameof(TopSide)); }
    /// <summary>Gets or sets the right auto-hide strip.</summary>
    public LayoutAnchorSide RightSide { get => _rightSide; set => SetSide(ref _rightSide, value, AnchorSide.Right, nameof(RightSide)); }
    /// <summary>Gets or sets the bottom auto-hide strip.</summary>
    public LayoutAnchorSide BottomSide { get => _bottomSide; set => SetSide(ref _bottomSide, value, AnchorSide.Bottom, nameof(BottomSide)); }

    private void SetSide(ref LayoutAnchorSide field, LayoutAnchorSide value, AnchorSide side, string name)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (ReferenceEquals(field, value)) return;
        value.Parent?.RemoveChild(value);
        field?.Detach();
        field = value;
        value.Attach(this, side);
        RaisePropertyChanged(name);
        NotifyUpdated();
    }
    /// <inheritdoc/>
    public LayoutContent? ActiveContent
    {
        get => ((RootDock)DockModel).FocusedDockable is { } model ? Context.Get(model) as LayoutContent : null;
        set => DockFactory.SetFocusedDockable((RootDock)DockModel, value?.DockModel);
    }
    /// <summary>Gets the most recently focused document.</summary>
    public LayoutContent? LastFocusedDocument => _lastFocusedDocument;
    /// <inheritdoc/>
    public ObservableCollection<LayoutAnchorable> Hidden => _hidden;
    /// <inheritdoc/>
    public ObservableCollection<LayoutFloatingWindow> FloatingWindows => _floatingWindows;
    /// <inheritdoc/>
    public IEnumerable<ILayoutElement> Children
    {
        get
        {
            yield return RootPanel;
            yield return TopSide;
            yield return RightSide;
            yield return LeftSide;
            yield return BottomSide;
            foreach (LayoutFloatingWindow window in FloatingWindows) yield return window;
            foreach (LayoutAnchorable hidden in Hidden) yield return hidden;
        }
    }
    /// <inheritdoc/>
    public int ChildrenCount => (((RootDock)DockModel).VisibleDockables?.Count ?? 0) + 4 + Hidden.Count + FloatingWindows.Count;
    /// <inheritdoc/>
    public void RemoveChild(ILayoutElement element)
    {
        if (ReferenceEquals(element, RootPanel)) RootPanel = new LayoutPanel(new LayoutDocumentPane());
        else if (ReferenceEquals(element, LeftSide)) LeftSide = new LayoutAnchorSide();
        else if (ReferenceEquals(element, TopSide)) TopSide = new LayoutAnchorSide();
        else if (ReferenceEquals(element, RightSide)) RightSide = new LayoutAnchorSide();
        else if (ReferenceEquals(element, BottomSide)) BottomSide = new LayoutAnchorSide();
        else if (element is LayoutAnchorable hidden) Hidden.Remove(hidden);
        else if (element is LayoutFloatingWindow window) FloatingWindows.Remove(window);
    }
    /// <inheritdoc/>
    public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        if (ReferenceEquals(oldElement, RootPanel)) RootPanel = (LayoutPanel)newElement;
        else if (ReferenceEquals(oldElement, LeftSide)) LeftSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, TopSide)) TopSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, RightSide)) RightSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, BottomSide)) BottomSide = (LayoutAnchorSide)newElement;
        else if (oldElement is LayoutAnchorable hidden && Hidden.IndexOf(hidden) is int index && index >= 0)
            Hidden[index] = (LayoutAnchorable)newElement;
        else if (oldElement is LayoutFloatingWindow window && FloatingWindows.IndexOf(window) is int windowIndex && windowIndex >= 0)
            FloatingWindows[windowIndex] = (LayoutFloatingWindow)newElement;
    }
    /// <inheritdoc/>
    public void CollectGarbage() => LayoutGarbageCollector.Collect(this);
    /// <summary>Occurs when the layout tree changes.</summary>
    public event EventHandler? Updated;
    internal void NotifyUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == RootDock.HiddenDockablesProperty) _hidden?.Reconnect();
        if (e.Property == RootDock.LeftPinnedDockablesProperty) _leftSide?.Reconnect();
        if (e.Property == RootDock.TopPinnedDockablesProperty) _topSide?.Reconnect();
        if (e.Property == RootDock.RightPinnedDockablesProperty) _rightSide?.Reconnect();
        if (e.Property == RootDock.BottomPinnedDockablesProperty) _bottomSide?.Reconnect();
        if (e.Property == RootDock.WindowsProperty) _floatingWindows?.Reconnect();
        if (e.Property == DockBase.FocusedDockableProperty)
        {
            if (ActiveContent is LayoutDocument document)
            {
                _lastFocusedDocument = document;
                RaisePropertyChanged(nameof(LastFocusedDocument));
            }
            RaisePropertyChanged(nameof(ActiveContent));
        }
    }
}
