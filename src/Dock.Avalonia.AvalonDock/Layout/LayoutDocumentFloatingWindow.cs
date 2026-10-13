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

/// <summary>A document floating window rendered through Dock's document pane.</summary>
public class LayoutDocumentFloatingWindow : LayoutFloatingWindow
{
    private DocumentDock _pane = null!;
    private LayoutDocument? _document;
    private INotifyCollectionChanged? _changes;
    private bool _changing;

    /// <summary>Creates an empty document floating window.</summary>
    public LayoutDocumentFloatingWindow() => InitializePane();
    internal LayoutDocumentFloatingWindow(DockWindow window, LayoutModelContext context) : base(window, context) => InitializePane();

    private void InitializePane()
    {
        _pane = NativeRoot.VisibleDockables is { Count: > 0 } children && children[0] is DocumentDock pane ? pane : new DocumentDock();
        _pane.IsCollapsable = false;
        if (NativeRoot.VisibleDockables!.Count == 0) NativeRoot.VisibleDockables.Add(_pane);
        _pane.Owner = NativeRoot;
        NativeRoot.ActiveDockable = _pane;
        NativeRoot.DefaultDockable = _pane;
        _pane.PropertyChanged += PaneChanged;
        Reconnect();
    }

    /// <summary>Gets or sets the document displayed in this window.</summary>
    /// <remarks>A removed or replaced document retains its public floating parent until another
    /// container adopts it. Its native Dock owner is detached immediately.</remarks>
    [Content]
    public LayoutDocument? RootDocument
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value)) return;
            LayoutDocument? previous = _document;
            BeginDocumentChange(value);
            _changing = true;
            try
            {
                if (previous is not null)
                {
                    _pane.VisibleDockables?.Remove(previous.DockModel);
                    if (ReferenceEquals(previous.DockModel.Owner, _pane)) previous.DockModel.Owner = null;
                }
                if (value is not null)
                {
                    value.RememberDockedLocation();
                    if (!ReferenceEquals(value.Parent, this)) LayoutParenting.RemoveFromParent(value);
                    Context.Adopt(value.Context);
                    (_pane.VisibleDockables ??= new AvaloniaList<IDockable>()).Add(value.DockModel);
                    if (DockModel.Factory is { } factory) factory.InitDockable(value.DockModel, _pane);
                    else value.DockModel.Owner = _pane;
                }
                _pane.ActiveDockable = value?.DockModel;
                _pane.DefaultDockable = value?.DockModel;
            }
            finally { _changing = false; }
            CompleteDocumentChange();
        }
    }
    /// <inheritdoc/>
    public override IEnumerable<ILayoutElement> Children { get { if (RootDocument is { } document) yield return document; } }
    /// <inheritdoc/>
    public override int ChildrenCount => RootDocument is null ? 0 : 1;
    /// <inheritdoc/>
    public override bool IsValid => RootDocument is not null;
    /// <inheritdoc/>
    public override void RemoveChild(ILayoutElement element) => RootDocument = null;
    /// <inheritdoc/>
    public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        RootDocument = newElement as LayoutDocument;
    }
    /// <summary>Occurs when the root document changes.</summary>
    /// <remarks>Floating hosts use this event for deferred closure when the document becomes null.
    /// Overriding the property-notification hook does not suppress this event.</remarks>
    public event EventHandler? RootDocumentChanged;

    internal override bool OwnsNativeChild(IDockable owner) => ReferenceEquals(owner, _pane);

    /// <inheritdoc/>
    public override void ReadXml(XmlReader reader) => LayoutXmlDeserializer.ReadContainer(this, reader);

    private void Reconnect()
    {
        if (_changes is not null) _changes.CollectionChanged -= ChildrenChanged;
        _changes = _pane.VisibleDockables as INotifyCollectionChanged;
        if (_changes is not null) _changes.CollectionChanged += ChildrenChanged;
        SynchronizeDocument();
    }
    private void PaneChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == DockBase.VisibleDockablesProperty) Reconnect();
    }
    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs args) { if (!_changing) SynchronizeDocument(); }
    private void SynchronizeDocument()
    {
        LayoutDocument? next = null;
        if (_pane.VisibleDockables is { } children)
            foreach (IDockable model in children) if (model is Document) { next = (LayoutDocument)Context.Get(model); break; }
        if (ReferenceEquals(_document, next)) return;
        BeginDocumentChange(next);
        CompleteDocumentChange();
    }
    private void BeginDocumentChange(LayoutDocument? next)
    {
        RaisePropertyChanging(nameof(RootDocument));
        _document?.DockingHistory.CaptureRemoval(this, -1);
        // The public document retains its floating parent until another parent
        // adopts it. Closing the host subsequently detaches that floating model.
        _document = next;
    }
    private void CompleteDocumentChange()
    {
        _document?.SetProjectedParent(this);
        RefreshBounds();
        RaisePropertyChanged(nameof(RootDocument));
        RootDocumentChanged?.Invoke(this, EventArgs.Empty);
        (Root as LayoutRoot)?.NotifyUpdated();
    }
}
