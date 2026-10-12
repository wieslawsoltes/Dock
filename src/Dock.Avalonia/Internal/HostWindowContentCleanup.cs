// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Controls.DeferredContentControl;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Avalonia.Internal;

internal static class HostWindowContentCleanup
{
    internal static void ReleaseModels(Control host, IDock? layout)
    {
        // Snapshot first: clearing a presenter can remove its descendants.
        List<ContentControl> presenters = new();
        foreach (var visual in host.GetVisualDescendants())
        {
            if (visual is ContentControl { Content: IDockable } presenter)
                presenters.Add(presenter);
        }
        // A template replaced earlier can still be the model's logical parent,
        // even though its presenter is no longer in the window's visual tree.
        if (layout is not null)
            CollectModelPresenters(layout, host, presenters, new HashSet<IDockable>(ReferenceEqualityComparer.Instance));

        for (int i = presenters.Count - 1; i >= 0; i--)
        {
            ContentControl presenter = presenters[i];
            presenter.Content = null;
            if (presenter is DeferredContentControl deferred)
                deferred.ClearPresentation();
        }
    }

    private static void CollectModelPresenters(IDockable model, Control host, List<ContentControl> presenters,
        HashSet<IDockable> visited)
    {
        if (!visited.Add(model)) return;
        if (model is ILogical { LogicalParent: ContentControl parent } &&
            !ReferenceEquals(parent, host) && ReferenceEquals(parent.Content, model) &&
            (parent.GetVisualRoot() is not { } root || ReferenceEquals(root, host)) && !presenters.Contains(parent))
            presenters.Add(parent);

        if (model is IDock dock) Collect(dock.VisibleDockables, host, presenters, visited);
        if (model is IRootDock rootDock)
        {
            Collect(rootDock.HiddenDockables, host, presenters, visited);
            Collect(rootDock.LeftPinnedDockables, host, presenters, visited);
            Collect(rootDock.RightPinnedDockables, host, presenters, visited);
            Collect(rootDock.TopPinnedDockables, host, presenters, visited);
            Collect(rootDock.BottomPinnedDockables, host, presenters, visited);
            if (rootDock.PinnedDock is { } pinned) CollectModelPresenters(pinned, host, presenters, visited);
        }
        if (model is ISplitViewDock split)
        {
            if (split.PaneDockable is { } pane) CollectModelPresenters(pane, host, presenters, visited);
            if (split.ContentDockable is { } content) CollectModelPresenters(content, host, presenters, visited);
        }
    }

    private static void Collect(IList<IDockable>? models, Control host, List<ContentControl> presenters,
        HashSet<IDockable> visited)
    {
        if (models is null) return;
        for (int i = 0; i < models.Count; i++)
            CollectModelPresenters(models[i], host, presenters, visited);
    }
}
