using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using AvalonDock.Controls;

namespace AvalonDock.Infrastructure;

// One item may have several headers (a tab, title bar, and auto-hide strip).
// Weak registrations allow discarded native presenters to be collected.
internal sealed class LayoutHeaderViews(LayoutItem item)
{
    private readonly List<WeakReference<LayoutHeaderPresenter>> _views = new();

    public Control Build(LayoutHeaderKind kind, IDataTemplate? fallback, Control? existing)
    {
        if (existing is LayoutHeaderPresenter view && ReferenceEquals(view.Item, item) && view.Kind == kind)
            return view;
        view = new LayoutHeaderPresenter { Item = item, Kind = kind, DefaultTemplate = fallback };
        Attach(view);
        return view;
    }

    internal void Attach(LayoutHeaderPresenter view)
    {
        for (int i = _views.Count - 1; i >= 0; i--)
        {
            if (!_views[i].TryGetTarget(out LayoutHeaderPresenter? existing) || !ReferenceEquals(existing.Item, item))
                _views.RemoveAt(i);
            else if (ReferenceEquals(existing, view)) return;
        }
        _views.Add(new(view));
        ApplyTemplate(view);
    }

    public void Refresh(LayoutHeaderKind kind, bool selectorChanged)
    {
        for (int i = _views.Count - 1; i >= 0; i--)
        {
            if (!_views[i].TryGetTarget(out LayoutHeaderPresenter? view) || !ReferenceEquals(view.Item, item)) _views.RemoveAt(i);
            else if (view.Kind == kind) ApplyTemplate(view, selectorChanged);
        }
    }

    public void Detach()
    {
        foreach (WeakReference<LayoutHeaderPresenter> reference in _views)
            if (reference.TryGetTarget(out LayoutHeaderPresenter? view) && ReferenceEquals(view.Item, item))
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

    private void ApplyTemplate(LayoutHeaderPresenter view, bool? selectorChanged = null)
    {
        if (item.Manager is { } manager)
        {
            LayoutTemplateSettings settings = LayoutTemplateSettings.Header(manager, view.Kind);
            if (view.ContentTemplate is LayoutHeaderContentTemplate previous)
            {
                settings = previous.Settings.Update(settings, selectorChanged);
                if (previous.Settings.Matches(settings) && ReferenceEquals(previous.Fallback, view.DefaultTemplate)) return;
            }
            view.SetCurrentValue(ContentPresenter.ContentTemplateProperty,
                new LayoutHeaderContentTemplate(settings, view.DefaultTemplate, view));
        }
    }
}
