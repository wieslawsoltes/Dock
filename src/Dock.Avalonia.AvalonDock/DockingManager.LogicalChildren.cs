using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using AvalonDock.Infrastructure;

namespace AvalonDock;

public partial class DockingManager : ILogical
{
    private readonly List<WeakReference> _publicLogicalChildren = new();
    private LogicalChildrenView? _logicalChildrenView;

    /// <summary>Enumerates the manager's registered presentation children.</summary>
    /// <remarks>Overrides also supply public Avalonia logical-tree traversal. The projection filters
    /// non-logical objects; <see cref="LogicalChildrenPublic"/> preserves the raw enumeration.</remarks>
    protected new virtual IEnumerator LogicalChildren => EnumerateLogicalChildren().GetEnumerator();

    global::Avalonia.Collections.IAvaloniaReadOnlyList<ILogical> ILogical.LogicalChildren =>
        _logicalChildrenView is { } children ? children : base.LogicalChildren;

    private IEnumerable<object?> EnumerateLogicalChildren()
    {
        foreach (WeakReference child in _publicLogicalChildren) yield return child.Target;
    }

    private void InitializeLogicalChildren()
    {
        _logicalChildrenView = new(() => LogicalChildren);
    }

    internal void InternalAddLogicalChild(object? child)
    {
        foreach (WeakReference existing in _publicLogicalChildren)
            if (Equals(existing.Target, child)) return;
        _publicLogicalChildren.Add(new(child));
        if (child is ISetLogicalParent logical) logical.SetParent(this);
        _logicalChildrenView?.NotifyChanged();
    }

    internal void InternalRemoveLogicalChild(object? child)
    {
        for (int i = 0; i < _publicLogicalChildren.Count; i++)
        {
            if (!ReferenceEquals(_publicLogicalChildren[i].Target, child)) continue;
            _publicLogicalChildren.RemoveAt(i);
            break;
        }
        if (child is ILogical { LogicalParent: { } parent } && ReferenceEquals(parent, this))
            ((ISetLogicalParent)child).SetParent(null);
        _logicalChildrenView?.NotifyChanged();
    }

    private void ClearLogicalChildrenList()
    {
        object?[] children = new object?[_publicLogicalChildren.Count];
        for (int i = 0; i < children.Length; i++) children[i] = _publicLogicalChildren[i].Target;
        foreach (object? child in children)
            if (child is ILogical { LogicalParent: { } parent } && ReferenceEquals(parent, this))
                ((ISetLogicalParent)child).SetParent(null);
        _publicLogicalChildren.Clear();
        _logicalChildrenView?.NotifyChanged();
    }

    private void ReplaceLogicalChild(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is { } previous) InternalRemoveLogicalChild(previous);
        if (change.NewValue is { } current) InternalAddLogicalChild(current);
    }
}
