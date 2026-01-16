// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model.Adapters;

/// <summary>
/// Helper methods for overlay dock bookkeeping shared across model implementations.
/// </summary>
public class OverlayAdapter : IOverlayAdapter
{
    /// <summary>
    /// Gets the optional background dockable from the visible list.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <returns>Background dockable or null.</returns>
    public IDockable? GetBackground(IOverlayDock overlayDock)
    {
        return overlayDock.VisibleDockables is { Count: > 0 } list && list[0] is not IOverlayPanel
            ? list[0]
            : null;
    }

    /// <summary>
    /// Gets overlay panels from the dock by skipping the optional background item.
    /// Panels that belong to a splitter group are excluded.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <returns>Panels list or null if none.</returns>
    public IList<IOverlayPanel>? GetOverlayPanels(IOverlayDock overlayDock)
    {
        var visible = overlayDock.VisibleDockables;
        if (visible is null || visible.Count == 0)
        {
            return null;
        }

        var startIndex = visible[0] is IOverlayPanel ? 0 : 1;
        if (visible.Count <= startIndex)
        {
            return null;
        }

        var result = new List<IOverlayPanel>();
        for (var i = startIndex; i < visible.Count; i++)
        {
            if (visible[i] is IOverlayPanel panel)
            {
                if (panel.SplitterGroup is null)
                {
                    result.Add(panel);
                }
            }
        }

        return result.Count == 0 ? null : result;
    }

    /// <summary>
    /// Sets the background dockable and keeps existing panels.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="background">The background dockable.</param>
    /// <param name="listFactory">Factory used to create the visible list.</param>
    public void SetBackground(IOverlayDock overlayDock, IDockable? background, Func<IList<IDockable>> listFactory)
    {
        UpdateVisibleDockables(overlayDock, background, GetOverlayPanels(overlayDock), listFactory);
    }

    /// <summary>
    /// Sets overlay panels and keeps the existing background item.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="panels">The panels to set.</param>
    /// <param name="listFactory">Factory used to create the visible list.</param>
    public void SetPanels(IOverlayDock overlayDock, IList<IOverlayPanel>? panels, Func<IList<IDockable>> listFactory)
    {
        UpdateVisibleDockables(overlayDock, GetBackground(overlayDock), panels, listFactory);
    }

    /// <summary>
    /// Assigns the overlay dock as the owner of each splitter group.
    /// </summary>
    /// <param name="overlayDock">The overlay dock instance.</param>
    /// <param name="splitterGroups">The splitter groups to update.</param>
    public void SetSplitterGroupsOwner(IOverlayDock overlayDock, IList<IOverlaySplitterGroup>? splitterGroups)
    {
        if (splitterGroups is null)
        {
            return;
        }

        foreach (var group in splitterGroups)
        {
            if (group is null)
            {
                continue;
            }

            group.Owner = overlayDock;
        }

        if (overlayDock.VisibleDockables is { } visible)
        {
            foreach (var group in splitterGroups)
            {
                if (group?.Panels is null)
                {
                    continue;
                }

                foreach (var panel in group.Panels)
                {
                    if (panel is null || visible.Contains(panel))
                    {
                        continue;
                    }

                    panel.Owner = overlayDock;
                    visible.Add(panel);
                }
            }
        }
    }

    /// <inheritdoc/>
    public void SetOverlayPanelContent(IOverlayPanel panel, IDockable? content, Func<IList<IDockable>> listFactory)
    {
        if (listFactory is null)
        {
            throw new ArgumentNullException(nameof(listFactory));
        }

        var list = panel.VisibleDockables;
        if (list is null)
        {
            if (content is null)
            {
                return;
            }

            content.Owner = panel;
            list = listFactory();
            list.Add(content);
            panel.VisibleDockables = list;
            return;
        }

        if (list.Count == 0)
        {
            if (content is null)
            {
                return;
            }

            content.Owner = panel;
            list.Add(content);
            return;
        }

        if (content is null)
        {
            list.RemoveAt(0);
            return;
        }

        content.Owner = panel;
        list[0] = content;
    }

    /// <summary>
    /// Updates panel back-references when the panel collection changes.
    /// </summary>
    /// <param name="splitterGroup">The owning splitter group.</param>
    /// <param name="previous">Panels previously attached to the group.</param>
    /// <param name="next">Panels being assigned to the group.</param>
    public void UpdateGroupPanels(IOverlaySplitterGroup splitterGroup, IList<IOverlayPanel>? previous, IList<IOverlayPanel>? next)
    {
        if (previous is not null)
        {
            foreach (var panel in previous)
            {
                if (panel is null)
                {
                    continue;
                }

                if (ReferenceEquals(panel.SplitterGroup, splitterGroup))
                {
                    panel.SplitterGroup = null;
                }
            }
        }

        if (next is null)
        {
            return;
        }

        foreach (var panel in next)
        {
            if (panel is null)
            {
                continue;
            }

            panel.SplitterGroup = splitterGroup;
        }
    }

    /// <summary>
    /// Assigns the splitter group owner to each splitter.
    /// </summary>
    /// <param name="splitterGroup">The owning splitter group.</param>
    /// <param name="splitters">Splitters to update.</param>
    public void UpdateGroupSplitters(IOverlaySplitterGroup splitterGroup, IList<IOverlaySplitter>? splitters)
    {
        if (splitters is null)
        {
            return;
        }

        foreach (var splitter in splitters)
        {
            if (splitter is null)
            {
                continue;
            }

            splitter.Owner = splitterGroup;
        }
    }

    private void UpdateVisibleDockables(
        IOverlayDock overlayDock,
        IDockable? background,
        IList<IOverlayPanel>? panels,
        Func<IList<IDockable>> listFactory)
    {
        if (listFactory is null)
        {
            throw new ArgumentNullException(nameof(listFactory));
        }

        var newVisible = listFactory();

        if (background is not null)
        {
            background.Owner = overlayDock;
            newVisible.Add(background);
        }

        var seenPanels = new HashSet<IOverlayPanel>();

        if (panels is not null)
        {
            foreach (var panel in panels)
            {
                if (panel is null || !seenPanels.Add(panel))
                {
                    continue;
                }

                panel.Owner = overlayDock;
                newVisible.Add(panel);
            }
        }

        if (overlayDock.SplitterGroups is not null)
        {
            foreach (var group in overlayDock.SplitterGroups)
            {
                if (group?.Panels is null)
                {
                    continue;
                }

                foreach (var panel in group.Panels)
                {
                    if (panel is null || !seenPanels.Add(panel))
                    {
                        continue;
                    }

                    panel.Owner = overlayDock;
                    newVisible.Add(panel);
                }
            }
        }

        overlayDock.VisibleDockables = newVisible;
    }
}
