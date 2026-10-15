using System;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace AvalonDock.Infrastructure;

// Dock may build the destination before its old deferred presenter unloads.
// Transfer only the adapter's presenter; its content and native model stay put.
internal static class LayoutItemViewTransfer
{
    public static Control? Prepare(ContentPresenter view, Control? existing)
    {
        if (ReferenceEquals(view, existing)) return view;
        TopLevel? previousRoot = TopLevel.GetTopLevel(view);
        // The public View remains logically owned by its manager. Transfer
        // through its physical host, which may be an application panel.
        var parent = view.GetVisualParent() ?? view.Parent;
        if (parent is ContentPresenter previous && ReferenceEquals(previous.Child, view))
        {
            previous.SetCurrentValue(ContentPresenter.ContentProperty, null);
            previous.UpdateChild();
        }
        else if (parent is Panel panel) panel.Children.Remove(view);
        else if (parent is Decorator decorator && ReferenceEquals(decorator.Child, view)) decorator.Child = null;
        // Retire invalidations queued by the old host before attaching the
        // presenter to a different layout manager.
        if (view.GetVisualParent() is null) previousRoot?.UpdateLayout();
        return view.GetVisualParent() is null ? view : existing;
    }
}
