using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Dock.Model.Inpc.Core;

/// <summary>
/// Helper extensions for <see cref="INotifyPropertyChanged"/>.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    /// Creates an observable sequence for property change notifications.
    /// </summary>
    public static IObservable<TValue> WhenPropertyChanged<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> selector)
        where TSource : INotifyPropertyChanged
    {
        return new PropertyChangedObservable<TSource, TValue>(source, selector);
    }
}
