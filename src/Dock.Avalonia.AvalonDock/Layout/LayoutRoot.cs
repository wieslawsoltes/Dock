using System;
using System.Collections.Generic;
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
    /// <summary>Gets or sets the active content.</summary>
    LayoutContent? ActiveContent { get; set; }
    /// <summary>Removes empty collapsible groups.</summary>
    void CollectGarbage();
}

/// <summary>A layout root backed by a Dock Avalonia root and factory.</summary>
public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot
{
    private LayoutContent? _lastFocusedDocument;
    /// <summary>Creates a layout with an empty document pane.</summary>
    public LayoutRoot() : this(new RootDock { IsCollapsable = false })
    {
        RootPanel = new LayoutPanel(new LayoutDocumentPane());
    }
    internal LayoutRoot(RootDock model, LayoutModelContext? context = null) : base(model, context)
    {
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
            };
        }
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
            root.DefaultDockable = value.DockModel;
            DockFactory.InitLayout(root);
            RaisePropertyChanged();
            NotifyUpdated();
        }
    }
    /// <inheritdoc/>
    public DockingManager? Manager { get; internal set; }
    /// <inheritdoc/>
    public LayoutContent? ActiveContent
    {
        get => ((RootDock)DockModel).FocusedDockable is { } model ? Context.Get(model) as LayoutContent : null;
        set => DockFactory.SetFocusedDockable((RootDock)DockModel, value?.DockModel);
    }
    /// <summary>Gets the most recently focused document.</summary>
    public LayoutContent? LastFocusedDocument => _lastFocusedDocument;
    /// <inheritdoc/>
    public IEnumerable<ILayoutElement> Children { get { yield return RootPanel; } }
    /// <inheritdoc/>
    public int ChildrenCount => ((RootDock)DockModel).VisibleDockables?.Count ?? 0;
    /// <inheritdoc/>
    public void RemoveChild(ILayoutElement element)
    {
        if (ReferenceEquals(element, RootPanel)) RootPanel = new LayoutPanel(new LayoutDocumentPane());
    }
    /// <inheritdoc/>
    public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        if (ReferenceEquals(oldElement, RootPanel)) RootPanel = (LayoutPanel)newElement;
    }
    /// <inheritdoc/>
    public void CollectGarbage()
    {
        List<LayoutElement> empty = new();
        foreach (ILayoutElement child in this.Descendents())
            if (child is LayoutElement element && element.DockModel.IsCollapsable && child is ILayoutContainer { ChildrenCount: 0 }) empty.Add(element);
        for (int i = empty.Count - 1; i >= 0; i--) empty[i].Parent?.RemoveChild(empty[i]);
    }
    /// <summary>Occurs when the layout tree changes.</summary>
    public event EventHandler? Updated;
    internal void NotifyUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
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
