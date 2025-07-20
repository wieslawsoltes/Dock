using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Dock.Model.Inpc.Core;

internal class PropertyChangedObservable<TSource, TValue> : IObservable<TValue>, IDisposable
    where TSource : INotifyPropertyChanged
{
    private readonly TSource _source;
    private readonly string _propertyName;
    private readonly Func<TSource, TValue> _getter;
    private readonly List<IObserver<TValue>> _observers = new();

    public PropertyChangedObservable(TSource source, Expression<Func<TSource, TValue>> selector)
    {
        _source = source;
        _propertyName = ((MemberExpression)selector.Body).Member.Name;
        _getter = selector.Compile();
        _source.PropertyChanged += OnPropertyChanged;
    }

    public IDisposable Subscribe(IObserver<TValue> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
            observer.OnNext(_getter(_source));
        }
        return new Unsubscriber(_observers, observer);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName)
        {
            var value = _getter(_source);
            foreach (var observer in _observers.ToArray())
            {
                observer.OnNext(value);
            }
        }
    }

    public void Dispose()
    {
        _source.PropertyChanged -= OnPropertyChanged;
        _observers.Clear();
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<TValue>> _observers;
        private readonly IObserver<TValue> _observer;

        public Unsubscriber(List<IObserver<TValue>> observers, IObserver<TValue> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}
