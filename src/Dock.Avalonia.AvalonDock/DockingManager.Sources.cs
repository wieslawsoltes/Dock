using System;
using System.Collections.Specialized;
using Avalonia.Utilities;

namespace AvalonDock;

public partial class DockingManager : IWeakEventSubscriber<NotifyCollectionChangedEventArgs>
{
    /// <summary>Receives source collection changes before updating the layout.</summary>
    /// <param name="managerType">For collection changes, the Avalonia type
    /// <see cref="WeakEvent{TSender,TEventArgs}"/> with <see cref="INotifyCollectionChanged"/>
    /// and <see cref="NotifyCollectionChangedEventArgs"/> type arguments.</param>
    /// <param name="sender">The source collection.</param>
    /// <param name="e">The collection change arguments.</param>
    /// <returns>Whether the event manager type is recognized.</returns>
    /// <remarks>Call the base implementation to update the current source's layout,
    /// or return true to handle the event without the default update. When both
    /// sources are the same collection, documents take precedence. Returning false
    /// for a delivered collection notification causes an <see cref="InvalidOperationException"/>.</remarks>
    protected virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
    {
        if (managerType != typeof(WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>))
            return false;
        NotifyCollectionChangedEventArgs change = (NotifyCollectionChangedEventArgs)e;
        if (ReferenceEquals(sender, DocumentsSource)) _documents.SourceChanged(change);
        else if (ReferenceEquals(sender, AnchorablesSource)) _anchorables.SourceChanged(change);
        return true;
    }

    void IWeakEventSubscriber<NotifyCollectionChangedEventArgs>.OnEvent(object? sender, WeakEvent ev,
        NotifyCollectionChangedEventArgs e)
    {
        if (sender is null || !ReferenceEquals(ev, WeakEvents.CollectionChanged)) return;
        if (!OnReceiveWeakEvent(typeof(WeakEvent<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>), sender, e))
            throw new InvalidOperationException("A source collection event must be handled by OnReceiveWeakEvent.");
    }
}
