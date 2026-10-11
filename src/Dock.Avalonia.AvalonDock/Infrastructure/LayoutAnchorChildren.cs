using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Projects the public group order onto reusable controls without changing Dock's
// flat pinned collections. The model publisher must not own its generated views.
internal sealed class LayoutAnchorChildren<TModel, TControl>
    where TModel : class, ILayoutElement
    where TControl : Control, ILayoutControl
{
    private readonly ObservableCollection<TModel> _models;
    private readonly ObservableCollection<TControl> _views = new();
    private readonly Func<TModel, TControl> _create;
    private bool _updating;
    private bool _pending;

    internal ObservableCollection<TControl> Views => _views;

    internal LayoutAnchorChildren(ObservableCollection<TModel> models, Func<TModel, TControl> create)
    {
        _models = models; _create = create;
        _ = new Subscription(models, this);
        Refresh();
    }

    private void Refresh()
    {
        if (_updating) { _pending = true; return; }
        _updating = true;
        try
        {
            do
            {
                _pending = false;
                for (int i = 0; i < _models.Count; i++)
                {
                    TModel model = _models[i];
                    if (i < _views.Count && ReferenceEquals(_views[i].Model, model)) continue;
                    int found = -1;
                    for (int j = i + 1; j < _views.Count; j++)
                        if (ReferenceEquals(_views[j].Model, model)) { found = j; break; }
                    if (found >= 0) _views.Move(found, i);
                    else _views.Insert(i, _create(model));
                }
                while (_views.Count > _models.Count) _views.RemoveAt(_views.Count - 1);
            } while (_pending);
        }
        finally { _updating = false; }
    }

    private sealed class Subscription
    {
        private readonly WeakReference<LayoutAnchorChildren<TModel, TControl>> _owner;
        internal Subscription(ObservableCollection<TModel> models, LayoutAnchorChildren<TModel, TControl> owner)
        {
            _owner = new(owner);
            models.CollectionChanged += Changed;
        }
        private void Changed(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (_owner.TryGetTarget(out LayoutAnchorChildren<TModel, TControl>? owner)) owner.Refresh();
            else if (sender is INotifyCollectionChanged source) source.CollectionChanged -= Changed;
        }
    }
}
