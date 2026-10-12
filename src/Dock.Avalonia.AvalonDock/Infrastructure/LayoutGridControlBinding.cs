using System;
using System.Collections.Generic;
using System.ComponentModel;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

// Length policies run during layout, after model collection observers finish.
// Neither a retained model nor an individual child can retain a discarded grid.
internal sealed class LayoutGridControlBinding(LayoutGroupBase model, Action invalidate, Action resize)
{
    private Subscription? _subscription;

    internal void Attach()
    {
        if (_subscription is not null) return;
        _subscription = new(model, this);
        Refresh();
    }

    internal void Detach()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    private void Refresh()
    {
        invalidate();
    }

    private void Resize() => resize();

    private sealed class Subscription : IDisposable
    {
        private readonly WeakReference<LayoutGroupBase> _model;
        private readonly WeakReference<LayoutGridControlBinding> _owner;
        private readonly List<WeakReference<ILayoutElement>> _children = new();

        internal Subscription(LayoutGroupBase model, LayoutGridControlBinding owner)
        {
            _model = new(model);
            _owner = new(owner);
            model.PropertyChanged += ModelChanged;
            model.ChildrenTreeChanged += ChildrenChanged;
            WatchChildren(model);
        }

        private void WatchChildren(LayoutGroupBase model)
        {
            ReleaseChildren();
            if (model is not ILayoutContainer container) return;
            foreach (ILayoutElement child in container.Children)
            {
                _children.Add(new(child));
                child.PropertyChanged += ChildChanged;
            }
        }

        private void ModelChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutGridControlBinding? owner)) { Dispose(); return; }
            if (args.PropertyName is nameof(ILayoutOrientableGroup.Orientation) or nameof(ILayoutElement.Root) or nameof(ILayoutElement.Parent))
                owner.Refresh();
        }

        private void ChildChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutGridControlBinding? owner)) { Dispose(); return; }
            if (args.PropertyName == nameof(ILayoutPanelElement.IsVisible)) owner.Refresh();
            else if (args.PropertyName is "DockWidth" or "DockHeight" or "DockMinWidth" or "DockMinHeight") owner.Resize();
        }

        private void ChildrenChanged(object? sender, ChildrenTreeChangedEventArgs args)
        {
            if (!_owner.TryGetTarget(out LayoutGridControlBinding? owner)) { Dispose(); return; }
            if (args.Change != ChildrenTreeChange.DirectChildrenChanged) return;
            if (_model.TryGetTarget(out LayoutGroupBase? model)) WatchChildren(model);
            owner.Refresh();
        }

        private void ReleaseChildren()
        {
            foreach (WeakReference<ILayoutElement> reference in _children)
                if (reference.TryGetTarget(out ILayoutElement? child)) child.PropertyChanged -= ChildChanged;
            _children.Clear();
        }

        public void Dispose()
        {
            if (_model.TryGetTarget(out LayoutGroupBase? model))
            {
                model.PropertyChanged -= ModelChanged;
                model.ChildrenTreeChanged -= ChildrenChanged;
            }
            ReleaseChildren();
        }
    }
}
