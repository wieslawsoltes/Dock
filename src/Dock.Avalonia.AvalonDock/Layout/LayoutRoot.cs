using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>The root contract of an AvalonDock layout.</summary>
public interface ILayoutRoot
{
    /// <summary>Gets the manager displaying the layout.</summary>
    DockingManager? Manager { get; }
    /// <summary>Gets the root split panel.</summary>
    LayoutPanel RootPanel { get; }
    /// <summary>Gets the top auto-hide strip.</summary>
    LayoutAnchorSide? TopSide { get; }
    /// <summary>Gets the left auto-hide strip.</summary>
    LayoutAnchorSide? LeftSide { get; }
    /// <summary>Gets the right auto-hide strip.</summary>
    LayoutAnchorSide? RightSide { get; }
    /// <summary>Gets the bottom auto-hide strip.</summary>
    LayoutAnchorSide? BottomSide { get; }
    /// <summary>Gets or sets the active document or tool across docked and floating layouts.</summary>
    LayoutContent? ActiveContent { get; set; }
    /// <summary>Gets the hidden tools.</summary>
    ObservableCollection<LayoutAnchorable> Hidden { get; }
    /// <summary>Gets the floating windows.</summary>
    ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; }
    /// <summary>Removes empty collapsible groups.</summary>
    void CollectGarbage();
}

/// <summary>A layout root backed by a Dock Avalonia root and factory.</summary>
public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot, IXmlSerializable
{
    private readonly LayoutChildren<LayoutAnchorable> _hidden;
    private readonly LayoutFloatingWindows _floatingWindows;
    private LayoutAnchorSide? _leftSide;
    private LayoutAnchorSide? _topSide;
    private LayoutAnchorSide? _rightSide;
    private LayoutAnchorSide? _bottomSide;
    private DockingManager? _manager;
    /// <summary>Creates a layout with an empty document pane.</summary>
    public LayoutRoot() : this(new RootDock { IsCollapsable = false })
    {
        RootPanel = new LayoutPanel(new LayoutDocumentPane());
        DockFactory.InitLayout(DockModel);
    }
    internal LayoutRoot(RootDock model, LayoutModelContext? context = null) : base(model, context)
    {
        Operations = new LayoutOperationEvents(this);
        _hidden = new LayoutChildren<LayoutAnchorable>(this, model, () => model.HiddenDockables, value => model.HiddenDockables = value);
        if (model.Factory is null)
        {
            Factory factory = new LayoutFactory(window => LayoutWindowClosing.TryClose(this, window), new LayoutFloatingPlacement(this), new LayoutDropPolicy(this, new LayoutDuplicateContentPolicy(Context)), Operations,
                dockable => Context.Get(dockable) is LayoutContent content ? Manager?.GetLayoutItemForNativePresentation(content) : null,
                dockable => Context.Get(dockable))
                { HideToolsOnClose = false, HideDocumentsOnClose = false };
            model.Factory = factory;
            factory.DockableClosing += (_, args) =>
            {
                if (args.Dockable is null || Context.Get(args.Dockable) is not LayoutContent content) return;
                CancelEventArgs forwarded = new(args.Cancel);
                if (content is LayoutDocument document) Manager?.NotifyDocumentClosing(document, forwarded);
                if (!forwarded.Cancel) content.NotifyClosing(forwarded);
                args.Cancel |= forwarded.Cancel;
            };
            factory.DockableClosed += (_, args) =>
            {
                if (args.Dockable is null || Context.Get(args.Dockable) is not LayoutContent content) return;
                DockingManager? manager = Manager;
                content.DockModel.Owner = null;
                CollectGarbage();
                content.NotifyClosed();
                if (content is LayoutDocument document) manager?.NotifyDocumentClosed(document);
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
                if (args.Dockable is Tool hidden && Context.Get(hidden) is LayoutContent content)
                    content.DockingHistory.CompleteHide();
            };
        }
        RightSide = new LayoutAnchorSide();
        LeftSide = new LayoutAnchorSide();
        TopSide = new LayoutAnchorSide();
        BottomSide = new LayoutAnchorSide();
        _floatingWindows = new LayoutFloatingWindows(this);
        Focus = new LayoutActiveContent(this, RaisePropertyChanging, RaisePropertyChanged);
    }
    /// <summary>Gets the Dock factory which operates this layout.</summary>
    public IFactory DockFactory => DockModel.Factory!;
    internal ILayoutOperationEvents Operations { get; }
    internal LayoutActiveContent Focus { get; }
    /// <summary>Gets or sets the root split panel; assigning null creates an empty document layout.</summary>
    [Content]
    [AllowNull]
    public LayoutPanel RootPanel
    {
        get
        {
            if (((RootDock)DockModel).VisibleDockables is { Count: > 0 } children)
                return (LayoutPanel)Context.Get(children[0]);
            return null!;
        }
        set
        {
            VerifyAccess();
            RootDock root = (RootDock)DockModel;
            if (ReferenceEquals(RootPanel, value)) return;
            RaisePropertyChanging();
            if (RootPanel is { } previous && ReferenceEquals(previous.Parent, this)) previous.Parent = null;
            value ??= new LayoutPanel(new LayoutDocumentPane());
            value.Parent?.RemoveChild(value);
            Context.Adopt(value.Context);
            root.VisibleDockables!.Clear();
            root.VisibleDockables.Add(value.DockModel);
            value.DockModel.IsCollapsable = false;
            root.DefaultDockable = value.DockModel;
            DockFactory.InitDockable(value.DockModel, root);
            root.ActiveDockable = value.DockModel;
            RaisePropertyChanged();
            NotifyStructureChanged();
        }
    }
    /// <inheritdoc/>
    public DockingManager? Manager { get => _manager; internal set => SetField(ref _manager, value); }
    /// <summary>Gets or sets the left auto-hide strip; null removes the strip.</summary>
    /// <remarks>Replacing the slot retains the former side's public parent and releases its unused native projection.</remarks>
    public LayoutAnchorSide? LeftSide { get => _leftSide; set => SetSide(ref _leftSide, value, nameof(LeftSide)); }
    /// <summary>Gets or sets the top auto-hide strip; null removes the strip.</summary>
    /// <remarks>Replacing the slot retains the former side's public parent and releases its unused native projection.</remarks>
    public LayoutAnchorSide? TopSide { get => _topSide; set => SetSide(ref _topSide, value, nameof(TopSide)); }
    /// <summary>Gets or sets the right auto-hide strip; null removes the strip.</summary>
    /// <remarks>Replacing the slot retains the former side's public parent and releases its unused native projection.</remarks>
    public LayoutAnchorSide? RightSide { get => _rightSide; set => SetSide(ref _rightSide, value, nameof(RightSide)); }
    /// <summary>Gets or sets the bottom auto-hide strip; null removes the strip.</summary>
    /// <remarks>Replacing the slot retains the former side's public parent and releases its unused native projection.</remarks>
    public LayoutAnchorSide? BottomSide { get => _bottomSide; set => SetSide(ref _bottomSide, value, nameof(BottomSide)); }

    private void SetSide(ref LayoutAnchorSide? field, LayoutAnchorSide? value, string name)
    {
        VerifyAccess();
        if (ReferenceEquals(field, value)) return;
        RaisePropertyChanging(name);
        LayoutAnchorSide? previous = field;
        field = value;
        previous?.ReleaseProjection(this);
        value?.Attach(this);
        RaisePropertyChanged(name);
    }

    internal AnchorSide? GetSideSlot(LayoutAnchorSide side)
    {
        LayoutAnchorSide? current = side.Side switch
        {
            AnchorSide.Left => _leftSide,
            AnchorSide.Top => _topSide,
            AnchorSide.Right => _rightSide,
            _ => _bottomSide
        };
        if (ReferenceEquals(current, side)) return side.Side;
        if (ReferenceEquals(_leftSide, side)) return AnchorSide.Left;
        if (ReferenceEquals(_topSide, side)) return AnchorSide.Top;
        if (ReferenceEquals(_rightSide, side)) return AnchorSide.Right;
        if (ReferenceEquals(_bottomSide, side)) return AnchorSide.Bottom;
        return null;
    }
    /// <inheritdoc/>
    public LayoutContent? ActiveContent
    {
        get => Focus.Active;
        set => Focus.Activate(value);
    }
    /// <summary>Gets the most recently focused document or tool hosted in a document pane, including floating layouts.</summary>
    public LayoutContent? LastFocusedDocument => Focus.LastDocument;
    /// <inheritdoc/>
    public ObservableCollection<LayoutAnchorable> Hidden => _hidden;
    /// <inheritdoc/>
    /// <remarks>
    /// Indexed replacement publishes one replacement notification after parent hooks and retains native host close cancellation.
    /// A failing public callback leaves completed collection and parent phases in place; assigning the entry again can retry attachment.
    /// Parent hooks participate in observable collection reentrancy checks before nested edits change native membership.
    /// </remarks>
    public ObservableCollection<LayoutFloatingWindow> FloatingWindows => _floatingWindows;
    /// <inheritdoc/>
    public IEnumerable<ILayoutElement> Children
    {
        get
        {
            if (RootPanel is { } panel) yield return panel;
            if (_floatingWindows is not null)
                foreach (LayoutFloatingWindow window in _floatingWindows) yield return window;
            if (TopSide is { } top) yield return top;
            if (RightSide is { } right) yield return right;
            if (BottomSide is { } bottom) yield return bottom;
            if (LeftSide is { } left) yield return left;
            foreach (LayoutAnchorable hidden in Hidden) yield return hidden;
        }
    }
    /// <summary>Gets five fixed root slots plus the number of floating windows and hidden tools.</summary>
    /// <remarks>Null sides are omitted from <see cref="Children"/> but still count as fixed slots.</remarks>
    public int ChildrenCount => 5 + (_hidden?.Count ?? 0) + (_floatingWindows?.Count ?? 0);
    /// <inheritdoc/>
    public void RemoveChild(ILayoutElement element)
    {
        if (ReferenceEquals(element, RootPanel)) RootPanel = null;
        else if (element is LayoutFloatingWindow window) FloatingWindows.Remove(window);
        else if (element is LayoutAnchorable hidden) Hidden.Remove(hidden);
        else if (ReferenceEquals(element, TopSide)) TopSide = null;
        else if (ReferenceEquals(element, RightSide)) RightSide = null;
        else if (ReferenceEquals(element, BottomSide)) BottomSide = null;
        else if (ReferenceEquals(element, LeftSide)) LeftSide = null;
    }
    /// <inheritdoc/>
    public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        if (ReferenceEquals(oldElement, RootPanel)) RootPanel = (LayoutPanel)newElement;
        else if (oldElement is LayoutFloatingWindow window && FloatingWindows.IndexOf(window) is int windowIndex && windowIndex >= 0)
        {
            FloatingWindows.RemoveAt(windowIndex);
            if (!FloatingWindows.Contains(window)) FloatingWindows.Insert(windowIndex, (LayoutFloatingWindow)newElement);
        }
        else if (oldElement is LayoutAnchorable hidden && Hidden.IndexOf(hidden) is int index && index >= 0)
        {
            Hidden.RemoveAt(index);
            Hidden.Insert(index, (LayoutAnchorable)newElement);
        }
        else if (ReferenceEquals(oldElement, TopSide)) TopSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, RightSide)) RightSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, BottomSide)) BottomSide = (LayoutAnchorSide)newElement;
        else if (ReferenceEquals(oldElement, LeftSide)) LeftSide = (LayoutAnchorSide)newElement;
    }
    /// <inheritdoc/>
    /// <remarks>Removes empty panes in tree order, retaining the last document pane.
    /// Container pruning follows a successful pane removal. Nested tool groups
    /// collapse before document groups, including live changes from callbacks.</remarks>
    public void CollectGarbage()
    {
        LayoutGarbageCollector.Collect(this);
        Focus.Validate();
    }
    /// <summary>Returns no custom XML schema.</summary>
    public XmlSchema? GetSchema() => null;
    /// <summary>Reads the layout hierarchy while retaining this root's native model and factory.</summary>
    /// <remarks>Sections restore as they are read. A later error retains preceding changes.
    /// Use XmlLayoutSerializer to keep the manager's existing layout until parsing and content recovery succeed.</remarks>
    public void ReadXml(XmlReader reader) => LayoutXmlDeserializer.ReadRoot(this, reader);
    /// <summary>Writes the layout hierarchy inside the caller's XML element.</summary>
    public void WriteXml(XmlWriter writer) => AvalonDockXml.WriteContents(this, writer);
    /// <summary>Occurs after an element's parent change completes within this layout.</summary>
    /// <remarks>Reordering children and propagating group-tree notifications do not raise this event.
    /// The root is resolved after the element's parent property observers return.</remarks>
    public event EventHandler? Updated;
    // Native presentation also needs refreshes for mutations that preserve
    // public parents, including reordering and removing a floating child.
    internal event EventHandler? StructureChanged;
    /// <summary>Occurs when a parent transition attaches an element or subtree to this layout.</summary>
    public event EventHandler<LayoutElementEventArgs>? ElementAdded;
    /// <summary>Occurs when a parent transition removes an element or subtree from this layout.</summary>
    public event EventHandler<LayoutElementEventArgs>? ElementRemoved;
    internal void NotifyElementAdded(LayoutElement element) => ElementAdded?.Invoke(this, new LayoutElementEventArgs(element));
    internal void NotifyElementRemoved(LayoutElement element)
    {
        Focus?.Validate();
        ElementRemoved?.Invoke(this, new LayoutElementEventArgs(element));
    }
    internal void NotifyUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    internal void NotifyStructureChanged() => StructureChanged?.Invoke(this, EventArgs.Empty);
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == RootDock.HiddenDockablesProperty) _hidden?.Reconnect();
        if (e.Property == RootDock.LeftPinnedDockablesProperty) _leftSide?.Reconnect();
        if (e.Property == RootDock.TopPinnedDockablesProperty) _topSide?.Reconnect();
        if (e.Property == RootDock.RightPinnedDockablesProperty) _rightSide?.Reconnect();
        if (e.Property == RootDock.BottomPinnedDockablesProperty) _bottomSide?.Reconnect();
        if (e.Property == RootDock.WindowsProperty) _floatingWindows?.Reconnect();
        if (e.Property == DockBase.FocusedDockableProperty) Focus?.NativeFocusChanged((RootDock)DockModel);
    }
}
