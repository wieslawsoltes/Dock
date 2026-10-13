using System;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using AvalonDock.Infrastructure;

namespace AvalonDock.Layout;

/// <summary>Describes the scope of a tree change.</summary>
public enum ChildrenTreeChange
{
    /// <summary>The group's immediate children changed.</summary>
    DirectChildrenChanged,
    /// <summary>A descendant's children changed.</summary>
    TreeChanged
}

/// <summary>Describes a layout tree change.</summary>
public class ChildrenTreeChangedEventArgs : EventArgs
{
    /// <summary>Creates event data for a tree change.</summary>
    public ChildrenTreeChangedEventArgs(ChildrenTreeChange change) => Change = change;
    /// <summary>Gets the scope of the change.</summary>
    public ChildrenTreeChange Change { get; }
}

/// <summary>Base for groups exposing collection and tree notifications.</summary>
public abstract class LayoutGroupBase : LayoutElement
{
    /// <summary>Creates an application-defined group backed by a native Dock container.</summary>
    protected LayoutGroupBase() : this(new ProportionalDock()) { }
    internal LayoutGroupBase(DockBase model, LayoutModelContext? context = null) : base(model, context) { }

    /// <summary>Occurs when the immediate children change.</summary>
    public event EventHandler? ChildrenCollectionChanged;
    /// <summary>Occurs when this group's subtree changes.</summary>
    public event EventHandler<ChildrenTreeChangedEventArgs>? ChildrenTreeChanged;
    /// <summary>Raises the immediate collection notification.</summary>
    protected virtual void OnChildrenCollectionChanged() => ChildrenCollectionChanged?.Invoke(this, EventArgs.Empty);
    /// <summary>Raises a tree notification and propagates it to the parent.</summary>
    protected void NotifyChildrenTreeChanged(ChildrenTreeChange change)
    {
        OnChildrenTreeChanged(change);
        if (Parent is LayoutGroupBase parent) parent.NotifyChildrenTreeChanged(ChildrenTreeChange.TreeChanged);
        if (Root is LayoutRoot root) root.NotifyUpdated();
    }
    /// <summary>Raises the tree notification.</summary>
    protected virtual void OnChildrenTreeChanged(ChildrenTreeChange change) =>
        ChildrenTreeChanged?.Invoke(this, new ChildrenTreeChangedEventArgs(change));
}
