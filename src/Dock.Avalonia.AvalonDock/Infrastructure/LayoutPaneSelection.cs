using System;
using Avalonia.Threading;
using Dock.Model.Avalonia.Core;
using AvalonDock.Layout;

namespace AvalonDock.Infrastructure;

internal interface ILayoutPaneSelection
{
    void ReconcileSelection();
    bool IsRefreshingChildren { get; }
    bool IsMovingChildren { get; }
}

internal sealed class LayoutPaneSelection<T>(LayoutGroup<T> pane, bool documents,
    Action<string?> changing, Action<string?> changed) where T : LayoutContent
{
    private readonly DockBase _model = (DockBase)pane.DockModel;
    private int _index = -1;
    private bool _projecting;
    private int _selecting;
    private bool _pending;
    private bool _autoFix = true;

    internal T? Selected { get { EnsureCurrent(); return _index == -1 ? null : pane.Children[_index]; } }
    internal int Index { get { EnsureCurrent(); return _index; } }

    private void EnsureCurrent()
    {
        if (!_pending || pane.IsRefreshingNativeChildren) return;
        _pending = false;
        // A native collection edit preserves Dock's selected item identity.
        // Public collection edits instead run the pane's index-based policy.
        Refresh();
        ChildrenChanged();
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
        _index = ResolveIndex();
    }

    internal void BeginXmlRead() => _autoFix = false;

    internal void CompleteXmlRead()
    {
        // Upstream reenables fallback only after successful restoration. A
        // failed or nested read retains the resulting state of this one flag.
        _autoFix = true;
        FixSelection();
    }

    internal void Select(int index)
    {
        if (index < 0 || index >= pane.Children.Count) index = -1;
        if (_index == index)
        {
            // Insertion can change the child occupying an unchanged public
            // index. Keep the native presentation aligned without adding public
            // notifications or rewriting any content selection flags.
            if (_selecting == 0) Project();
            return;
        }
        _selecting++;
        try
        {
            changing(nameof(ILayoutContentSelector.SelectedContentIndex));
            changing(nameof(ILayoutContentSelector.SelectedContent));
            SetCurrentFlag(false);
            _index = index;
            SetCurrentFlag(true);
            if (_selecting == 1) Project();
            changed(nameof(ILayoutContentSelector.SelectedContentIndex));
            changed(nameof(ILayoutContentSelector.SelectedContent));
        }
        finally
        {
            // Native presentation needs the completed selection. Publishing
            // the nested clear would close open menus and detach editors.
            if (--_selecting == 0) Project();
        }
    }

    private void SetCurrentFlag(bool selected)
    {
        if (_index >= 0 && _index < pane.Children.Count) pane.Children[_index].IsSelected = selected;
    }

    private void Project()
    {
        T? selected = _index >= 0 && _index < pane.Children.Count ? pane.Children[_index] : null;
        if (ReferenceEquals(_model.ActiveDockable, selected?.DockModel)) return;
        bool previous = _projecting;
        _projecting = true;
        try
        {
            if (_model.Factory is LayoutFactory factory) factory.SetPaneSelection(_model, selected?.DockModel);
            else _model.ActiveDockable = selected?.DockModel;
        }
        finally { _projecting = previous; }
    }

    internal void ChildMoved(int oldIndex, int newIndex)
    {
        if (_index != oldIndex) return;
        changing(nameof(ILayoutContentSelector.SelectedContentIndex));
        _index = newIndex;
        Project();
        changed(nameof(ILayoutContentSelector.SelectedContentIndex));
    }
    internal void ChildrenChanged()
    {
        if (pane.IsRefreshingNativeChildren) { DeferNativeChange(); return; }
        if (_pending) { EnsureCurrent(); return; }
        _pending = false;
        if (documents || _autoFix) FixSelection();
        if (documents) return;
        for (int index = 0; index < pane.Children.Count; index++)
        {
            if (!pane.Children[index].IsSelected) continue;
            Select(index);
            break;
        }
    }

    private void FixSelection()
    {
        if (_index >= pane.Children.Count) Select(pane.Children.Count - 1);
        if (_index == -1) SelectFallback();
    }

    private void SelectFallback()
    {
        int next = -1;
        if (documents && pane.Root is not null)
        {
            if (pane.Children.Count == 0)
            {
                pane.Root.ActiveContent = null;
                return;
            }
            DateTime latest = DateTime.MinValue;
            for (int index = 0; index < pane.Children.Count; index++)
            {
                DateTime timestamp = pane.Children[index].LastActivationTimeStamp.GetValueOrDefault();
                if (next < 0 || timestamp > latest) { next = index; latest = timestamp; }
            }
            T selected = pane.Children[next];
            Select(next);
            // Application callbacks may move this pane or select another item.
            // The fallback still activates the child chosen before the callback.
            selected.IsActive = true;
            return;
        }
        else
        {
            for (int index = 0; index < pane.Children.Count; index++)
                if (pane.Children[index].IsEnabled) { next = index; break; }
        }
        Select(next);
    }

    internal void Refresh()
    {
        if (_projecting || pane.IsMovingNativeChildren) return;
        if (pane.IsRefreshingNativeChildren) { DeferNativeChange(); return; }
        Select(ResolveIndex());
    }

    private int ResolveIndex()
    {
        for (int index = 0; index < pane.Children.Count; index++)
            if (ReferenceEquals(pane.Children[index].DockModel, _model.ActiveDockable)) return index;
        return -1;
    }
}
