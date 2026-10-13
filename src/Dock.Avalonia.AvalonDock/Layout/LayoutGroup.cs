using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A typed view of the children of a live Dock container.</summary>
/// <typeparam name="T">The accepted layout child type.</typeparam>
public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutContainer, ILayoutGroup, IXmlSerializable where T : class, ILayoutElement
{
    private readonly LayoutChildren<T> _children;
    private bool _isVisible = true;

    internal LayoutGroup(DockBase model, LayoutModelContext? context = null) : base(model, context)
    {
        _children = new LayoutChildren<T>(this, model);
        _children.CollectionChanged += (_, _) =>
        {
            ComputeVisibility();
            OnChildrenCollectionChanged();
            NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
            RaisePropertyChanged(nameof(ChildrenCount));
        };
        // Native trees may already have parents and children when first wrapped.
        // Empty, detached public groups retain AvalonDock's initial visible state.
        if (model.Owner is not null || _children.Count > 0) ComputeVisibility();
    }

    /// <summary>Gets the mutable children, synchronized with Dock.</summary>
    [Content]
    public ObservableCollection<T> Children => _children;
    IEnumerable<ILayoutElement> ILayoutContainer.Children => _children;
    /// <inheritdoc/>
    public int ChildrenCount => Children.Count;
    /// <summary>Returns no custom XML schema.</summary>
    public XmlSchema? GetSchema() => null;
    /// <summary>Appends XML children to the live native container.</summary>
    /// <remarks>
    /// Derived model overrides restore their metadata before reading children. This base does not infer metadata from additional interfaces.
    /// The caller's reader stays live; a later read error retains preceding changes and appended children.
    /// </remarks>
    public virtual void ReadXml(XmlReader reader)
    {
        if (LayoutXmlDeserializer.ReadContainer(this, reader)) ComputeVisibility();
    }
    /// <summary>Writes children inside the caller's XML element after any derived metadata.</summary>
    public virtual void WriteXml(XmlWriter writer) => AvalonDockXml.WriteContents(this, writer);
    /// <inheritdoc/>
    public bool IsVisible
    {
        get => _isVisible;
        protected set
        {
            VerifyAccess();
            if (_isVisible == value) return;
            RaisePropertyChanging();
            _isVisible = value;
            OnIsVisibleChanged();
            RaisePropertyChanged();
        }
    }
    /// <summary>Determines whether the group has visible content.</summary>
    protected virtual bool GetVisibility() => Children.Count > 0;
    /// <inheritdoc/>
    public void ComputeVisibility()
    {
        // Child wrappers can notify their registered parent while its own
        // native children are still being wrapped by the constructor.
        if (_children is not null) IsVisible = GetVisibility();
    }

    /// <summary>Updates ancestor visibility after this group's visible state changes.</summary>
    protected virtual void OnIsVisibleChanged() => (Parent as ILayoutElementWithVisibility)?.ComputeVisibility();

    /// <summary>Recomputes visibility using the new parent context.</summary>
    protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
    {
        base.OnParentChanged(oldValue, newValue);
        ComputeVisibility();
    }
    /// <inheritdoc/>
    public void MoveChild(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex) return;
        Children.Move(oldIndex, newIndex);
        ChildMoved(oldIndex, newIndex);
    }
    /// <summary>Called after a child has been moved to its new index.</summary>
    protected virtual void ChildMoved(int oldIndex, int newIndex) { }
    /// <inheritdoc/>
    public void RemoveChildAt(int childIndex) => Children.RemoveAt(childIndex);
    /// <inheritdoc/>
    public int IndexOfChild(ILayoutElement element) => element is T child ? Children.IndexOf(child) : -1;
    /// <inheritdoc/>
    public void InsertChildAt(int index, ILayoutElement element) => Children.Insert(index, (T)element);
    /// <inheritdoc/>
    /// <remarks>An element of an incompatible type raises a cast exception; an absent compatible element is ignored.</remarks>
    public void RemoveChild(ILayoutElement element) => Children.Remove((T)element);
    /// <inheritdoc/>
    /// <remarks>The old element must have the group's child type. The replacement is checked only when the old element is found.</remarks>
    public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        int index = Children.IndexOf((T)oldElement);
        if (index >= 0) ReplaceChildAt(index, newElement);
    }
    /// <inheritdoc/>
    /// <remarks>An index outside the current collection is ignored before checking the replacement's type.</remarks>
    public void ReplaceChildAt(int index, ILayoutElement element)
    {
        if ((uint)index < (uint)Children.Count) Children[index] = (T)element;
    }

    internal void AddProjectedChild(T child) => _children.AddProjected(child);
    internal void RemoveProjectedChild(T child) => _children.RemoveProjected(child);
    internal bool IsRefreshingNativeChildren => _children?.IsRefreshingSource == true;

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockBase.VisibleDockablesProperty) _children?.Reconnect();
    }
}
