using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Metadata;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>A typed view of the children of a live Dock container.</summary>
/// <typeparam name="T">The accepted layout child type.</typeparam>
public abstract class LayoutGroup<T> : LayoutGroupBase, ILayoutGroup, ILayoutPane, ILayoutPanelElement where T : class, ILayoutElement
{
    private readonly LayoutChildren<T> _children;

    internal LayoutGroup(DockBase model, LayoutModelContext? context = null) : base(model, context)
    {
        _children = new LayoutChildren<T>(this, model);
        _children.CollectionChanged += (_, _) =>
        {
            RaisePropertyChanged(nameof(ChildrenCount));
            ComputeVisibility();
            OnChildrenCollectionChanged();
            NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
        };
    }

    /// <summary>Gets the mutable children, synchronized with Dock.</summary>
    [Content]
    public ObservableCollection<T> Children => _children;
    IEnumerable<ILayoutElement> ILayoutContainer.Children => _children;
    /// <inheritdoc/>
    public int ChildrenCount => Children.Count;
    /// <inheritdoc/>
    public bool IsVisible => GetVisibility();
    /// <summary>Determines whether the group has visible content.</summary>
    protected virtual bool GetVisibility() => Children.Count > 0;
    /// <inheritdoc/>
    public void ComputeVisibility() => RaisePropertyChanged(nameof(IsVisible));
    /// <inheritdoc/>
    public void MoveChild(int oldIndex, int newIndex) => Children.Move(oldIndex, newIndex);
    /// <inheritdoc/>
    public void RemoveChildAt(int childIndex) => Children.RemoveAt(childIndex);
    /// <inheritdoc/>
    public int IndexOfChild(ILayoutElement element) => element is T child ? Children.IndexOf(child) : -1;
    /// <inheritdoc/>
    public void InsertChildAt(int index, ILayoutElement element) => Children.Insert(index, (T)element);
    /// <inheritdoc/>
    public void RemoveChild(ILayoutElement element)
    {
        if (element is T child) Children.Remove(child);
    }
    /// <inheritdoc/>
    public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
    {
        int index = IndexOfChild(oldElement);
        if (index >= 0) ReplaceChildAt(index, newElement);
    }
    /// <inheritdoc/>
    public void ReplaceChildAt(int index, ILayoutElement element) => Children[index] = (T)element;

    internal void AddProjectedChild(T child) => _children.AddProjected(child);
    internal void RemoveProjectedChild(T child) => _children.RemoveProjected(child);

    internal override void OnModelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        base.OnModelPropertyChanged(sender, e);
        if (e.Property == DockBase.VisibleDockablesProperty) _children?.Reconnect();
    }
}
