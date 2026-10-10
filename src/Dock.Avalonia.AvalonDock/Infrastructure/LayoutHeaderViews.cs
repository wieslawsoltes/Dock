using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// One item may have several headers (for example a tab and an auto-hide strip).
// Weak registrations allow discarded native tab presenters to be collected.
internal sealed class LayoutHeaderViews(LayoutItem item)
{
    private readonly List<WeakReference<LayoutHeaderPresenter>> _views = new();

    public Control Build(LayoutHeaderKind kind, IDataTemplate? fallback, Control? existing)
    {
        if (existing is LayoutHeaderPresenter view && ReferenceEquals(view.Item, item) && view.Kind == kind)
            return view;
        for (int i = _views.Count - 1; i >= 0; i--)
            if (!_views[i].TryGetTarget(out _)) _views.RemoveAt(i);
        view = new LayoutHeaderPresenter { Item = item, Kind = kind, DefaultTemplate = fallback };
        _views.Add(new(view));
        ApplyTemplate(view);
        return view;
    }

    public void Refresh(LayoutHeaderKind kind)
    {
        for (int i = _views.Count - 1; i >= 0; i--)
        {
            if (!_views[i].TryGetTarget(out LayoutHeaderPresenter? view)) _views.RemoveAt(i);
            else if (view.Kind == kind) ApplyTemplate(view);
        }
    }

    public void Detach()
    {
        foreach (WeakReference<LayoutHeaderPresenter> reference in _views)
            if (reference.TryGetTarget(out LayoutHeaderPresenter? view))
            {
                view.Item = null;
                view.ContentTemplate = null;
                view.DefaultTemplate = null;
                view.DataTemplates.Clear();
                view.UpdateChild();
                LayoutItemViewTransfer.Prepare(view, null);
            }
        _views.Clear();
    }

    private void ApplyTemplate(LayoutHeaderPresenter view)
    {
        if (item.Manager is { } manager)
            view.SetCurrentValue(ContentPresenter.ContentTemplateProperty,
                new LayoutHeaderContentTemplate(manager, view.Kind, view.DefaultTemplate, view));
    }
}
