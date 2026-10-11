using System.Collections;
using System.Collections.Specialized;
using Avalonia.Utilities;

namespace AvalonDock.Infrastructure;

internal sealed class LayoutSourceSubscriptions(IWeakEventSubscriber<NotifyCollectionChangedEventArgs> listener)
{
    private INotifyCollectionChanged? _documents;
    private INotifyCollectionChanged? _anchorables;

    internal void SetDocuments(IEnumerable? source) => SetSource(ref _documents, _anchorables, source);

    internal void SetAnchorables(IEnumerable? source) => SetSource(ref _anchorables, _documents, source);

    private void SetSource(ref INotifyCollectionChanged? current, INotifyCollectionChanged? other, IEnumerable? source)
    {
        INotifyCollectionChanged? next = source as INotifyCollectionChanged;
        if (ReferenceEquals(current, next)) return;
        if (current is not null && !ReferenceEquals(current, other))
            WeakEvents.CollectionChanged.Unsubscribe(current, listener);
        current = next;
        // A collection used for both sources must deliver each event only once.
        if (next is not null && !ReferenceEquals(next, other))
            WeakEvents.CollectionChanged.Subscribe(next, listener);
    }
}
