// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Dock.Model.Inpc.Core;

/// <summary>
/// Helper extensions for <see cref="INotifyPropertyChanged"/>.
/// </summary>
internal static class ObservableExtensions
{
    /// <summary>
    /// Creates an observable sequence for property change notifications.
    /// </summary>
    public static IObservable<TValue> WhenPropertyChanged<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> selector)
        where TSource : INotifyPropertyChanged
    {
        return new PropertyChangedObservable<TSource, TValue>(source, selector);
    }

    /// <summary>
    /// Subscribes to the observable sequence using a simple <see cref="Action{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the observable sequence elements.</typeparam>
    /// <param name="source">The observable source.</param>
    /// <param name="onNext">Callback invoked when a value is produced.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (onNext == null)
        {
            throw new ArgumentNullException(nameof(onNext));
        }

        return source.Subscribe(new DelegateObserver<T>(onNext));
    }

    private sealed class DelegateObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;

        public DelegateObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }
    }
}
