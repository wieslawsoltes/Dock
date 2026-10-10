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
    [Content]
    public LayoutDocument? RootDocument
    {
        get => _document;
        set
        {
            if (ReferenceEquals(_document, value)) return;
            _changing = true;
            try
            {
                if (_document is { } previous)
                {
                    previous.SetProjectedParent(null);
                    _pane.VisibleDockables?.Remove(previous.DockModel);
                    if (ReferenceEquals(previous.DockModel.Owner, _pane)) previous.DockModel.Owner = null;
                }
                if (value is not null)
                {
                    value.RememberDockedLocation();
                    value.Parent?.RemoveChild(value);
                    Context.Adopt(value.Context);
                    (_pane.VisibleDockables ??= new AvaloniaList<IDockable>()).Add(value.DockModel);
                    if (DockModel.Factory is { } factory) factory.InitDockable(value.DockModel, _pane);
                    else value.DockModel.Owner = _pane;
                }
                _pane.ActiveDockable = value?.DockModel;
                _pane.DefaultDockable = value?.DockModel;
            }
            finally { _changing = false; }
            SynchronizeDocument();
        }
    }
    /// <inheritdoc/>
    public override IEnumerable<ILayoutElement> Children { get { if (RootDocument is { } document) yield return document; } }
    /// <inheritdoc/>
    public override int ChildrenCount => RootDocument is null ? 0 : 1;
    /// <inheritdoc/>
    public override bool IsValid => RootDocument is not null;
    /// <inheritdoc/>
    public override void RemoveChild(ILayoutElement element) { if (ReferenceEquals(element, RootDocument)) RootDocument = null; }
    /// <inheritdoc/>
    public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        if (ReferenceEquals(oldElement, RootDocument)) RootDocument = (LayoutDocument)newElement;
    }
    /// <summary>Occurs when the root document changes.</summary>
    public event EventHandler? RootDocumentChanged;

    /// <inheritdoc/>
    public override void ReadXml(XmlReader reader)
    {
        LayoutDocumentFloatingWindow restored = (LayoutDocumentFloatingWindow)AvalonDockXml.ReadFloatingWindow(reader, nameof(LayoutDocumentFloatingWindow));
        CopyBoundsFrom(restored);
        RootDocument = restored.RootDocument;
    }

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
        _document?.DockingHistory.CaptureRemoval(this, -1);
        _document?.SetProjectedParent(null);
        _document = next;
        next?.SetProjectedParent(this);
        RefreshBounds();
        RaisePropertyChanged(nameof(RootDocument));
        RaisePropertyChanged(nameof(Children));
        RaisePropertyChanged(nameof(ChildrenCount));
        RaisePropertyChanged(nameof(IsValid));
        RootDocumentChanged?.Invoke(this, EventArgs.Empty);
        (Root as LayoutRoot)?.NotifyUpdated();
    }
}
