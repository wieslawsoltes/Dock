using System;
using System.Collections.Specialized;

namespace AvalonDock;

/// <summary>Describes a change to a manager's native floating-window controls.</summary>
public class LayoutFloatingWindowControlCollectionChangedEventArgs : EventArgs
{
    /// <summary>Wraps the collection change without replacing its action, items, or indices.</summary>
    public LayoutFloatingWindowControlCollectionChangedEventArgs(NotifyCollectionChangedEventArgs collectionChangedEventArgs) =>
        CollectionChangedEventArgs = collectionChangedEventArgs;

    /// <summary>Gets the original collection action, indices, and window controls.</summary>
    public NotifyCollectionChangedEventArgs CollectionChangedEventArgs { get; private set; }
}
