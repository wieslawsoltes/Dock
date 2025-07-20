using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Model.Internal;

namespace Dock.Model;

/// <summary>
/// Factory base class.
/// </summary>
public abstract partial class FactoryBase
{
    private readonly SimpleSubject<DockablePropertyChangedEventArgs> _dockableChangedSubject = new();
    private readonly Dictionary<IDockable, PropertyChangedEventHandler> _subscriptions = new();

    /// <summary>
    /// Observable stream of dockable property changes.
    /// </summary>
    public IObservable<DockablePropertyChangedEventArgs> DockableChanged => _dockableChangedSubject;

    private void SubscribeDockable(IDockable dockable)
    {
        if (dockable is INotifyPropertyChanged notify && !_subscriptions.ContainsKey(dockable))
        {
            PropertyChangedEventHandler handler = (_, e) =>
                _dockableChangedSubject.OnNext(new DockablePropertyChangedEventArgs(dockable, e.PropertyName));
            notify.PropertyChanged += handler;
            _subscriptions[dockable] = handler;
        }
    }

    private void UnsubscribeDockable(IDockable dockable)
    {
        if (dockable is INotifyPropertyChanged notify && _subscriptions.TryGetValue(dockable, out var handler))
        {
            notify.PropertyChanged -= handler;
            _subscriptions.Remove(dockable);
        }
    }
}
