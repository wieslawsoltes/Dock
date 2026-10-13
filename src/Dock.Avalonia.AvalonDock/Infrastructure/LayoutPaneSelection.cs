using System;
using Avalonia.Threading;
using Dock.Model.Avalonia.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal interface ILayoutPaneSelection
{
    void ReconcileSelection();
}

internal sealed class LayoutPaneSelection<T>(LayoutGroup<T> pane, bool documents,
    Action<string?> changing, Action<string?> changed) where T : LayoutContent
{
    private readonly DockBase _model = (DockBase)pane.DockModel;
    private T? _selected;
    private int _index = -1;
    private bool _refreshing;
    private bool _publishingChanges;
    private bool _pending;
    private bool _autoFix = true;

    internal T? Selected { get { EnsureCurrent(); return _selected; } }
    internal int Index { get { EnsureCurrent(); return _index; } }

    private void EnsureCurrent()
    {
        if (_pending && !pane.IsRefreshingNativeChildren) ChildrenChanged();
    }

    private void DeferNativeChange()
    {
        if (_pending) return;
        _pending = true;
        // Native item generators must finish their collection notification before
        // selection can rebuild presentation. A factory completion or a later
        // public read can reconcile first; the queued fallback retains no pane.
        WeakReference<LayoutPaneSelection<T>> reference = new(this);
        Dispatcher.UIThread.Post(() =>
        {
            if (reference.TryGetTarget(out LayoutPaneSelection<T>? selection)) selection.EnsureCurrent();
        });
    }

    internal void Initialize()
    {
        _selected = Resolve(out _index);
    }

    internal void BeginXmlRead() => _autoFix = false;

    internal void CompleteXmlRead()
    {
        // Upstream reenables fallback only after successful restoration. A
        // failed or nested read retains the resulting state of this one flag.
        _autoFix = true;
        ChildrenChanged();
    }

    internal void Select(int index)
    {
        T? selected = index >= 0 && index < pane.Children.Count ? pane.Children[index] : null;
        if (!ReferenceEquals(_model.ActiveDockable, selected?.DockModel))
        {
            if (_model.Factory is LayoutFactory factory) factory.SetPaneSelection(_model, selected?.DockModel);
            else _model.ActiveDockable = selected?.DockModel;
        }
        Refresh();
    }

    internal void ChildrenChanged()
    {
        if (pane.IsRefreshingNativeChildren) { DeferNativeChange(); return; }
        _pending = false;
        if (Resolve(out _) is not null || !_autoFix) { Refresh(); return; }
        int next = -1;
        if (documents && pane.Root is not null)
        {
            DateTime latest = DateTime.MinValue;
            for (int index = 0; index < pane.Children.Count; index++)
            {
                DateTime timestamp = pane.Children[index].LastActivationTimeStamp.GetValueOrDefault();
                if (next < 0 || timestamp > latest) { next = index; latest = timestamp; }
            }
        }
        else
        {
            for (int index = 0; index < pane.Children.Count; index++)
                if (pane.Children[index].IsEnabled) { next = index; break; }
        }
        Select(next);
        if (documents && pane.Root is LayoutRoot root)
        {
            if (_selected is { } selected) selected.IsActive = true;
            else root.ActiveContent = null;
        }
    }

    internal void Refresh()
    {
        if (pane.IsRefreshingNativeChildren) { DeferNativeChange(); return; }
        if (_refreshing && !_publishingChanges) return;
        T? selected = Resolve(out int index);
        bool contentChanged = !ReferenceEquals(_selected, selected);
        bool indexChanged = _index != index;
        if (!contentChanged && !indexChanged) return;
        bool previousRefreshing = _refreshing;
        bool previousPublishing = _publishingChanges;
        _refreshing = true;
        _publishingChanges = false;
        try
        {
            if (indexChanged) changing(nameof(ILayoutContentSelector.SelectedContentIndex));
            if (contentChanged) changing(nameof(ILayoutContentSelector.SelectedContent));
            // A changing observer may redirect selection to a different item.
            selected = Resolve(out index);
            T? previous = _selected;
            _selected = selected;
            _index = index;
            if (previous is not null && !ReferenceEquals(previous, selected)) previous.SetSelectedFromPane(false);
            for (int childIndex = 0; childIndex < pane.Children.Count; childIndex++)
            {
                T child = pane.Children[childIndex];
                if (ReferenceEquals(child.DockModel.Owner, _model))
                    child.SetSelectedFromPane(ReferenceEquals(child, selected));
            }
            _publishingChanges = true;
            if (indexChanged) changed(nameof(ILayoutContentSelector.SelectedContentIndex));
            if (contentChanged) changed(nameof(ILayoutContentSelector.SelectedContent));
        }
        finally { _publishingChanges = previousPublishing; _refreshing = previousRefreshing; }
        // Changed observers can start another selection transition.
        if (!_refreshing) Refresh();
    }

    private T? Resolve(out int index)
    {
        for (index = 0; index < pane.Children.Count; index++)
            if (ReferenceEquals(pane.Children[index].DockModel, _model.ActiveDockable)) return pane.Children[index];
        index = -1;
        return null;
    }
}
